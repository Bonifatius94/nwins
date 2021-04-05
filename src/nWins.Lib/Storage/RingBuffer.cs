using System;
using System.Collections.Generic;
using System.Linq;

public class RingBuffer<T> : System.Collections.Generic.Queue<T>
{
    public RingBuffer(int size) { this.size = size; }

    private int size = 0;

    public void AddItem(T item)
    {
        // if memory is full, forget oldest item
        if (Count == size) { Dequeue(); }

        // add new item as youngest
        Enqueue(item);
    }

    public IEnumerable<T> RandBatch(int batchSize)
    {
        // generate a random index permutation and select items using indices
        ///var indices = Enumerable.Range(0, this.Count).LinearShuffle().Take(batchSize);
        var indices = Enumerable.Range(0, this.Count).YieldShuffle().Take(batchSize);
        //var indices = randomIndices(batchSize);
        return indices.Select(x => this.ElementAt(x)).ToList();
    }

    private List<int> randomIndices(int batchSize) {
        Random random = new Random();
        List<int> indices = new List<int>();
        for(int i = 0; i < batchSize; i++){
            int randomInt = random.Next() % Count;
            indices.Add(randomInt);
        }
        return indices;
    }
}