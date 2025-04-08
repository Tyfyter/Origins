using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PegasusLib;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ModLoader;
using ThoriumMod.Projectiles;

namespace Origins.Tiles.Defiled {
    public class Petrified_Tree : ModTree {
        public string[] Categories => [
            "Plant"
        ];
        private static Mod Mod => Origins.instance;
		public static Petrified_Tree Instance { get; private set; }
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
		internal static void Load() {
			Instance = new Petrified_Tree();
		}

		internal static void Unload() {
			Instance = null;
		}
		public override int TreeLeaf() {
			return Mod.GetGoreSlot($"Gores/NPCs/DF_Effect_{(Main.rand.NextBool() ? "Medium" : "Small")}{Main.rand.Next(3) + 1}");//adds one because sprites use 1-based indices
		}
		public override int SaplingGrowthType(ref int style) => ModContent.TileType<Petrified_Tree_Sapling>();

		public override int DropWood() {
			return ModContent.ItemType<Endowood_Item>();
		}

		public override Asset<Texture2D> GetTexture() {
			return Mod.Assets.Request<Texture2D>("Tiles/Defiled/Petrified_Tree");
		}
		public static AutoLoadingAsset<Texture2D> branchesTopsTexture = "Origins/Tiles/Defiled/Petrified_Tree_Tops_Tangela";
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
}