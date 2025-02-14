using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Phaeyz.Collections;

/// <summary>
/// A key and value pair collection where only one instance of the key may exist, and items are ordered like an array or a list.
/// </summary>
/// <typeparam name="TKey">
/// The type of the keys in the collection.
/// </typeparam>
/// <typeparam name="TValue">
/// The type of the values in the collection.
/// </typeparam>
/// <remarks>
/// This collection differs from a dictionary in that the order of items stored internally may be preserved and determined, much like a list.
/// The elements may be iterated in order they are stored. Much like a dictionary, additions and lookups are very fast.
/// </remarks>
public class KeyedCollection<TKey, TValue> : ICollection<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue> where TKey: notnull
{
    private readonly IEqualityComparer<TKey> _comparer;
    private readonly Dictionary<TKey, TValue> _dictionary;
    private readonly List<KeyValuePair<TKey, TValue>> _list;

    /// <summary>
    /// Initializes a new instance of the collection that is empty.
    /// </summary>
    public KeyedCollection() : this(0, null) { }

    /// <summary>
    /// Initializes a new instance of the collection that is empty, having the specified initial capacity.
    /// </summary>
    /// <param name="capacity">
    /// The initial number of elements that the collection may contain without needing to grow internal buffers.
    /// </param>
    public KeyedCollection(int capacity) : this(capacity, null) { }

    /// <summary>
    /// Initializes a new instance of the collection that is empty, and uses a specified comparer.
    /// </summary>
    /// <param name="comparer">
    /// The comparer to use when comparing keys, or <c>null</c> to use the default comparer for the type of the key.
    /// </param>
    public KeyedCollection(IEqualityComparer<TKey>? comparer) : this(0, comparer) { }

    /// <summary>
    /// Initializes a new instance of the collection that is empty, having the specified initial capacity, and uses a specified comparer.
    /// </summary>
    /// <param name="capacity">
    /// The initial number of elements that the collection may contain without needing to grow internal buffers.
    /// </param>
    /// <param name="comparer">
    /// The comparer to use when comparing keys, or <c>null</c> to use the default comparer for the type of the key.
    /// </param>
    public KeyedCollection(int capacity, IEqualityComparer<TKey>? comparer)
    {
        _list = new List<KeyValuePair<TKey, TValue>>(capacity);
        _dictionary = new Dictionary<TKey, TValue>(capacity, comparer);
        _comparer = _dictionary.Comparer;
    }

    /// <summary>
    /// Initializes a new instance of the collection that contains the items copied from the specified dictionary.
    /// </summary>
    /// <param name="dictionary">
    /// The enumerable whose items are copied to the new collection.
    /// </param>
    public KeyedCollection(IDictionary<TKey, TValue> dictionary) : this(dictionary, null) { }

    /// <summary>
    /// Initializes a new instance of the collection that contains the items copied from the specified enumerable,.
    /// </summary>
    /// <param name="collection">
    /// The enumerable whose items are copied to the new collection.
    /// </param>
    public KeyedCollection(IEnumerable<KeyValuePair<TKey, TValue>> collection) : this(collection, null) { }

    /// <summary>
    /// Initializes a new instance of the collection that contains the items copied from the specified enumerable, and uses the specified comparer.
    /// </summary>
    /// <param name="collection">
    /// The enumerable whose items are copied to the new collection.
    /// </param>
    /// <param name="comparer">
    /// The comparer implementation to use when comparing keys, or <c>null</c> to use the default comparer for the type of the key.
    /// </param>
    public KeyedCollection(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey>? comparer) :
        this((collection as ICollection<KeyValuePair<TKey, TValue>>)?.Count ?? 0, comparer)
    {
        ArgumentNullException.ThrowIfNull(collection);
        AddRange(collection);
    }

    /// <summary>
    /// Gets the number of items contained in the collection.
    /// </summary>
    public int Count => _list.Count;

    /// <summary>
    /// Gets a value indicating whether the collection is read-only.
    /// </summary>
    public bool IsReadOnly => false;

    /// <summary>
    /// Gets a collection object containing the keys in the keyed collection.
    /// </summary>
    public ICollection<TKey> Keys => _dictionary.Keys;

    /// <summary>
    /// Gets a collection object containing the values in the keyed collection.
    /// </summary>
    public ICollection<TValue> Values => _dictionary.Values;

