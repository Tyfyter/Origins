using Origins.Tiles.Ashen;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Walls {
    public class Artifiber_Wall : ModWall {
		public override void SetStaticDefaults() {
			Main.wallBlend[Type] = WallID.Wood;
			AddMapEntry(FromHexRGB(0x7F6554));
			Main.wallHouse[Type] = true;
			DustType = DustID.t_Granite;
		}
	}
	public class Artifiber_Wall_Item : ModItem {
		public override void SetDefaults() {
			Item.DefaultToPlaceableWall(WallType<Artifiber_Wall>());
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 4)
			.AddIngredient(ItemType<Artifiber_Item>())
			.AddTile(TileID.WorkBenches)
			.Register();

			Recipe.Create(ItemType<Artifiber_Item>(), 1)
			.AddIngredient(Type, 4)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
	}
}
