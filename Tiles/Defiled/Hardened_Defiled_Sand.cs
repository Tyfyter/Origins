using Microsoft.Xna.Framework;
using Origins.Tiles.Riven;
using Origins.World.BiomeData;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Defiled {
	public class Hardened_Defiled_Sand : OriginTile, IDefiledTile {
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			TileID.Sets.SandBiome[Type] = 1;
			TileID.Sets.isDesertBiomeSand[Type] = true;
			Main.tileBlockLight[Type] = true;
			TileID.Sets.ForAdvancedCollision.ForSandshark[Type] = true;
			TileID.Sets.Conversion.HardenedSand[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			/*Main.tileMergeDirt[Type] = Main.tileMergeDirt[TileID.HardenedSand];
            Main.tileMerge[TileID.HardenedSand][Type] = true;
            Main.tileMerge[Type] = Main.tileMerge[TileID.HardenedSand];
            Main.tileMerge[Type][TileID.HardenedSand] = true;*/
			AddMapEntry(new Color(200, 200, 200));
			mergeID = TileID.HardenedSand;
			AddDefiledTile();
			DustType = Defiled_Wastelands.DefaultTileDust;
		}
	}
	public class Hardened_Defiled_Sand_Item : ModItem {
		public override void SetStaticDefaults() {
			ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<Defiled_Sand_Item>();
			ItemTrader.ChlorophyteExtractinator.AddOption_FromAny(ItemID.HardenedSand, Type);
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Hardened_Defiled_Sand>());
		}
	}
}
