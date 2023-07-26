using Microsoft.Xna.Framework;
using Origins.World.BiomeData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Riven {
	public class Primordial_Permafrost : OriginTile, RivenTile {
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			TileID.Sets.Ices[Type] = true;
			TileID.Sets.IcesSlush[Type] = true;
			TileID.Sets.IcesSnow[Type] = true;
			TileID.Sets.Conversion.Ice[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			Main.tileMergeDirt[Type] = true;
			Main.tileMerge[Type] = Main.tileMerge[TileID.IceBlock];
			Main.tileMerge[Type][TileID.IceBlock] = true;
			Main.tileBlockLight[Type] = true;
			AddMapEntry(new Color(100, 200, 200));
			mergeID = TileID.IceBlock;
		}
		public override bool CreateDust(int i, int j, ref int type) {
			type = DustID.Water_Snow;
			return true;
		}
		public override void FloorVisuals(Player player) {
			player.slippy = true;
		}
	}
	public class Primordial_Permafrost_Item : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Primordial Permafrost");
			// Tooltip.SetDefault("A dangerous single-celled organism frozen...");
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.StoneBlock);
			Item.createTile = TileType<Primordial_Permafrost>();
		}
	}
}
