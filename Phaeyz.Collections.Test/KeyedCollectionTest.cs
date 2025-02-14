using TUnit.Assertions.AssertConditions.Throws;

namespace Phaeyz.Collections.Test;

public class KeyedCollectionTest
{
    private static async Task VerifyCollection(KeyedCollection<int, char> collection, params IEnumerable<(int, char)> expected)
    {
        await Assert.That(collection.Count).IsEqualTo(expected.Count());
        IEnumerator<KeyValuePair<int, char>> enumerable = collection.GetEnumerator();
        int index = 0;
        foreach ((int key, char value) in expected)
        {
            await Assert.That(enumerable.MoveNext()).IsTrue();
            await Assert.That(enumerable.Current.Key).IsEqualTo(key);
            await Assert.That(enumerable.Current.Value).IsEqualTo(value);
            await Assert.That(collection[key]).IsEqualTo(value);
            await Assert.That(collection.GetAt(index++)).IsEqualTo(new KeyValuePair<int, char>(key, value));
        }
    }

    [Test]
    public async Task Add_AttemptToAddExistingElement_Throws()
    {
        KeyedCollection<int, char> collection = [];
        collection.Add(11, 'b');
        collection.Add(10, 'a');
        await Assert.That(() => collection.Add(10, 'a')).Throws<ArgumentException>();
        await VerifyCollection(collection, (11, 'b'), (10, 'a'));
    }

    [Test]
    public async Task Add_BasicAdds_ElementsAdded()
    {
        KeyedCollection<int, char> collection = [];
        collection.Add(12, 'c');
        collection.Add(11, 'b');
        collection.Add(10, 'a');
        await VerifyCollection(collection, (12, 'c'), (11, 'b'), (10, 'a'));
    }

    [Test]
    public async Task AddRange_AttemptToAddExistingElement_Throws()
    {
        KeyedCollection<int, char> collection = [];
        collection.Add(12, 'c');
        collection.Add(11, 'b');
        await Assert.That(() => collection.AddRange([
            new KeyValuePair<int, char>(10, 'a'),
            new KeyValuePair<int, char>(11, 'b')])).Throws<ArgumentException>();
        // Verifying with (10, 'a') because AddRange is incremental.
        await VerifyCollection(collection, (12, 'c'), (11, 'b'), (10, 'a'));
    }

    [Test]
    public async Task AddRange_EmptySourceCollection_ElementsAdded()
    {
        KeyedCollection<int, char> collection = [];
        collection.AddRange([
            new KeyValuePair<int, char>(12, 'c'),
            new KeyValuePair<int, char>(11, 'b'),
            new KeyValuePair<int, char>(10, 'a')]);
        await VerifyCollection(collection, (12, 'c'), (11, 'b'), (10, 'a'));
    }

    [Test]
    public async Task AddRange_EmptySourceCollectionAndEmptyRange_CollectionStillEmpty()
    {
        KeyedCollection<int, char> collection = [];
        collection.AddRange([]);
        await VerifyCollection(collection);
    }

    [Test]
    public async Task AddRange_NonEmptySourceCollection_ElementsAdded()
    {
        KeyedCollection<int, char> collection = [];
        collection.Add(13, 'd');
        collection.Add(12, 'c');
        collection.AddRange([
            new KeyValuePair<int, char>(11, 'b'),
            new KeyValuePair<int, char>(10, 'a')]);
        await VerifyCollection(collection, (13, 'd'), (12, 'c'), (11, 'b'), (10, 'a'));
    }

    [Test]
    public async Task AddRange_NonEmptySourceCollectionButEmptyRange_CollectionUnchanged()
    {
        KeyedCollection<int, char> collection = [];
        collection.Add(12, 'c');
        collection.Add(11, 'b');
        collection.Add(10, 'a');
        collection.AddRange([]);
        await VerifyCollection(collection, (12, 'c'), (11, 'b'), (10, 'a'));
    }

    [Test]
    public async Task Clear_EmptyCollection_StillEmpty()
    {
        KeyedCollection<int, char> collection = [];
        collection.Clear();
        await VerifyCollection(collection);
    }

