using System;
using System.Collections.Generic;

public class ListWithDuplicates<TKey, TValue> : List<KeyValuePair<TKey, TValue>> where TKey  : System.IEquatable<TKey>
{
    public void Add(TKey key, TValue value)
    {
        var element = new KeyValuePair<TKey, TValue>(key, value);
        this.Add(element);
    }

    public List<TValue> GetValues(TKey key)
    {
        List<TValue> toReturn = new();
        foreach(KeyValuePair<TKey, TValue> keyValuePair in this)
        {
            if (keyValuePair.Key.Equals(key))
                toReturn.Add(keyValuePair.Value);
        }
        return toReturn;
    }

    internal void RemoveAllValuesForKey(TKey key)
    {
        foreach (KeyValuePair<TKey, TValue> keyValuePair in this.ToArray())
        {
            if (keyValuePair.Key.Equals(key))
                this.Remove(keyValuePair);
        }
    }
}