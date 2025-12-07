using Origins.Tiles.Defiled;
using Origins.World.BiomeData;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Walls {
	public class Hardened_Defiled_Sand_Wall : ModWall {
		public override void SetStaticDefaults() {
			WallID.Sets.AllowsUndergroundDesertEnemiesToSpawn[Type] = true;
			WallID.Sets.Conversion.HardenedSand[Type] = true;
			Main.wallBlend[Type] = WallID.HardenedSand;//what wall type this wall is considered to be when blending
			WallID.Sets.CannotBeReplacedByWallSpread[Type] = true;
			AddMapEntry(new Color(150, 150, 150));
			DustType = Defiled_Wastelands.DefaultTileDust;
		}
	}
	public class Hardened_Defiled_Sand_Wall_Safe : Hardened_Defiled_Sand_Wall {
		public override string Texture => "Origins/Walls/Hardened_Defiled_Sand_Wall";
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.wallHouse[Type] = true;
		}
	}
	public class Hardened_Defiled_Sand_Wall_Item : ModItem {
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.StoneWall);
			Item.createWall = WallType<Hardened_Defiled_Sand_Wall_Safe>();
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 4)
			.AddIngredient(ItemType<Hardened_Defiled_Sand_Item>())
			.AddTile(TileID.WorkBenches)
			.Register();

			Recipe.Create(ItemType<Hardened_Defiled_Sand_Item>(), 1)
			.AddIngredient(Type, 4)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
	}
}
