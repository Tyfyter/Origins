using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Origins.Tiles.Defiled {
    public class Petrified_Tree : ModTree {
		private static Mod mod => Origins.instance;
		public static Petrified_Tree Instance { get; private set; }
		public override TreePaintingSettings TreeShaderSettings => new();
		public override TreeTypes CountsAsTreeType => TreeTypes.None;
		public override void SetStaticDefaults() {
			GrowsOnTileId = new int[] {
				ModContent.TileType<Defiled_Grass>(),
				ModContent.TileType<Defiled_Sand>(),
				ModContent.TileType<Defiled_Stone>()
			};
		}
		internal static void Load() {
			Instance = new Petrified_Tree();
		}

		internal static void Unload() {
			Instance = null;
		}
		/*public override bool Shake(int x, int y, ref bool createLeaves) {
			if (!Origins.PlantLoader_ShakeTree(x, y, Main.tile[x, y].TileType, out _) && WorldGen.genRand.NextBool(15)) {
				int type = WorldGen.genRand.NextBool() ? ModContent.ItemType<Bileberry>() : ModContent.ItemType<Prickly_Pear>();
				Item.NewItem(WorldGen.GetItemSource_FromTreeShake(x, y), x * 16, y * 16, 16, 16, type);
				createLeaves = true;
				return false;
			}
			return true;
		}
		/*public override int CreateDust() {
			return ModContent.DustType<>();
		}*/

		public override int TreeLeaf() {
			return mod.GetGoreSlot($"Gores/NPCs/DF_Effect_{(Main.rand.NextBool() ? "Medium" : "Small")}{Main.rand.Next(3) + 1}");//adds one because sprites use 1-based indices
		}
		public override int SaplingGrowthType(ref int style) => ModContent.TileType<Petrified_Tree_Sapling>();

		public override int DropWood() {
			return ModContent.ItemType<Endowood_Item>();
		}

		public override Asset<Texture2D> GetTexture() {
			return mod.Assets.Request<Texture2D>("Tiles/Defiled/Petrified_Tree");
		}
		public override Asset<Texture2D> GetTopTextures() {
			return mod.Assets.Request<Texture2D>("Tiles/Defiled/Petrified_Tree_Tops");
		}

		public override Asset<Texture2D> GetBranchTextures() {
			return mod.Assets.Request<Texture2D>("Tiles/Defiled/Petrified_Tree_Branches");
		}

		public override void SetTreeFoliageSettings(Tile tile, ref int xoffset, ref int treeFrame, ref int floorY, ref int topTextureFrameWidth, ref int topTextureFrameHeight) {

		}
	}
	public class Petrified_Tree_Sapling : SaplingBase {
		public override Color MapColor => new Color(200, 200, 200);
		public override int[] ValidAnchorTypes => new[] { ModContent.TileType<Defiled_Grass>(), ModContent.TileType<Defiled_Sand>(), ModContent.TileType<Defiled_Stone>() };
	}
}