using Origins.Dev;
using Origins.Tiles.Ashen;
using Origins.World.BiomeData;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Other {
	public class Sanguinite_Brick : OriginTile, ICustomWikiStat {
		public override string Texture => typeof(Lost_Brick).GetDefaultTMLName();
		public string[] Categories => [
            WikiCategories.Brick
        ];
        public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			AddMapEntry(new Color(255, 151, 151));
			DustType = Ashen_Biome.DefaultTileDust;
			HitSound = SoundID.Tink;
		}
	}
	public class Sanguinite_Brick_Item : ModItem {
		public override string Texture => typeof(Lost_Brick_Item).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Sanguinite_Brick>());
		}
		public override void AddRecipes() {
			CreateRecipe(5)
			.AddIngredient(OriginTile.TileItem<Tainted_Stone>(), 5)
			.AddIngredient<Sanguinite_Ore_Item>()
			.AddTile(TileID.Furnaces)
			.Register();
		}
	}
}
