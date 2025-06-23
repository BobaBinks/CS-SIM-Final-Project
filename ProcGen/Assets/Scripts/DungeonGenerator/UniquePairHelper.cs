using UnityEngine;
using System.Linq;
using System.Collections.Generic;
public class UniquePairHelper
{
    public static bool CheckPairUnique<T>(List<KeyValuePair<T,T>> pairs, KeyValuePair<T,T> pair)
    {
        if(pairs == null || pair.Key == null || pair.Value == null) return false;

        foreach(var p in pairs)
        {
            if (PairExist(p, pair))
                return false;
        }

        return true;
    }

    private static bool PairExist<T>(KeyValuePair<T,T> a, KeyValuePair<T,T> b)
    {
        // checks the different possible orders of having the same pairs
        return (EqualityComparer<T>.Default.Equals(a.Key,b.Key) && EqualityComparer<T>.Default.Equals(a.Value, b.Value)) ||
               (EqualityComparer<T>.Default.Equals(a.Key, b.Value) && EqualityComparer<T>.Default.Equals(a.Value, b.Key));
    }
}
