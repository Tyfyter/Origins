using System.Collections.Generic;
using System.Linq;

namespace Origins.Core {
	public class CompressorMap<TKey, TValue>(IEqualityComparer<TKey> keyComparer = null, IEqualityComparer<TValue> valueComparer = null) {
		readonly static CountComparer countComparer = new();
		readonly Dictionary<TKey, Dictionary<TValue, int>> innerDictionary = new(keyComparer);
		public Dictionary<TKey, TValue> Export() => new(innerDictionary.Select(kvp => new KeyValuePair<TKey, TValue>(
			kvp.Key,
			kvp.Value.Max(countComparer).Key
		)));
		public void Add(TKey key, TValue value) {
			if (!innerDictionary.TryGetValue(key, out Dictionary<TValue, int> counts)) innerDictionary[key] = counts = new(valueComparer);
			counts.TryGetValue(value, out int count);
			counts[value] = count + 1;
		}
		class CountComparer : IComparer<KeyValuePair<TValue, int>> {
			public int Compare(KeyValuePair<TValue, int> x, KeyValuePair<TValue, int> y) => x.Value.CompareTo(y.Value);
		}
	}
}
