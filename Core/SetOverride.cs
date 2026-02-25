using System;
using Terraria;

namespace Origins.Core;
public readonly struct SetOverride<T>(T[] set, int index, T value) : IDisposable where T : struct {
	readonly T originalValue = set[index];
	readonly bool replaced = set[index].TrySet(value);
	void IDisposable.Dispose() {
		if (replaced) set[index] = originalValue;
	}
}
public readonly struct WorldGenOverride(bool value = true) : IDisposable {
	readonly bool originalValue = WorldGen.gen;
#pragma warning disable CS0420 // A reference to a volatile field will not be treated as volatile
	readonly bool replaced = WorldGen.gen.TrySet(value);
#pragma warning restore CS0420 // A reference to a volatile field will not be treated as volatile
	void IDisposable.Dispose() {
		if (replaced) WorldGen.gen = originalValue;
	}
}