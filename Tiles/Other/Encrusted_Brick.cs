using Origins.Dev;
using Origins.Tiles.Riven;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Other {
	public class Encrusted_Brick : OriginTile, ICustomWikiStat {
        public string[] Categories => [
            WikiCategories.Brick
        ];
        public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			AddMapEntry(new Color(70, 110, 177));
			DustType = DustID.Astra;
			HitSound = SoundID.Tink;
		}
	}
	public class Encrusted_Brick_Item : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Encrusted_Brick>());
		}
		public override void AddRecipes() {
			CreateRecipe(5)
			.AddIngredient(OriginTile.TileItem<Spug_Flesh>(), 5)
			.AddIngredient<Encrusted_Ore_Item>()
			.AddTile(TileID.Furnaces)
			.Register();
		}
	}
}
