using System;
using System.Collections.Generic;
using System.Linq;

public static class ShuffleIEnumerable
{
    public static IList<T> LinearShuffle<T>(this IEnumerable<T> items)
    {
        Random random = new Random();

        // make sure the overloaded list is not null
        if (items == null) { throw new ArgumentException("items must not be null"); }

        IList<T> results;

        // make sure the overloaded list is not empty
        if (items?.Count() > 0)
        {
            // fix the order of the given elements by converting the enumerable to a list
            var tempItems = items.ToArray();

            // shuffle all elements (=> linear shuffle with equal distribution)
            for (int i = 0; i < tempItems.Length - 1; i++)
            {
                // get index to switch with
                int k = (int)(random.Next() >> 1) % tempItems.Length;

                // check if element needs to switch (same index => avoid switching with itself)
                if (i != k)
                {
                    // switch position: results[k] <--> results[i]
                    T value = tempItems[k];
                    tempItems[k] = tempItems[i];
                    tempItems[i] = value;
                }
            }

            results = tempItems.ToList();
        }
        else { results = new List<T>(); }

        return results;
    }

    
    public static IEnumerable<T> YieldShuffle<T>(this IEnumerable<T> items)
    {
        Random random = new Random();

        // make sure the overloaded list is not null
        if (items == null) { throw new ArgumentException("items must not be null"); }

        // make sure the overloaded list is not empty
        if (items?.Count() > 0)
        {
            // fix the order of the given elements by converting the enumerable to a list
            var tempItems = items.ToArray();

            // shuffle all elements (=> linear shuffle with equal distribution)
            for (int i = 0; i < tempItems.Length - 1; i++)
            {
                // get index to switch with
                int k = (int)(random.Next() >> 1) % tempItems.Length;

                // check if element needs to switch (same index => avoid switching with itself)
                if (i != k)
                {
                    // switch position: results[k] <--> results[i]
                    T value = tempItems[k];
                    tempItems[k] = tempItems[i];
                    tempItems[i] = value;
                }

                yield return tempItems[i];
            }
        }
    }
}