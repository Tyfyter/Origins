using Origins.Tiles.Ashen;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Walls {
    public class Witherwood_Wall : ModWall {
		public override string Texture => typeof(Endowood_Wall).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			Main.wallBlend[Type] = WallID.Wood;
			AddMapEntry(new Color(30, 10, 30));
			Main.wallHouse[Type] = true;
			DustType = DustID.t_Granite;
		}
	}
	public class Witherwood_Wall_Item : ModItem {
		public override string Texture => typeof(Endowood_Wall_Item).GetDefaultTMLName();
		public override void SetDefaults() {
			Item.DefaultToPlaceableWall(WallType<Witherwood_Wall>());
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 4)
			.AddIngredient(ItemType<Witherwood_Item>())
			.AddTile(TileID.WorkBenches)
			.Register();

			Recipe.Create(ItemType<Witherwood_Item>(), 1)
			.AddIngredient(Type, 4)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
	}
}
