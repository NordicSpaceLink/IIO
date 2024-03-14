// Copyright (C) 2024 - Nordic Space Link
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NordicSpaceLink.IIO
{
    public class Indexer<TKey, TValue> : IEnumerable<TValue>
    {
        private readonly Dictionary<TKey, TValue> valueLookup;
        private readonly List<TValue> values;

        internal Indexer(List<(TValue value, TKey[] keys)> values)
        {
            this.valueLookup = values.SelectMany(v => v.keys.Select(key => (key, v.value))).ToDictionary(v => v.key, v => v.value);
            this.values = values.Select(x => x.value).ToList();
        }

        public TValue this[TKey key]
        {
            get
            {
                return valueLookup[key];
            }
        }

        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
        {
            return values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return values.GetEnumerator();
        }
    }
}