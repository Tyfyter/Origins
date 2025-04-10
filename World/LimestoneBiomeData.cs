using Origins.World.BiomeData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace Origins.World {
	public class Limestone_Cave : ModBiome {
		public override bool IsBiomeActive(Player player) {
			return OriginSystem.limestoneTiles > NeededTiles;
		}
		public const int NeededTiles = 400;
		public static class Gen {
			public static void StartLimestone(float i, float j) {
				
			}
		}
	}
}
