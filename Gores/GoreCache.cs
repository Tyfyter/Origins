using System;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Gores;
[ReinitializeDuringResizeArrays]
public sealed class GoreCache {
	public static GoreCache Ashen_Generic = new("NPCs/Ashen_Gore", 4);
	#region impl
	readonly int[] variants;
	GoreCache(string name, int count = 1, int start = 1) {
		name = "Origins/Gores/" + name;
		variants = count > 0 ? new int[count] : throw new ArgumentException("Count must be one or greater", nameof(count));
		for (int i = 0; i < count; i++) {
			variants[i] = Origins.instance.GetGoreSlot(name + (i + start));
		}
	}
	public int this[int variant] => variants[variant];
	public static implicit operator int(GoreCache value) => value.variants.Length > 1 ? Main.rand.Next(value.variants) : value.variants[0];
	#endregion
}
