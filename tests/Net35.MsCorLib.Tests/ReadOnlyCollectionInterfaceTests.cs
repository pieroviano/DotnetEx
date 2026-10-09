// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
extern alias shim;

using System;
using System.Collections;
using System.Collections.Generic;
using shimcol = shim::System.Collections.Generic;

namespace Net35.MsCorLib.Tests
{
    public sealed class ReadOnlyCollectionInterfaceTests
    {
        /// <summary>A read-only list view over an array.</summary>
        private sealed class ReadOnlyArray<T> : shimcol.IReadOnlyList<T>
        {
            private readonly T[] _items;
            public ReadOnlyArray(T[] items)
            {
                _items = items;
            }

            public int Count
            {
                get
                {
                    return _items.Length;
                }
            }

            public T this[int index]
            {
                get
                {
                    return _items[index];
                }
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

        /// <summary>A read-only dictionary view over a <see cref = "Dictionary{TKey, TValue}"/>.</summary>
        private sealed class ReadOnlyMap<TKey, TValue> : shimcol.IReadOnlyDictionary<TKey, TValue>
        {
            private readonly Dictionary<TKey, TValue> _items;
            public ReadOnlyMap(Dictionary<TKey, TValue> items)
            {
                _items = items;
            }

            public int Count
            {
                get
                {
                    return _items.Count;
                }
            }

            public TValue this[TKey key]
            {
                get
                {
                    return _items[key];
                }
            }

            public IEnumerable<TKey> Keys
            {
                get
                {
                    return _items.Keys;
                }
            }

            public IEnumerable<TValue> Values
            {
                get
                {
                    return _items.Values;
                }
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
        private sealed class ReadOnlyBag<T> : shimcol.IReadOnlySet<T>
        {
            private readonly List<T> _items;
            public ReadOnlyBag(params T[] items)
            {
                _items = new List<T>(items);
            }

            public int Count
            {
                get
                {
                    return _items.Count;
                }
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

        [Fact]
        public void ReadOnlyCollection_ExposesCountAndEnumerates()
        {
            shimcol.IReadOnlyCollection<int> collection = new ReadOnlyArray<int>(new int[] { 1, 2, 3 });
            Assert.Equal(3, collection.Count);
            Assert.Equal(new int[] { 1, 2, 3 }, collection);
        }

        [Fact]
        public void ReadOnlyCollection_CanBeEmpty()
        {
            shimcol.IReadOnlyCollection<int> collection = new ReadOnlyArray<int>(new int[0]);
            Assert.Equal(0, collection.Count);
            Assert.Equal(new int[0], collection);
        }

        [Fact]
        public void ReadOnlyList_ExposesTheIndexer()
        {
            shimcol.IReadOnlyList<string> list = new ReadOnlyArray<string>(new string[] { "a", "b", "c" });
            Assert.Equal("a", list[0]);
            Assert.Equal("c", list[2]);
            Assert.Equal(3, list.Count);
        }

        [Fact]
        public void ReadOnlyList_IndexerOutOfRange_Throws()
        {
            shimcol.IReadOnlyList<string> list = new ReadOnlyArray<string>(new string[] { "a" });
            // The interface leaves the failure mode to the implementation; this one indexes an array.
            Assert.Throws<IndexOutOfRangeException>(delegate
            {
                GC.KeepAlive(list[5]);
            });
        }

        [Fact]
        public void ReadOnlyList_IsAlsoAReadOnlyCollection()
        {
            shimcol.IReadOnlyList<int> list = new ReadOnlyArray<int>(new int[] { 1 });
            Assert.True(list is shimcol.IReadOnlyCollection<int>);
            Assert.True(list is IEnumerable<int>);
        }

        [Fact]
        public void ReadOnlyDictionary_LooksUpByKey()
        {
            Dictionary<string, int> backing = new Dictionary<string, int>();
            backing.Add("one", 1);
            backing.Add("two", 2);
            shimcol.IReadOnlyDictionary<string, int> map = new ReadOnlyMap<string, int>(backing);
            Assert.Equal(2, map.Count);
            Assert.Equal(1, map["one"]);
            Assert.True(map.ContainsKey("two"));
            Assert.False(map.ContainsKey("three"));
        }

        [Fact]
        public void ReadOnlyDictionary_TryGetValue()
        {
            Dictionary<string, int> backing = new Dictionary<string, int>();
            backing.Add("one", 1);
            shimcol.IReadOnlyDictionary<string, int> map = new ReadOnlyMap<string, int>(backing);
            int found;
            Assert.True(map.TryGetValue("one", out found));
            Assert.Equal(1, found);
            int missing;
            Assert.False(map.TryGetValue("nope", out missing));
            Assert.Equal(0, missing);
        }

        [Fact]
        public void ReadOnlyDictionary_ExposesKeysAndValues()
        {
            Dictionary<string, int> backing = new Dictionary<string, int>();
            backing.Add("one", 1);
            backing.Add("two", 2);
            shimcol.IReadOnlyDictionary<string, int> map = new ReadOnlyMap<string, int>(backing);
            Assert.Equal(new string[] { "one", "two" }, map.Keys);
            Assert.Equal(new int[] { 1, 2 }, map.Values);
        }

        [Fact]
        public void ReadOnlyDictionary_EnumeratesKeyValuePairs()
        {
            Dictionary<string, int> backing = new Dictionary<string, int>();
            backing.Add("one", 1);
            shimcol.IReadOnlyDictionary<string, int> map = new ReadOnlyMap<string, int>(backing);
            List<KeyValuePair<string, int>> pairs = new List<KeyValuePair<string, int>>(map);
            Assert.Equal(1, pairs.Count);
            Assert.Equal("one", pairs[0].Key);
            Assert.Equal(1, pairs[0].Value);
        }

        [Fact]
        public void ReadOnlySet_Contains()
        {
            shimcol.IReadOnlySet<int> set = new ReadOnlyBag<int>(1, 2, 3);
            Assert.True(set.Contains(2));
            Assert.False(set.Contains(9));
            Assert.Equal(3, set.Count);
        }

        [Fact]
        public void ReadOnlySet_SubsetAndSupersetRelations()
        {
            shimcol.IReadOnlySet<int> set = new ReadOnlyBag<int>(1, 2);
            Assert.True(set.IsSubsetOf(new int[] { 1, 2, 3 }));
            Assert.True(set.IsSubsetOf(new int[] { 1, 2 }));
            Assert.False(set.IsSubsetOf(new int[] { 1 }));
            Assert.True(set.IsProperSubsetOf(new int[] { 1, 2, 3 }));
            Assert.False(set.IsProperSubsetOf(new int[] { 1, 2 }));
            Assert.True(set.IsSupersetOf(new int[] { 1 }));
            Assert.True(set.IsProperSupersetOf(new int[] { 1 }));
            Assert.False(set.IsProperSupersetOf(new int[] { 1, 2 }));
        }

        [Fact]
        public void ReadOnlySet_OverlapsAndSetEquals()
        {
            shimcol.IReadOnlySet<int> set = new ReadOnlyBag<int>(1, 2);
            Assert.True(set.Overlaps(new int[] { 2, 5 }));
            Assert.False(set.Overlaps(new int[] { 7, 8 }));
            Assert.True(set.SetEquals(new int[] { 2, 1 }));
            Assert.False(set.SetEquals(new int[] { 1 }));
        }

        [Fact]
        public void ReadOnlySet_NullOther_Throws()
        {
            shimcol.IReadOnlySet<int> set = new ReadOnlyBag<int>(1);
            Assert.Throws<ArgumentNullException>(delegate
            {
                set.IsSubsetOf(null);
            });
            Assert.Throws<ArgumentNullException>(delegate
            {
                set.Overlaps(null);
            });
            Assert.Throws<ArgumentNullException>(delegate
            {
                set.SetEquals(null);
            });
        }

        [Fact]
        public void ReadOnlySet_IsAlsoAReadOnlyCollection()
        {
            shimcol.IReadOnlySet<int> set = new ReadOnlyBag<int>(1);
            Assert.True(set is shimcol.IReadOnlyCollection<int>);
            Assert.Equal(new int[] { 1 }, set);
        }
    }
}