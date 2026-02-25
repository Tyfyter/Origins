using System;

namespace Origins.Core; 
public readonly struct SetOverride<T>(T[] set, int index, T value) : IDisposable where T : struct {
	readonly T originalValue = set[index];
	readonly bool replaced = set[index].TrySet(value);
	void IDisposable.Dispose() {
		if (replaced) set[index] = originalValue;
	}
}
