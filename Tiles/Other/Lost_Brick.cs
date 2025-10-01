using Origins.Dev;
using Origins.Tiles.Defiled;
using Origins.World.BiomeData;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Other {
	public class Lost_Brick : OriginTile, ICustomWikiStat {
        public string[] Categories => [
            "Brick"
        ];
        public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			AddMapEntry(new Color(151, 151, 151));
			DustType = Defiled_Wastelands.DefaultTileDust;
			HitSound = SoundID.Tink;
		}
	}
	public class Lost_Brick_Item : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Lost_Brick>());
		}
		public override void AddRecipes() {
			CreateRecipe(5)
			.AddIngredient<Defiled_Stone_Item>(5)
			.AddIngredient<Lost_Ore_Item>()
			.AddTile(TileID.Furnaces)
			.Register();
		}
	}
}
