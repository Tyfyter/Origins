using Origins.Tiles.Defiled;
using Origins.World.BiomeData;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Ashen {
	public class Soot_Sandstone : OriginTile, IAshenTile {
		public override string Texture => typeof(Defiled_Sandstone).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			TileID.Sets.SandBiome[Type] = 1;
			TileID.Sets.isDesertBiomeSand[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileMergeDirt[Type] = Main.tileMergeDirt[TileID.Sandstone];
			Main.tileMerge[TileType<Sootsand>()][Type] = true;
			TileID.Sets.ForAdvancedCollision.ForSandshark[Type] = true;
			TileID.Sets.Conversion.Sandstone[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			//Main.tileMerge[TileID.Sandstone][Type] = true;
			//Main.tileMerge[Type] = Main.tileMerge[TileID.Sandstone];
			//Main.tileMerge[Type][TileID.Sandstone] = true;
			/*for(int i = 0; i < TileLoader.TileCount; i++) {
                Main.tileMerge[i][Type] = Main.tileMerge[i][TileID.Sandstone];
            }*/
			AddMapEntry(new Color(255, 150, 150));
			mergeID = TileID.Sandstone;
			DustType = Ashen_Biome.DefaultTileDust;
		}
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			TileExtenstions.DoFraming(i, j, resetFrame, map: [(Type, 1), (TileType<Sootsand>(), 2)], TileExtenstions.ExtraTileBlending);
			return false;
		}
	}
	public class Soot_Sandstone_Item : ModItem {
		public override string Texture => typeof(Defiled_Sandstone_Item).GetDefaultTMLName();
		public override void SetStaticDefaults() {
            ItemID.Sets.ShimmerTransformToItem[ItemType<Soot_Sandstone_Item>()] = ItemType<Sootsand_Item>();
			ItemTrader.ChlorophyteExtractinator.AddOption_FromAny(ItemID.Sandstone, Type);
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Soot_Sandstone>());
		}
    }
}