    /// <summary>
    /// Gets or sets the item with the specified key.
    /// </summary>
    /// <param name="key">
    /// The key of the item to get or set.
    /// </param>
    /// <returns>
    /// The item with the specified key, or null if the key does not exist.
    /// </returns>
    public TValue this[TKey key]
    {
        get => _dictionary[key];
        set
        {
            if (_dictionary.TryAdd(key, value))
            {
                _list.Add(new KeyValuePair<TKey, TValue>(key, value));
                return;
            }

            _dictionary[key] = value;
            _list[IndexOf(key)] = new KeyValuePair<TKey, TValue>(key, value);
        }
    }

    /// <summary>
    /// Adds a key and value pair to the end of the collection.
    /// </summary>
    /// <param name="item">
    /// The key and value pair to add to the end of the collection.
    /// </param>
    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

    /// <summary>
    /// Adds a key and value pair to the end of the collection.
    /// </summary>
    /// <param name="key">
    /// The key of the item to add.
    /// </param>
    /// <param name="value">
    /// The value of the item to add.
    /// </param>
    /// <exception cref="System.ArgumentException">
    /// An item with the same key already exists.
    /// </exception>
    public void Add(TKey key, TValue value)
    {
        if (_dictionary.ContainsKey(key))
        {
            throw new ArgumentException("An item with the same key already exists.", nameof(key));
        }

        _dictionary.Add(key, value);
        _list.Add(new KeyValuePair<TKey, TValue>(key, value));
    }

