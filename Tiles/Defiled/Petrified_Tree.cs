using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dusts;
using PegasusLib;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod.Projectiles;

namespace Origins.Tiles.Defiled {
	public class Petrified_Tree : ModTree {
		public string[] Categories => [
			"Plant"
		];
		private static Mod Mod => Origins.instance;
		public override TreePaintingSettings TreeShaderSettings => new();
		public override TreeTypes CountsAsTreeType => TreeTypes.None;
		public static int[] AnchorTypes => [
			ModContent.TileType<Defiled_Grass>(),
			ModContent.TileType<Defiled_Sand>(),
			ModContent.TileType<Defiled_Stone>(),
			ModContent.TileType<Defiled_Jungle_Grass>()
		];
		public override void SetStaticDefaults() {
			GrowsOnTileId = AnchorTypes;
		}
		public override int TreeLeaf() {
			return ModContent.GoreType<Petrified_Tree_Leaf_Gore>();
		}
		public override int SaplingGrowthType(ref int style) => ModContent.TileType<Petrified_Tree_Sapling>();

		public override int DropWood() {
			return ModContent.ItemType<Endowood_Item>();
		}

		public override Asset<Texture2D> GetTexture() {
			return Mod.Assets.Request<Texture2D>("Tiles/Defiled/Petrified_Tree");
		}
		public static AutoLoadingAsset<Texture2D> topsTangelaTexture = "Origins/Tiles/Defiled/Petrified_Tree_Tops_Tangela";
		public override Asset<Texture2D> GetTopTextures() {
			return Mod.Assets.Request<Texture2D>("Tiles/Defiled/Petrified_Tree_Tops");
		}
		public static AutoLoadingAsset<Texture2D> branchesTangelaTexture = "Origins/Tiles/Defiled/Petrified_Tree_Branches_Tangela";
		public override Asset<Texture2D> GetBranchTextures() {
			return Mod.Assets.Request<Texture2D>("Tiles/Defiled/Petrified_Tree_Branches");
		}

		public override void SetTreeFoliageSettings(Tile tile, ref int xoffset, ref int treeFrame, ref int floorY, ref int topTextureFrameWidth, ref int topTextureFrameHeight) {

		}
	}
	public class Petrified_Tree_Sapling : SaplingBase {
		public override Color MapColor => new(200, 200, 200);
		public override int[] ValidAnchorTypes => Petrified_Tree.AnchorTypes;
		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
			tileFrameX += 54;
		}
	}
	public class Petrified_Tree_Leaf_Gore : Dust_Spawner_Gore {
		public override void SpawnDust(Vector2 Position, int Type, Vector2 Velocity) {
			for (int i = 0; i < 2; i++) {
				Vector2 velocity = Velocity.RotatedByRandom(0.1f);
				if (velocity.Y < 0 && Main.rand.NextBool()) velocity.Y = -velocity.Y;
				base.SpawnDust(Position, Main.rand.Next(Petrified_Tree_Leaf1.dustIDs), velocity);
			}
		}
	}
}