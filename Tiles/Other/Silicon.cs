using Microsoft.Xna.Framework;
using Origins.Tiles.Defiled;
using Origins.Tiles.Riven;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Tiles.Other {
    public class Silicon : OriginTile {
        public string[] Categories => new string[] {
            "Ore"
        };
        public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			Main.tileMerge[Type][TileID.Dirt] = true;
			Main.tileMerge[TileID.Dirt][Type] = true;
			AddMapEntry(new Color(60, 60, 60));
			MinPick = 35;
			MineResist = 3;
		}
	}
    public class Silicon_Item : ModItem {
        public override void SetStaticDefaults() {
            Item.ResearchUnlockCount = 100;
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.StoneBlock);
            Item.value = Item.sellPrice(copper: 44);
            Item.createTile = ModContent.TileType<Silicon>();
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.SandBlock, 3);
            recipe.AddTile(TileID.GlassKiln);
            recipe.Register();

            recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.EbonsandBlock, 3);
            recipe.AddTile(TileID.GlassKiln);
            recipe.Register();

            recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.CrimsandBlock, 3);
            recipe.AddTile(TileID.GlassKiln);
            recipe.Register();

            recipe = Recipe.Create(Type);
            recipe.AddIngredient(ModContent.ItemType<Defiled_Sand_Item>(), 3);
            recipe.AddTile(TileID.GlassKiln);
            recipe.Register();

            recipe = Recipe.Create(Type);
            recipe.AddIngredient(ModContent.ItemType<Silica_Item>(), 3);
            recipe.AddTile(TileID.GlassKiln);
            recipe.Register();

            /*recipe = Recipe.Create(Type);
            recipe.AddIngredient(ModContent.ItemType<Ashen_Sand_Item>(), 3);
            recipe.AddTile(TileID.GlassKiln);
            recipe.Register();*/
        }
    }
}
