using System.Collections.Generic;
using System.Linq;

namespace Origins.Core {
	public class CompressorMap<TKey, TValue>(IEqualityComparer<TKey> keyComparer = null, IEqualityComparer<TValue> valueComparer = null) {
		readonly static CountComparer countComparer = new();
		readonly Dictionary<TKey, Dictionary<TValue, int>> innerDictionary = new(keyComparer);
		public Dictionary<TKey, TValue> Export(int threshold = 2) => new(innerDictionary.TrySelect((KeyValuePair<TKey, Dictionary<TValue, int>> kvp, out KeyValuePair<TKey, TValue> output) => {
			KeyValuePair<TValue, int> max = kvp.Value.Max(countComparer);
			output = new(
				kvp.Key,
				max.Key
			);
			return max.Value >= threshold;
		}));
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
