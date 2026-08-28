// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Net35.MsCorLib.Tests
{
    /// <summary>
    /// Tests for the read-only collection interfaces. They carry no implementation, so what is
    /// verified is that the declared contracts are implementable and dispatch correctly.
    /// </summary>
    [TestFixture]
    public sealed class ReadOnlyCollectionInterfaceTests
    {
        /// <summary>A read-only list view over an array.</summary>
        private sealed class ReadOnlyArray<T> : IReadOnlyList<T>
        {
            private readonly T[] _items;

            public ReadOnlyArray(T[] items)
            {
                _items = items;
            }

            public int Count
            {
                get { return _items.Length; }
            }

            public T this[int index]
            {
                get { return _items[index]; }
            }

            public IEnumerator<T> GetEnumerator()
            {
                foreach (T item in _items)
                {
                    yield return item;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        /// <summary>A read-only dictionary view over a <see cref="Dictionary{TKey,TValue}"/>.</summary>
        private sealed class ReadOnlyMap<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
        {
            private readonly Dictionary<TKey, TValue> _items;

            public ReadOnlyMap(Dictionary<TKey, TValue> items)
            {
                _items = items;
            }

            public int Count
            {
                get { return _items.Count; }
            }

            public TValue this[TKey key]
            {
                get { return _items[key]; }
            }

            public IEnumerable<TKey> Keys
            {
                get { return _items.Keys; }
            }

            public IEnumerable<TValue> Values
            {
                get { return _items.Values; }
            }

            public bool ContainsKey(TKey key)
            {
                return _items.ContainsKey(key);
            }

            public bool TryGetValue(TKey key, out TValue value)
            {
                return _items.TryGetValue(key, out value);
            }

            public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
            {
                return _items.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        /// <summary>A read-only set view over a list, implementing the full set-relation contract.</summary>
        private sealed class ReadOnlyBag<T> : IReadOnlySet<T>
        {
            private readonly List<T> _items;

            public ReadOnlyBag(params T[] items)
            {
                _items = new List<T>(items);
            }

            public int Count
            {
                get { return _items.Count; }
            }

            public bool Contains(T item)
            {
                return _items.Contains(item);
            }

            public bool IsProperSubsetOf(IEnumerable<T> other)
            {
                List<T> otherItems = Materialise(other);
                return IsSubsetOfCore(otherItems) && otherItems.Count > _items.Count;
            }

            public bool IsProperSupersetOf(IEnumerable<T> other)
            {
                List<T> otherItems = Materialise(other);
                return IsSupersetOfCore(otherItems) && otherItems.Count < _items.Count;
            }

            public bool IsSubsetOf(IEnumerable<T> other)
            {
                return IsSubsetOfCore(Materialise(other));
            }

            public bool IsSupersetOf(IEnumerable<T> other)
            {
                return IsSupersetOfCore(Materialise(other));
            }

            public bool Overlaps(IEnumerable<T> other)
            {
                foreach (T item in Materialise(other))
                {
                    if (_items.Contains(item))
                    {
                        return true;
                    }
                }

                return false;
            }

            public bool SetEquals(IEnumerable<T> other)
            {
                List<T> otherItems = Materialise(other);
                return IsSubsetOfCore(otherItems) && IsSupersetOfCore(otherItems);
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _items.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            private static List<T> Materialise(IEnumerable<T> other)
            {
                if (other == null)
                {
                    throw new ArgumentNullException("other");
                }

                return new List<T>(other);
            }

            private bool IsSubsetOfCore(List<T> other)
            {
                foreach (T item in _items)
                {
                    if (!other.Contains(item))
                    {
                        return false;
                    }
                }

                return true;
            }

            private bool IsSupersetOfCore(List<T> other)
            {
                foreach (T item in other)
                {
                    if (!_items.Contains(item))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        [Test]
        public void ReadOnlyCollection_ExposesCountAndEnumerates()
        {
            IReadOnlyCollection<int> collection = new ReadOnlyArray<int>(new int[] { 1, 2, 3 });

            Assert.AreEqual(3, collection.Count);
            CollectionAssert.AreEqual(new int[] { 1, 2, 3 }, collection);
        }

        [Test]
        public void ReadOnlyCollection_CanBeEmpty()
        {
            IReadOnlyCollection<int> collection = new ReadOnlyArray<int>(new int[0]);

            Assert.AreEqual(0, collection.Count);
            CollectionAssert.AreEqual(new int[0], collection);
        }

        [Test]
        public void ReadOnlyList_ExposesTheIndexer()
        {
            IReadOnlyList<string> list = new ReadOnlyArray<string>(new string[] { "a", "b", "c" });

            Assert.AreEqual("a", list[0]);
            Assert.AreEqual("c", list[2]);
            Assert.AreEqual(3, list.Count);
        }

        [Test]
        public void ReadOnlyList_IndexerOutOfRange_Throws()
        {
            IReadOnlyList<string> list = new ReadOnlyArray<string>(new string[] { "a" });

            // The interface leaves the failure mode to the implementation; this one indexes an array.
            Assert.Throws<IndexOutOfRangeException>(delegate { GC.KeepAlive(list[5]); });
        }

        [Test]
        public void ReadOnlyList_IsAlsoAReadOnlyCollection()
        {
            IReadOnlyList<int> list = new ReadOnlyArray<int>(new int[] { 1 });

            Assert.IsTrue(list is IReadOnlyCollection<int>);
            Assert.IsTrue(list is IEnumerable<int>);
        }

        [Test]
        public void ReadOnlyDictionary_LooksUpByKey()
        {
            Dictionary<string, int> backing = new Dictionary<string, int>();
            backing.Add("one", 1);
            backing.Add("two", 2);

            IReadOnlyDictionary<string, int> map = new ReadOnlyMap<string, int>(backing);

            Assert.AreEqual(2, map.Count);
            Assert.AreEqual(1, map["one"]);
            Assert.IsTrue(map.ContainsKey("two"));
            Assert.IsFalse(map.ContainsKey("three"));
        }

        [Test]
        public void ReadOnlyDictionary_TryGetValue()
        {
            Dictionary<string, int> backing = new Dictionary<string, int>();
            backing.Add("one", 1);

            IReadOnlyDictionary<string, int> map = new ReadOnlyMap<string, int>(backing);

            int found;
            Assert.IsTrue(map.TryGetValue("one", out found));
            Assert.AreEqual(1, found);

            int missing;
            Assert.IsFalse(map.TryGetValue("nope", out missing));
            Assert.AreEqual(0, missing);
        }

        [Test]
        public void ReadOnlyDictionary_ExposesKeysAndValues()
        {
            Dictionary<string, int> backing = new Dictionary<string, int>();
            backing.Add("one", 1);
            backing.Add("two", 2);

            IReadOnlyDictionary<string, int> map = new ReadOnlyMap<string, int>(backing);

            CollectionAssert.AreEqual(new string[] { "one", "two" }, map.Keys);
            CollectionAssert.AreEqual(new int[] { 1, 2 }, map.Values);
        }

        [Test]
        public void ReadOnlyDictionary_EnumeratesKeyValuePairs()
        {
            Dictionary<string, int> backing = new Dictionary<string, int>();
            backing.Add("one", 1);

            IReadOnlyDictionary<string, int> map = new ReadOnlyMap<string, int>(backing);

            List<KeyValuePair<string, int>> pairs = new List<KeyValuePair<string, int>>(map);

            Assert.AreEqual(1, pairs.Count);
            Assert.AreEqual("one", pairs[0].Key);
            Assert.AreEqual(1, pairs[0].Value);
        }

        [Test]
        public void ReadOnlySet_Contains()
        {
            IReadOnlySet<int> set = new ReadOnlyBag<int>(1, 2, 3);

            Assert.IsTrue(set.Contains(2));
            Assert.IsFalse(set.Contains(9));
            Assert.AreEqual(3, set.Count);
        }

        [Test]
        public void ReadOnlySet_SubsetAndSupersetRelations()
        {
            IReadOnlySet<int> set = new ReadOnlyBag<int>(1, 2);

            Assert.IsTrue(set.IsSubsetOf(new int[] { 1, 2, 3 }));
            Assert.IsTrue(set.IsSubsetOf(new int[] { 1, 2 }));
            Assert.IsFalse(set.IsSubsetOf(new int[] { 1 }));

            Assert.IsTrue(set.IsProperSubsetOf(new int[] { 1, 2, 3 }));
            Assert.IsFalse(set.IsProperSubsetOf(new int[] { 1, 2 }));

            Assert.IsTrue(set.IsSupersetOf(new int[] { 1 }));
            Assert.IsTrue(set.IsProperSupersetOf(new int[] { 1 }));
            Assert.IsFalse(set.IsProperSupersetOf(new int[] { 1, 2 }));
        }

        [Test]
        public void ReadOnlySet_OverlapsAndSetEquals()
        {
            IReadOnlySet<int> set = new ReadOnlyBag<int>(1, 2);

            Assert.IsTrue(set.Overlaps(new int[] { 2, 5 }));
            Assert.IsFalse(set.Overlaps(new int[] { 7, 8 }));

            Assert.IsTrue(set.SetEquals(new int[] { 2, 1 }));
            Assert.IsFalse(set.SetEquals(new int[] { 1 }));
        }

        [Test]
        public void ReadOnlySet_NullOther_Throws()
        {
            IReadOnlySet<int> set = new ReadOnlyBag<int>(1);

            Assert.Throws<ArgumentNullException>(delegate { set.IsSubsetOf(null); });
            Assert.Throws<ArgumentNullException>(delegate { set.Overlaps(null); });
            Assert.Throws<ArgumentNullException>(delegate { set.SetEquals(null); });
        }

        [Test]
        public void ReadOnlySet_IsAlsoAReadOnlyCollection()
        {
            IReadOnlySet<int> set = new ReadOnlyBag<int>(1);

            Assert.IsTrue(set is IReadOnlyCollection<int>);
            CollectionAssert.AreEqual(new int[] { 1 }, set);
        }
    }
}
