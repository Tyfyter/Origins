using Microsoft.Xna.Framework;
using Origins.Walls;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Dawn {
    public class Eden_Wood : OriginTile {
        public string[] Categories => new string[] {
            "Plant"
        };
        public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			AddMapEntry(new Color(150, 40, 40));
			mergeID = TileID.WoodBlock;
		}
	}
	public class Eden_Wood_Item : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Wood);
			Item.createTile = TileType<Eden_Wood>();
		}
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ModContent.ItemType<Eden_Wood_Wall_Item>(), 4);
            recipe.AddTile(TileID.WorkBenches);
            recipe.Register();

            recipe = Recipe.Create(ModContent.ItemType<Eden_Wood_Wall_Item>(), 4);
            recipe.AddIngredient(this);
            recipe.AddTile(TileID.WorkBenches);
            recipe.Register();
        }
    }
}
