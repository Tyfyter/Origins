using Microsoft.Xna.Framework;
using Origins.Tiles.Riven;
using Origins.World.BiomeData;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Walls {
	public class Riven_Flesh_Wall : ModWall {
		public override void SetStaticDefaults() {
			WallID.Sets.Conversion.Stone[Type] = true;
			WallID.Sets.CannotBeReplacedByWallSpread[Type] = true;
			Main.wallBlend[Type] = WallID.Stone;//what wall type this wall is considered to be when blending
			AddMapEntry(new Color(40, 140, 200));
			DustType = DustID.GemEmerald;
			DustType = Riven_Hive.DefaultTileDust;
		}
	}
	public class Riven_Flesh_Wall_Safe : Riven_Flesh_Wall {
		public override string Texture => "Origins/Walls/Riven_Flesh_Wall";
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.wallHouse[Type] = true;
		}
	}
	public class Riven_Flesh_Wall_Item : ModItem {
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.StoneWall);
			Item.createWall = WallType<Riven_Flesh_Wall_Safe>();
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 4)
			.AddIngredient(ItemType<Riven_Flesh_Item>())
			.AddTile(TileID.WorkBenches)
			.Register();

			Recipe.Create(ItemType<Riven_Flesh_Item>(), 1)
			.AddIngredient(Type, 4)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
	}
}
