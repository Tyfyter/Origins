using Microsoft.Xna.Framework.Graphics;
using Origins;
using Origins.Dusts;
using Origins.Tiles.Defiled;
using ReLogic.Content;
using Terraria;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Origins.Tiles.Ashen {
	public class Witherleaf_Tree : ModTree {
		public string[] Categories => [
			"Plant"
		];
		private static Mod Mod => Origins.instance;
		public override TreePaintingSettings TreeShaderSettings => new();
		public override TreeTypes CountsAsTreeType => TreeTypes.None;
		public static int[] AnchorTypes => [
			ModContent.TileType<Ashen_Grass>(),
			ModContent.TileType<Sootsand>(),
			ModContent.TileType<Ashen_Jungle_Grass>()
		];
		public override void SetStaticDefaults() {
			GrowsOnTileId = AnchorTypes;
		}
		public override int TreeLeaf() {
			return ModContent.GoreType<Witherleaf_Tree_Leaf_Gore>();
		}
		public override int SaplingGrowthType(ref int style) => ModContent.TileType<Witherleaf_Tree_Sapling>();

		public override int DropWood() {
			return ModContent.ItemType<Witherwood_Item>();
		}

		public override Asset<Texture2D> GetTexture() {
			return Mod.Assets.Request<Texture2D>("Tiles/Defiled/Petrified_Tree");
		}
		public override Asset<Texture2D> GetTopTextures() {
			return Mod.Assets.Request<Texture2D>("Tiles/Defiled/Petrified_Tree_Tops");
		}
		public override Asset<Texture2D> GetBranchTextures() {
			return Mod.Assets.Request<Texture2D>("Tiles/Defiled/Petrified_Tree_Branches");
		}

		public override void SetTreeFoliageSettings(Tile tile, ref int xoffset, ref int treeFrame, ref int floorY, ref int topTextureFrameWidth, ref int topTextureFrameHeight) {

		}
	}
	public class Witherleaf_Tree_Sapling : SaplingBase {
		public override string Texture => typeof(Petrified_Tree_Sapling).GetDefaultTMLName();
		public override Color MapColor => new(200, 200, 200);
		public override int[] ValidAnchorTypes => Witherleaf_Tree.AnchorTypes;
		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
			tileFrameX += 54;
		}
	}
	public class Witherleaf_Tree_Leaf_Gore : Dust_Spawner_Gore {
		public override void SpawnDust(Vector2 Position, int Type, Vector2 Velocity) {
			for (int i = 0; i < 2; i++) {
				Vector2 velocity = Velocity.RotatedByRandom(0.1f);
				if (velocity.Y < 0 && Main.rand.NextBool()) velocity.Y = -velocity.Y;
				base.SpawnDust(Position, Main.rand.Next(Petrified_Tree_Leaf1.dustIDs), velocity);
			}
		}
	}
}