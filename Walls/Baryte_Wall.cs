using Origins.Tiles.Brine;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Walls {
	[LegacyName("Sulphur_Stone_Wall", "Dolomite_Wall")]
	public class Baryte_Wall : ModWall {
		public override void SetStaticDefaults() {
			Main.wallBlend[Type] = WallID.Stone;//what wall type this wall is considered to be when blending
			Origins.WallHammerRequirement[Type] = 70;
			Origins.WallBlocksMinecartTracks[Type] = true;
			WallID.Sets.CannotBeReplacedByWallSpread[Type] = true;
			AddMapEntry(new Color(6, 26, 19));
			DustType = DustID.GreenMoss;
		}
		public override void RandomUpdate(int i, int j) {
			Tile tile = Framing.GetTileSafely(i, j);
			if (j >= Main.worldSurface - 50 && (tile.LiquidAmount == 0 || (tile.LiquidAmount < 255 && tile.LiquidType == LiquidID.Water))) {
				tile.LiquidAmount = 255;
				tile.LiquidType = LiquidID.Water;
				WorldGen.SquareTileFrame(i, j);
			}
		}
	}
	[LegacyName("Sulphur_Stone_Wall_Safe", "Dolomite_Wall_Safe")]
	public class Baryte_Wall_Safe : Baryte_Wall {
		public override string Texture => "Origins/Walls/Baryte_Wall";
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.wallHouse[Type] = true;
		}
		public override void RandomUpdate(int i, int j) { }
	}
	[LegacyName("Sulphur_Stone_Wall_Item", "Dolomite_Wall_Item")]
	public class Baryte_Wall_Item : ModItem {
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.StoneWall);
			Item.createWall = WallType<Baryte_Wall_Safe>();
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Baryte_Item>()
			.AddTile(TileID.WorkBenches)
			.Register();
		}
	}
}
