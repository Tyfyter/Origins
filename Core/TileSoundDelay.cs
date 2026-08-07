using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Core; 
public static class TileSoundDelay {
	static readonly FungibleSet<Tile> delays = new();
	public static int GetSoundDelay(this Tile tile) => delays[tile];
	public static void SetSoundDelay(this Tile tile, int value) => delays[tile] = value;
	public static bool LoopSoundDelay(this Tile tile, int value) {
		if (delays[tile] <= 0) {
			delays[tile] = value;
			return true;
		}
		return false;
	}
	static void Update() {
		Tile[] toDecrement = delays.Keys.ToArray();
		foreach (Tile tile in toDecrement) delays[tile]--;
	}
	class Comparer : EqualityComparer<Tile> {
		public override bool Equals(Tile x, Tile y) => x == y;
		public override int GetHashCode([DisallowNull] Tile obj) => obj.GetHashCode();
	}
	class TileSoundDelaySystem : ModSystem {
		public override void PostUpdateWorld() => Update();
	}
}
