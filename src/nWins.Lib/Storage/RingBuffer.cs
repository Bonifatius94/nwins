using System;
using System.Collections.Generic;
using System.Linq;

public class RingBuffer<T> : Queue<T>
{
    public RingBuffer(int size)
        => this.size = size;

    private int size = 0;

    public void AddItem(T item)
    {
        if (Count == size)
            Dequeue();
        Enqueue(item);
    }

    private static Random rng = new Random();

    public IEnumerable<T> RandBatch(int batchSize)
    {
        var uniqueIndices = new HashSet<int>();
        while (uniqueIndices.Count < batchSize)
            uniqueIndices.Add(rng.Next() % this.Count);
        return uniqueIndices.Select(i => this.ElementAt(i));
    }
}
