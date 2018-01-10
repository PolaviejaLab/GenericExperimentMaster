/**
 * Dictionary that fires an event when items are being inserted or deleted.
 *
 * Copyright (c) 2015-2016 Ivar Clemens for Champalimaud Centre for the Unknown, Lisbon
 */
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


/**
 * Dictionary that fires an event when items are being inserted or deleted.
 */
public class ICDictionary<TKey, TValue>: IDictionary<TKey, TValue>
{    
    public UnityEvent OnChanged = new UnityEvent();

    private Dictionary<TKey, TValue> items = new Dictionary<TKey, TValue>();



    private void InvokeOnChanged() 
    {
        OnChanged.Invoke();
    }

    /*******************************
     * IDictionary<string, string> *
     *******************************/    

    public int Count { get { return items.Count; } }
    public TValue this[TKey key] { get { return items[key]; } set { items[key] = value; InvokeOnChanged(); } }
    public ICollection<TKey> Keys { get { return items.Keys; } }
    public ICollection<TValue> Values { get { return items.Values; } }

    //public void Add(string item) { throw NotSupportedException(); }
    public void Add(TKey key, TValue value) { 
        items.Add(key, value); 
        InvokeOnChanged();
    }

    public void Clear() { items.Clear(); InvokeOnChanged(); }
    public bool ContainsKey(TKey key) { return items.ContainsKey(key); }
    public bool Remove(TKey key) { bool result = items.Remove(key); InvokeOnChanged(); return result; }
    public bool TryGetValue(TKey key, out TValue value) { return items.TryGetValue(key, out value); }

    /*******************************************
     * ICollection<KeyValuePair<TKey, TValue>> *
     *******************************************/

    public bool IsReadOnly { get { 
        ICollection<KeyValuePair<TKey, TValue>> _items = (ICollection<KeyValuePair<TKey, TValue>>) items;
        return _items.IsReadOnly; } }

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> value) { 
        ICollection<KeyValuePair<TKey, TValue>> _items = (ICollection<KeyValuePair<TKey, TValue>>) items;
        _items.Add(value); InvokeOnChanged(); 
    }

    public bool Contains(KeyValuePair<TKey, TValue> value) { 
        ICollection<KeyValuePair<TKey, TValue>> _items = (ICollection<KeyValuePair<TKey, TValue>>) items;
        return _items.Contains(value); 
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) { 
        ICollection<KeyValuePair<TKey, TValue>> _items = (ICollection<KeyValuePair<TKey, TValue>>) items;
        _items.CopyTo(array, arrayIndex); 
    }

    public bool Remove(KeyValuePair<TKey, TValue> value) { 
        ICollection<KeyValuePair<TKey, TValue>> _items = (ICollection<KeyValuePair<TKey, TValue>>) items;
        bool result = _items.Remove(value); InvokeOnChanged(); return result; 
    }

    /*******************************************
     * IEnumerable<KeyValuePair<TKey, TValue>> *
     *******************************************/

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() { return items.GetEnumerator(); }
    IEnumerator IEnumerable.GetEnumerator() { return items.GetEnumerator(); }
}