    [Test]
    public async Task Clear_NonEmptyCollection_NowEmpty()
    {
        KeyedCollection<int, char> collection = [];
        collection.Add(12, 'c');
        collection.Add(11, 'b');
        collection.Add(10, 'a');
        await VerifyCollection(collection, (12, 'c'), (11, 'b'), (10, 'a'));
        collection.Clear();
        await VerifyCollection(collection);
    }

    [Test]
    [Arguments(0, 'a')]
    [Arguments(1, 'b')]
    [Arguments(2, 'c')]
    public async Task Contains_KvpFound_ReturnsTrue(int key, char value)
    {
        KeyedCollection<int, char> collection = [];
        collection.Add(0, 'a');
        collection.Add(1, 'b');
        collection.Add(2, 'c');
        await Assert.That(() => collection.Contains(new KeyValuePair<int, char>(key, value))).IsEqualTo(true);
        await VerifyCollection(collection, (0, 'a'), (1, 'b'), (2, 'c'));
    }

    [Test]
    [Arguments(-1, 'a')]
    [Arguments(0, 'b')]
    [Arguments(3, 'c')]
    public async Task Contains_KvpNotFound_ReturnsFalse(int key, char value)
    {
        KeyedCollection<int, char> collection = [];
        collection.Add(0, 'a');
        collection.Add(1, 'b');
        collection.Add(2, 'c');
        await Assert.That(() => collection.Contains(new KeyValuePair<int, char>(key, value))).IsEqualTo(false);
        await VerifyCollection(collection, (0, 'a'), (1, 'b'), (2, 'c'));
    }

    [Test]
    [Arguments(0)]
    [Arguments(1)]
    [Arguments(2)]
    public async Task ContainsKey_KeyFound_ReturnsTrue(int key)
    {
        KeyedCollection<int, char> collection = [];
        collection.Add(0, 'a');
        collection.Add(1, 'b');
        collection.Add(2, 'c');
        await Assert.That(() => collection.ContainsKey(key)).IsEqualTo(true);
        await VerifyCollection(collection, (0, 'a'), (1, 'b'), (2, 'c'));
    }

    [Test]
    [Arguments(-1)]
    [Arguments(3)]
    public async Task ContainsKey_KeyNotFound_ReturnsFalse(int key)
    {
        KeyedCollection<int, char> collection = [];
        collection.Add(0, 'a');
        collection.Add(1, 'b');
        collection.Add(2, 'c');
        await Assert.That(() => collection.ContainsKey(key)).IsEqualTo(false);
        await VerifyCollection(collection, (0, 'a'), (1, 'b'), (2, 'c'));
    }

    [Test]
    public async Task CopyTo_EmptyCollectionWithCount_NoElementsCopied()
    {
        KeyedCollection<int, char> collection = [];
        KeyValuePair<int, char>[] destination = new KeyValuePair<int, char>[6];
        collection.CopyTo(0, destination, 1, 0);
        KeyValuePair<int, char>[] expected =
        [
            default,
            default,
            default,
            default,
            default,
            default
        ];
        await Assert.That(() => destination).IsEquivalentTo(expected);
        await VerifyCollection(collection);
    }

    [Test]
    public async Task CopyTo_EmptyCollectionWithoutCount_NoElementsCopied()
    {
        KeyedCollection<int, char> collection = [];
        KeyValuePair<int, char>[] destination = new KeyValuePair<int, char>[6];
        collection.CopyTo(destination, 1);
        KeyValuePair<int, char>[] expected =
        [
            default,
            default,
            default,
            default,
            default,
            default
        ];
        await Assert.That(() => destination).IsEquivalentTo(expected);
        await VerifyCollection(collection);
    }

    [Test]
    public async Task CopyTo_CopySubsetWithCount_SubsetCopied()
    {
        KeyedCollection<int, char> collection = [];
        collection.Add(0, 'a');
        collection.Add(1, 'b');
        collection.Add(2, 'c');
        collection.Add(3, 'd');
        KeyValuePair<int, char>[] destination = new KeyValuePair<int, char>[6];
        collection.CopyTo(1, destination, 1, 2);
        KeyValuePair<int, char>[] expected =
        [
            default,
            new KeyValuePair<int, char>(1, 'b'),
            new KeyValuePair<int, char>(2, 'c'),
            default,
            default,
            default
        ];
        await Assert.That(() => destination).IsEquivalentTo(expected);
        await VerifyCollection(collection, (0, 'a'), (1, 'b'), (2, 'c'), (3, 'd'));
    }