    /// <summary>
    /// Adds all items in the specified enumerable to the collection.
    /// </summary>
    /// <param name="enumerable">
    /// An enumerable whose items are added to the collection.
    /// </param>
    public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> enumerable)
    {
        // We special-case KVP<>[] and List<KVP<>>, as they're commonly used to seed dictionaries, and
        // we want to avoid the enumerator costs (e.g. allocation) for them as well. Extract a span if possible.
        ReadOnlySpan<KeyValuePair<TKey, TValue>> span;
        if (enumerable is KeyValuePair<TKey, TValue>[] array)
        {
            span = array;
        }
        else if (enumerable.GetType() == typeof(List<KeyValuePair<TKey, TValue>>))
        {
            span = CollectionsMarshal.AsSpan((List<KeyValuePair<TKey, TValue>>)enumerable);
        }
        else
        {
            // Fallback path for all other enumerables
            foreach (KeyValuePair<TKey, TValue> pair in enumerable)
            {
                Add(pair.Key, pair.Value);
            }
            return;
        }

        // We got a span. Add the elements to the dictionary.
        EnsureCapacity(span.Length);

        foreach (KeyValuePair<TKey, TValue> pair in span)
        {
            Add(pair.Key, pair.Value);
        }
    }

    /// <summary>
    /// Removes all items from the collection.
    /// </summary>
    public void Clear()
    {
        _dictionary.Clear();
        _list.Clear();
    }

    /// <summary>
    /// Determines whether the collection contains a specific key and value pair. Both the key and value must match.
    /// </summary>
    /// <param name="item">
    /// The key and value pair to locate in the collection.
    /// </param>
    /// <returns>
    /// <c>true</c> if the key and value pair are found in the collection; otherwise, <c>false</c>.
    /// </returns>
    public bool Contains(KeyValuePair<TKey, TValue> item) =>
        _dictionary.TryGetValue(item.Key, out TValue? value) && EqualityComparer<TValue>.Default.Equals(value, item.Value);

    /// <summary>
    /// Determines whether the collection contains an item with the specified key.
    /// </summary>
    /// <param name="key">
    /// They key to locate in the collection.
    /// </param>
    /// <returns>
    /// <c>true</c> if the collection contains an item with the key; otherwise, <c>false</c>.
    /// </returns>
    public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

    /// <summary>
    /// Copies the items of the collection to an array starting at a particular array index.
    /// </summary>
    /// <param name="array">
    /// The one-dimensional array that is the destination of the items copied from the collection. The array
    /// must have zero-based indexing.
    /// </param>
    /// <param name="arrayIndex">
    /// The zero-based index in the destination array at which copying begins.
    /// </param>
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

    /// <summary>
    /// Copies the items of the collection to an array starting at a particular array index.
    /// </summary>
    /// <param name="index">
    /// The zero-based index within the current collection at which copying begins.
    /// </param>
    /// <param name="array">
    /// The one-dimensional array that is the destination of the items copied from the collection. The array
    /// must have zero-based indexing.
    /// </param>
    /// <param name="arrayIndex">
    /// The zero-based index in the destination array at which copying begins.
    /// </param>
    /// <param name="count">
    /// The total number of elements to copy.
    /// </param>
    public void CopyTo(
        int index,
        KeyValuePair<TKey, TValue>[] array,
        int arrayIndex,
        int count) => _list.CopyTo(index, array, arrayIndex, count);

    /// <summary>
    /// Ensures that the collection can hold up to a specified number of entries without any further expansion of its backing storage.
    /// </summary>
    /// <param name="capacity">
    /// The number of items.
    /// </param>
    /// <returns>
    /// The current capacity of the collection.
    /// </returns>
    public int EnsureCapacity(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        return Math.Min(_dictionary.EnsureCapacity(capacity), _list.EnsureCapacity(capacity));
    }

    /// <summary>
    /// Gets the item at the specified index.
    /// </summary>
    /// <param name="index">
    /// The zero-based index of the item to get.
    /// </param>
    public KeyValuePair<TKey, TValue> GetAt(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _list.Count);
        return _list[index];
    }

    /// <summary>
    /// Returns an enumerator which may be used to iterate all key and value pair items in the collection.
    /// </summary>
    /// <returns>
    /// An enumerator which may be used to iterate all key and value pair items in the collection.
    /// </returns>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _list.GetEnumerator();

    /// <summary>
    /// Returns an enumerator which may be used to iterate all key and value pair items in the collection.
    /// </summary>
    /// <returns>
    /// An enumerator which may be used to iterate all key and value pair items in the collection.
    /// </returns>
    IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();

    /// <summary>
    /// Determines the index of the specified key in the collection.
    /// </summary>
    /// <param name="key">
    /// The key to get the index from the collection.
    /// </param>
    /// <returns>
    /// The index of the item if found in the collection; otherwise, -1.
    /// </returns>
    public int IndexOf(TKey key) => _list.FindIndex(kvp => _comparer.Equals(key, kvp.Key));

    /// <summary>
    /// Insert an item to the collection at the specified index.
    /// </summary>
    /// <param name="index">
    /// The zero-based index at which the item should be inserted.
    /// </param>
    /// <param name="item">
    /// The item to insert into the collection.
    /// </param>
    public void Insert(int index, KeyValuePair<TKey, TValue> item)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, _list.Count);

        if (_dictionary.ContainsKey(item.Key))
        {
            throw new ArgumentException("An item with the same key already exists.", nameof(item));
        }

        _list.Insert(index, item);
        _dictionary.Add(item.Key, item.Value);
    }

    /// <summary>
    /// Removes the first occurrence of a specific key and value pair from the collection. Both the key and value must match.
    /// </summary>
    /// <param name="item">
    /// The key and value pair to remove from the collection.
    /// </param>
    /// <returns>
    /// <c>true</c> if the item was successfully removed from the collection; otherwise <c>false</c>. This method also returns <c>false</c> if
    /// the key and value pair is not found in the original collection.
    /// </returns>
    public bool Remove(KeyValuePair<TKey, TValue> item) =>
        _dictionary.TryGetValue(item.Key, out TValue? value)
            && EqualityComparer<TValue>.Default.Equals(value, item.Value)
            && Remove(item.Key);

    /// <summary>
    /// Removes the item with the specific key from the collection.
    /// </summary>
    /// <param name="key">
    /// The key of the item to remove from the collection.
    /// </param>
    /// <returns>
    /// <c>true</c> if the item was successfully removed from the collection; otherwise <c>false</c>. This method also returns <c>false</c> if
    /// the key is not found in the original collection.
    /// </returns>
    public bool Remove(TKey key)
    {
        if (!_dictionary.Remove(key))
        {
            return false;
        }

        _list.RemoveAt(IndexOf(key));
        return true;
    }

    /// <summary>
    /// Removes the item at the specified index.
    /// </summary>
    /// <param name="index">
    /// The zero-based index of the item to remove.
    /// </param>
    public void RemoveAt(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _list.Count);
        _dictionary.Remove(_list[index].Key);
        _list.RemoveAt(index);
    }

    /// <summary>
    /// Attempts to add the specified key and value pair to the collection.
    /// </summary>
    /// <param name="key">
    /// The key of the item to add.
    /// </param>
    /// <param name="value">
    /// The value of the item to add. It can be <c>null</c>.
    /// </param>
    /// <returns></returns>
    public bool TryAdd(TKey key, TValue value)
    {
        if (!_dictionary.TryAdd(key, value))
        {
            return false;
        }

        _list.Add(new KeyValuePair<TKey, TValue>(key, value));
        return true;
    }

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">
    /// The key whose value to get.
    /// </param>
    /// <param name="value">
    /// When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for
    /// the type of the value parameter. This parameter is passed uninitialized.
    /// </param>
    /// <returns>
    /// <c>true</c> if the collection contains an element with the specified key; otherwise, <c>false</c>.
    /// </returns>
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => _dictionary.TryGetValue(key, out value);
}
