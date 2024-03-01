using Microsoft.Xna.Framework;
using Origins.Walls;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Defiled {
	public class Endowood : OriginTile {
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			AddMapEntry(new Color(40, 10, 40));
			mergeID = TileID.WoodBlock;
		}
	}
	public class Endowood_Item : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Wood);
			Item.createTile = TileType<Endowood>();
		}
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ModContent.ItemType<Endowood_Wall_Item>(), 4);
            recipe.AddTile(TileID.WorkBenches);
            recipe.Register();

            recipe = Recipe.Create(ModContent.ItemType<Endowood_Wall_Item>(), 4);
            recipe.AddIngredient(this);
            recipe.AddTile(TileID.WorkBenches);
            recipe.Register();
        }
    }
}