    [Test]
    public async Task CopyTo_CopyWithoutCount_ElementsCopied()
    {
        KeyedCollection<int, char> collection = [];
        collection.Add(0, 'a');
        collection.Add(1, 'b');
        collection.Add(2, 'c');
        collection.Add(3, 'd');
        KeyValuePair<int, char>[] destination = new KeyValuePair<int, char>[6];
        collection.CopyTo(destination, 1);
        KeyValuePair<int, char>[] expected =
        [
            default,
            new KeyValuePair<int, char>(0, 'a'),
            new KeyValuePair<int, char>(1, 'b'),
            new KeyValuePair<int, char>(2, 'c'),
            new KeyValuePair<int, char>(3, 'd'),
            default
        ];
        await Assert.That(() => destination).IsEquivalentTo(expected);
        await VerifyCollection(collection, (0, 'a'), (1, 'b'), (2, 'c'), (3, 'd'));
    }

    [Test]
    public async Task IndexOf_EmptyCollection_ReturnsNegativeOne()
    {
        KeyedCollection<int, char> collection = [];
        await Assert.That(() => collection.IndexOf(0)).IsEqualTo(-1);
        await VerifyCollection(collection);
    }

    [Test]
    public async Task IndexOf_FirstIndex_ReturnsZero()
    {
        KeyedCollection<int, char> collection = [];
        collection.Add(0, 'a');
        collection.Add(1, 'b');
        collection.Add(2, 'c');
        await Assert.That(() => collection.IndexOf(0)).IsEqualTo(0);
        await VerifyCollection(collection, (0, 'a'), (1, 'b'), (2, 'c'));
    }

    [Test]
    public async Task IndexOf_KeyNotFound_ReturnsNegativeOne()
    {
        KeyedCollection<int, char> collection = [];
        collection.Add(0, 'a');
        await Assert.That(() => collection.IndexOf(1)).IsEqualTo(-1);
        await VerifyCollection(collection, (0, 'a'));
    }

    [Test]
    public async Task IndexOf_LastIndex_ReturnsTwo()
    {
        KeyedCollection<int, char> collection = [];
        collection.Add(0, 'a');
        collection.Add(1, 'b');
        collection.Add(2, 'c');
        await Assert.That(() => collection.IndexOf(2)).IsEqualTo(2);
        await VerifyCollection(collection, (0, 'a'), (1, 'b'), (2, 'c'));
    }

    [Test]
    public async Task Insert_EmptyCollection_Inserted()
    {
        KeyedCollection<int, char> collection = [];
        collection.Insert(0, new KeyValuePair<int, char>(0, 'a'));
        await VerifyCollection(collection, (0, 'a'));
    }

    [Test]
    public async Task Insert_KeyAlreadyExists_Throws()
    {
        KeyedCollection<int, char> collection = [];
        collection.Add(0, 'a');
        await Assert.That(() => collection.Insert(1, new KeyValuePair<int, char>(0, 'b'))).Throws<ArgumentException>();
        await VerifyCollection(collection, (0, 'a'));
    }

    [Test]
    [Arguments(-1)]
    [Arguments(2)]
    public async Task Insert_OutOfBounds_Throws(int index)
    {
        KeyedCollection<int, char> collection = [];
        collection.Add(0, 'a');
        await Assert.That(() => collection.Insert(index, new KeyValuePair<int, char>(1, 'b'))).Throws<ArgumentOutOfRangeException>();
        await VerifyCollection(collection, (0, 'a'));
    }

    [Test]
    public async Task Insert_ToFirst_Inserted()
    {
        KeyedCollection<int, char> collection = [];
        collection.Add(0, 'a');
        collection.Insert(0, new KeyValuePair<int, char>(1, 'b'));
        await VerifyCollection(collection, (1, 'b'), (0, 'a'));
    }

