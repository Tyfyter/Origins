using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Riven {
    public class Primordial_Permafrost : OriginTile, IRivenTile {
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
			DustType = DustID.Water_Snow;
		}
		public override void FloorVisuals(Player player) {
			player.slippy = true;
		}
	}
	public class Primordial_Permafrost_Item : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.PurpleIceBlock);
			Item.createTile = TileType<Primordial_Permafrost>();
		}
	}
}
