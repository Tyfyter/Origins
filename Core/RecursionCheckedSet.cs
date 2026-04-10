using System;
using System.Collections.Generic;

namespace Origins.Core;
//TODO: remove, moved to PegasusLib
public class RecursionCheckedSet<T>(IEqualityComparer<T> comparer = null) {
	readonly HashSet<T> values = new(comparer);
	public IDisposable TryAdd(T value) {
		if (!values.Add(value)) return null;
		return new RecursionInstance(this, value);
	}
	readonly struct RecursionInstance(RecursionCheckedSet<T> set, T value) : IDisposable {
		public readonly void Dispose() => set.values.Remove(value);
	}
}