    [Test]
    public async Task Insert_ToLast_Inserted()
    {
        KeyedCollection<int, char> collection = [];
        collection.Add(0, 'a');
        collection.Insert(1, new KeyValuePair<int, char>(1, 'b'));
        await VerifyCollection(collection, (0, 'a'), (1, 'b'));
    }

    [Test]
    public async Task Keys_EmptyCollection_KeysIsEmpty()
    {
        KeyedCollection<int, char> collection = [];
        await VerifyCollection(collection);
        await Assert.That(() => collection.Keys).IsEquivalentTo(Array.Empty<int>());
    }

    [Test]
    public async Task Keys_NonEmptyCollection_KeysAreCorrect()
    {
        KeyedCollection<int, char> collection = [];
        collection.Add(12, 'c');
        collection.Add(11, 'b');
        collection.Add(10, 'a');
        await VerifyCollection(collection, (12, 'c'), (11, 'b'), (10, 'a'));
        await Assert.That(() => collection.Keys).IsEquivalentTo([12, 11, 10]);
    }

    [Test]
    public async Task Remove_KvpFirstElement_OnlyFirstRemoved()
    {
        KeyedCollection<int, char> collection = [];
        collection.Add(0, 'a');
        collection.Add(1, 'b');
        collection.Add(2, 'c');
        await Assert.That(() => collection.Remove(new KeyValuePair<int, char>(0, 'a'))).IsEqualTo(true);
        await VerifyCollection(collection, (1, 'b'), (2, 'c'));
    }

    [Test]
    public async Task Remove_KvpLastElement_OnlyLastRemoved()
    {
        KeyedCollection<int, char> collection = [];
        collection.Add(0, 'a');
        collection.Add(1, 'b');
        collection.Add(2, 'c');
        await Assert.That(() => collection.Remove(new KeyValuePair<int, char>(2, 'c'))).IsEqualTo(true);
        await VerifyCollection(collection, (0, 'a'), (1, 'b'));
    }

    [Test]
    public async Task Remove_KvpKeyDoesNotExist_ReturnsFalse()
    {
        KeyedCollection<int, char> collection = [];
        collection.Add(0, 'a');
        await Assert.That(() => collection.Remove(new KeyValuePair<int, char>(1, 'a'))).IsEqualTo(false);
    }

    [Test]
    public async Task Remove_KvpSingleElement_BecomesEmpty()
    {
        KeyedCollection<int, char> collection = [];
        collection.Add(0, 'a');
        await Assert.That(() => collection.Remove(new KeyValuePair<int, char>(0, 'a'))).IsEqualTo(true);
        await VerifyCollection(collection);
    }

    [Test]
    public async Task Remove_KvpValueDoesNotExist_ReturnsFalse()
    {
        KeyedCollection<int, char> collection = [];
        collection.Add(0, 'a');
        await Assert.That(() => collection.Remove(new KeyValuePair<int, char>(0, 'b'))).IsEqualTo(false);
    }

    [Test]
    public async Task Remove_FirstElement_OnlyFirstRemoved()
    {
        KeyedCollection<int, char> collection = [];
        collection.Add(0, 'a');
        collection.Add(1, 'b');
        collection.Add(2, 'c');
        await Assert.That(() => collection.Remove(0)).IsEqualTo(true);
        await VerifyCollection(collection, (1, 'b'), (2, 'c'));
    }

    [Test]
    public async Task Remove_LastElement_OnlyLastRemoved()
    {
        KeyedCollection<int, char> collection = [];
        collection.Add(0, 'a');
        collection.Add(1, 'b');
        collection.Add(2, 'c');
        await Assert.That(() => collection.Remove(2)).IsEqualTo(true);
        await VerifyCollection(collection, (0, 'a'), (1, 'b'));
    }

    [Test]
    public async Task Remove_KeyDoesNotExist_ReturnsFalse()
    {
        KeyedCollection<int, char> collection = [];
        collection.Add(0, 'a');
        await Assert.That(() => collection.Remove(1)).IsEqualTo(false);
    }

    [Test]
    public async Task Remove_SingleElement_BecomesEmpty()
    {
        KeyedCollection<int, char> collection = [];
        collection.Add(0, 'a');
        await Assert.That(() => collection.Remove(0)).IsEqualTo(true);
        await VerifyCollection(collection);
    }

