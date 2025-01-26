using Microsoft.Xna.Framework;
using Origins.Tiles.Riven;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Walls {
    public class Marrowick_Wall : ModWall {
		public override void SetStaticDefaults() {
			Main.wallBlend[Type] = WallID.Wood;
			AddMapEntry(new Color(130, 110, 90));
			Main.wallHouse[Type] = true;
			DustType = DustID.TintablePaint;
		}
	}
	public class Marrowick_Wall_Item : ModItem {
		
		public override void SetDefaults() {
			Item.DefaultToPlaceableWall(WallType<Marrowick_Wall>());
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 4)
			.AddIngredient(ItemType<Marrowick_Item>())
			.AddTile(TileID.WorkBenches)
			.Register();

			Recipe.Create(ItemType<Marrowick_Item>(), 1)
			.AddIngredient(Type, 4)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
	}
}
