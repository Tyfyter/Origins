using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Graphics;
using Origins.World.BiomeData;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Origins.Tiles.Riven {
    public class Exoskeletal_Tree : ModTree, IGlowingModTree {
        public string[] Categories => [
			WikiCategories.Plant
		];
		public static AutoLoadingAsset<Texture2D> GlowTexture = typeof(Exoskeletal_Tree).GetDefaultTMLName() + "_Glow";
		AutoCastingAsset<Texture2D> IGlowingModTile.GlowTexture => GlowTexture;
		public static AutoLoadingAsset<Texture2D> TopTexture = typeof(Exoskeletal_Tree).GetDefaultTMLName() + "_Tops";
		AutoLoadingAsset<Texture2D> IGlowingModTree.TopTexture => TopTexture;
		public static AutoLoadingAsset<Texture2D> TopGlowTexture = typeof(Exoskeletal_Tree).GetDefaultTMLName() + "_Tops_Glow";
		AutoLoadingAsset<Texture2D> IGlowingModTree.TopGlowTexture => TopGlowTexture;
		public Color GlowColor => new(GlowValue, GlowValue, GlowValue, GlowValue);
		public static float GlowValue => Riven_Hive.NormalGlowValue.GetValue();
		public (float r, float g, float b) LightEmission(int i, int j) {
			if (!HasScar(Main.tile[i, j])) return default;
			float glowValue = GlowValue;
			return (0.394f * glowValue, 0.879f * glowValue, 0.912f * glowValue);
		}
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			if (HasScar(tile)) color = Vector3.Max(color, new Vector3(0.394f, 0.879f, 0.912f) * GlowValue);
		}
		static bool HasScar(Tile tile) {
			if (tile.TileFrameY >= 198 && tile.TileFrameX == 22) return true;
			switch ((tile.TileFrameX / 18, tile.TileFrameY / 18)) {
				case (3, 0):
				case (3, 1):
				case (3, 2):

				case (4, 3):
				case (4, 4):
				case (4, 5):

				case (1, 6):
				case (2, 6):
				case (1, 7):
				case (2, 7):
				case (1, 8):
				case (2, 8):
				return false;
			}
			return true;
		}
		public override TreePaintingSettings TreeShaderSettings => new();
		public override TreeTypes CountsAsTreeType => TreeTypes.None;
		public override int DropWood() {
			return ModContent.ItemType<Marrowick_Item>();
		}

		public override Asset<Texture2D> GetTexture() {
			return ModContent.Request<Texture2D>(typeof(Exoskeletal_Tree).GetDefaultTMLName());
		}
		public static int[] AnchorTypes => [
			ModContent.TileType<Riven_Grass>(),
			ModContent.TileType<Silica>(),
			ModContent.TileType<Spug_Flesh>(),
			ModContent.TileType<Riven_Jungle_Grass>()
		];
		public override void SetStaticDefaults() {
			GrowsOnTileId = AnchorTypes;
			this.SetupGlowKeys();
		}
		public override Asset<Texture2D> GetTopTextures() {
			return Asset<Texture2D>.Empty;
		}
		public override Asset<Texture2D> GetBranchTextures() {
			return ModContent.Request<Texture2D>(typeof(Exoskeletal_Tree).GetDefaultTMLName() + "_Branches");
		}

		public override void SetTreeFoliageSettings(Tile tile, ref int xoffset, ref int treeFrame, ref int floorY, ref int topTextureFrameWidth, ref int topTextureFrameHeight) {

		}
		public override int SaplingGrowthType(ref int style) {
			style = 0;
			return ModContent.TileType<Exoskeletal_Tree_Sapling>();
		}
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public CustomTilePaintLoader.CustomTileVariationKey TopPaintKey { get; set; }
		public CustomTilePaintLoader.CustomTileVariationKey TopGlowPaintKey { get; set; }
		public CustomTilePaintLoader.CustomTileVariationKey BranchesPaintKey { get; set; }
		public CustomTilePaintLoader.CustomTileVariationKey BranchesGlowPaintKey { get; set; }
		public AutoLoadingAsset<Texture2D> BranchesTexture { get; }
		public AutoLoadingAsset<Texture2D> BranchesGlowTexture { get; }
	}
	public class Exoskeletal_Tree_Sapling : SaplingBase, IGlowingModTile {
		public static AutoLoadingAsset<Texture2D> GlowTexture = typeof(Exoskeletal_Tree_Sapling).GetDefaultTMLName() + "_Glow";
		AutoCastingAsset<Texture2D> IGlowingModTile.GlowTexture => GlowTexture;
		public Color GlowColor => new(GlowValue, GlowValue, GlowValue, GlowValue);
		public float GlowValue => Riven_Hive.NormalGlowValue.GetValue();
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			if (tile.TileFrameX / 18 != 2) color = Vector3.Max(color, new Vector3(0.394f, 0.879f, 0.912f) * GlowValue);
		}
		public override Color MapColor => new(200, 175, 160);
		public override int[] ValidAnchorTypes => Exoskeletal_Tree.AnchorTypes;
		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
			drawData.glowTexture = GlowTexture;
			drawData.glowSourceRect = new Rectangle(drawData.tileFrameX, drawData.tileFrameY, 16, 16);
			float glowValue = GlowValue;
			drawData.glowColor = new Color(glowValue, glowValue, glowValue, glowValue);
		}
		public override void Load() => this.SetupGlowKeys();
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
	}
}