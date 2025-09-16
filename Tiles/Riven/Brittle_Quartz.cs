using Microsoft.Xna.Framework;
using Origins.World.BiomeData;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Riven {
	public class Brittle_Quartz : OriginTile, IRivenTile {
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			TileID.Sets.SandBiome[Type] = 1;
			TileID.Sets.isDesertBiomeSand[Type] = true;
			Main.tileMergeDirt[Type] = Main.tileMergeDirt[TileID.HardenedSand];
			Main.tileMerge[TileType<Silica>()][Type] = true;
			TileID.Sets.ForAdvancedCollision.ForSandshark[Type] = true;
			TileID.Sets.Conversion.Sandstone[Type] = true;
			AddMapEntry(new Color(150, 150, 150));
			mergeID = TileID.HardenedSand;
			DustType = DustID.Ghost;
		}
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			TileExtenstions.DoFraming(i, j, resetFrame, map: [(Type, 1), (TileType<Silica>(), 2), (TileType<Quartz>(), 2)], TileExtenstions.ExtraTileBlending);
			return false;
		}
	}
	public class Brittle_Quartz_Item : ModItem {
		public override void SetStaticDefaults() {
            ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<Silica_Item>();
			ItemTrader.ChlorophyteExtractinator.AddOption_FromAny(ItemID.Sandstone, Type);
            Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Brittle_Quartz>());
		}
	}
}
