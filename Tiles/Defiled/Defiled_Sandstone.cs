using Microsoft.Xna.Framework;
using Origins.Walls;
using Origins.World.BiomeData;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Defiled {
	public class Defiled_Sandstone : OriginTile, IDefiledTile {
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileMergeDirt[Type] = Main.tileMergeDirt[TileID.Sandstone];
			TileID.Sets.ForAdvancedCollision.ForSandshark[Type] = true;
			TileID.Sets.Conversion.Sandstone[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			//Main.tileMerge[TileID.Sandstone][Type] = true;
			//Main.tileMerge[Type] = Main.tileMerge[TileID.Sandstone];
			//Main.tileMerge[Type][TileID.Sandstone] = true;
			/*for(int i = 0; i < TileLoader.TileCount; i++) {
                Main.tileMerge[i][Type] = Main.tileMerge[i][TileID.Sandstone];
            }*/
			AddMapEntry(new Color(150, 150, 150));
			mergeID = TileID.Sandstone;
			AddDefiledTile();
			DustType = Defiled_Wastelands.DefaultTileDust;
		}
	}
	public class Defiled_Sandstone_Item : ModItem {
		public override void SetStaticDefaults() {
            ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Defiled_Sand_Item>()] = ModContent.ItemType<Defiled_Sandstone_Item>();
            ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Defiled_Sandstone_Item>()] = ModContent.ItemType<Defiled_Sand_Item>();
            Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Defiled_Sandstone>());
		}
        public override void AddRecipes() {
            CreateRecipe()
            .AddIngredient(ItemID.Sandstone)
            .AddTile(TileID.ChlorophyteExtractinator)
            .Register();

            CreateRecipe()
            .AddIngredient(ItemID.CorruptSandstone)
            .AddTile(TileID.ChlorophyteExtractinator)
            .Register();

            CreateRecipe()
            .AddIngredient(ItemID.CrimsonSandstone)
			.AddTile(TileID.ChlorophyteExtractinator)
            .Register();

            //CreateRecipe()
            //.AddIngredient(ModContent.ItemType<Brittle_Quartz>())
            //.AddTile(TileID.ChlorophyteExtractinator)
            //.Register();

            //CreateRecipe()
            //.AddIngredient(ModContent.ItemType<Ashen_Sandstone>())
            //.AddTile(TileID.ChlorophyteExtractinator)
            //.Register();
        }
    }
}