    [Test]
    public async Task RemoveAt_FirstElement_OnlyFirstRemoved()
    {
        KeyedCollection<int, char> collection = [];
        collection.Add(0, 'a');
        collection.Add(1, 'b');
        collection.Add(2, 'c');
        collection.RemoveAt(0);
        await VerifyCollection(collection, (1, 'b'), (2, 'c'));
    }

    [Test]
    public async Task RemoveAt_LastElement_OnlyLastRemoved()
    {
        KeyedCollection<int, char> collection = [];
        collection.Add(0, 'a');
        collection.Add(1, 'b');
        collection.Add(2, 'c');
        collection.RemoveAt(2);
        await VerifyCollection(collection, (0, 'a'), (1, 'b'));
    }

    [Test]
    [Arguments(-1)]
    [Arguments(1)]
    public async Task RemoveAt_OutOfBounds_Throws(int index)
    {
        KeyedCollection<int, char> collection = [];
        collection.Add(0, 'a');
        await Assert.That(() => collection.RemoveAt(index)).Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task RemoveAt_SingleElement_BecomesEmpty()
    {
        KeyedCollection<int, char> collection = [];
        collection.Add(0, 'a');
        collection.RemoveAt(0);
        await VerifyCollection(collection);
    }

    [Test]
    public async Task TryAdd_ElementAlreadyExists_ReturnsFalse()
    {
        KeyedCollection<int, char> collection = [];
        collection.Add(0, 'a');
        await Assert.That(() => collection.TryAdd(0, 'a')).IsEqualTo(false);
        await VerifyCollection(collection, (0, 'a'));
    }

    [Test]
    public async Task TryAdd_ElementDoesNotExist_ElementAdded()
    {
        KeyedCollection<int, char> collection = [];
        collection.Add(0, 'a');
        await Assert.That(() => collection.TryAdd(1, 'b')).IsEqualTo(true);
        await VerifyCollection(collection, (0, 'a'), (1, 'b'));
    }

    [Test]
    public async Task TryAdd_EmptyCollection_ElementAdded()
    {
        KeyedCollection<int, char> collection = [];
        await Assert.That(() => collection.TryAdd(0, 'a')).IsEqualTo(true);
        await VerifyCollection(collection, (0, 'a'));
    }

    [Test]
    public async Task TryGetValue_ElementExists_ReturnsTrue()
    {
        KeyedCollection<int, char> collection = [];
        collection.Add(0, 'a');
        char value = '\0';
        await Assert.That(() => collection.TryGetValue(0, out value)).IsEqualTo(true);
        await Assert.That(() => value).IsEqualTo('a');
        await VerifyCollection(collection, (0, 'a'));
    }

    [Test]
    public async Task TryGetValue_ElementDoesNotExist_ReturnsFalse()
    {
        KeyedCollection<int, char> collection = [];
        collection.Add(0, 'a');
        char value;
        await Assert.That(() => collection.TryGetValue(1, out value)).IsEqualTo(false);
        await VerifyCollection(collection, (0, 'a'));
    }

    [Test]
    public async Task TryGetValue_EmptyCollection_ReturnsFalse()
    {
        KeyedCollection<int, char> collection = [];
        char value;
        await Assert.That(() => collection.TryGetValue(0, out value)).IsEqualTo(false);
        await VerifyCollection(collection);
    }

    [Test]
    public async Task Values_EmptyCollection_ValuesIsEmpty()
    {
        KeyedCollection<int, char> collection = [];
        await VerifyCollection(collection);
        await Assert.That(() => collection.Values).IsEquivalentTo(Array.Empty<char>());
    }

    [Test]
    public async Task Values_NonEmptyCollection_ValuesAreCorrect()
    {
        KeyedCollection<int, char> collection = [];
        collection.Add(12, 'c');
        collection.Add(11, 'b');
        collection.Add(10, 'a');
        await VerifyCollection(collection, (12, 'c'), (11, 'b'), (10, 'a'));
        await Assert.That(() => collection.Values).IsEquivalentTo(['c', 'b', 'a']);
    }
}
