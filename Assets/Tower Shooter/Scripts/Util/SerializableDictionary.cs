using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializableDictionary<TKey, TValue> : ISerializationCallbackReceiver
{
    [Serializable]
    private struct Entry
    {
        public TKey key;
        public TValue value;

        public Entry(TKey key, TValue value)
        {
            this.key = key;
            this.value = value;
        }
    }

    [SerializeField]
    private List<Entry> entries = new();

    private Dictionary<TKey, TValue> dictionary = new();

    public TValue this[TKey key]
    {
        get => dictionary[key];
        set => dictionary[key] = value;
    }

    public int Count => dictionary.Count;

    public bool TryGetValue(TKey key, out TValue value)
        => dictionary.TryGetValue(key, out value);

    public bool ContainsKey(TKey key)
        => dictionary.ContainsKey(key);

    public bool Remove(TKey key)
        => dictionary.Remove(key);

    public void OnBeforeSerialize()
    {
        
    }

    public void OnAfterDeserialize()
    {
        
    }
}
