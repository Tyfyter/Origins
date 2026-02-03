using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.Dev;
using Origins.Graphics;
using Origins.Tiles.Riven;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Ashen {
	[LegacyName("Witherleaf_Tree")]
	public class Artifiber_Tree : ModTree, IGlowingModTree {
		public string[] Categories => [
			WikiCategories.Plant
		];
		private static Mod Mod => Origins.instance;
		public override TreePaintingSettings TreeShaderSettings => new();
		public override TreeTypes CountsAsTreeType => TreeTypes.None;
		public static int[] AnchorTypes => [
			TileType<Ashen_Grass>(),
			TileType<Ashen_Jungle_Grass>(),
			TileType<Ashen_Murky_Sludge_Grass>(),
			TileType<Sootsand>()
		];

		public override void SetStaticDefaults() {
			GrowsOnTileId = AnchorTypes;
			//this.SetupGlowKeys();
		}
		public override int SaplingGrowthType(ref int style) => TileType<Artifiber_Tree_Sapling>();
		public override int DropWood() => ItemType<Artifiber_Item>();

		public override Asset<Texture2D> GetTexture() => Mod.Assets.Request<Texture2D>("Tiles/Ashen/Artifiber_Tree");
		public override Asset<Texture2D> GetTopTextures() => Mod.Assets.Request<Texture2D>("Tiles/Ashen/Artifiber_Tree_Tops");
		public override Asset<Texture2D> GetBranchTextures() => Mod.Assets.Request<Texture2D>("Tiles/Ashen/Artifiber_Tree_Branches");
		public override void SetTreeFoliageSettings(int i, int j, Tile tile, int xoffset, ref int treeFrame, int floorY, ref int topTextureFrameWidth, ref int topTextureFrameHeight) {
			tile = Main.tile[i, j];
			switch (tile.TileFrameX) {
				case 22:
				case 44:
				case 66:
				if (tile.TileFrameNumber > 0) {
					treeFrame += 3;
				}
				break;
			}
		}
		public (float r, float g, float b) LightEmission(int i, int j) {
			GetGlow(Main.tile[i, j], out float r, out float g, out float b);
			const float brightness = 0.4f;
			return (r * brightness, g * brightness, b * brightness);
		}
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			Vector3 glow = default;
			GetGlow(tile, out glow.X, out glow.Y, out glow.Z);
			color.DoFancyGlow(glow, tile.TileColor);
		}
		static void GetGlow(Tile tile, out float r, out float g, out float b) {
			r = 0;
			g = 0;
			b = 0;
			if (tile.TileFrameY >= 198 && tile.TileFrameX >= 22) {
				switch (tile.TileFrameX) {
					case 22:
					break;

					case 44:
					case 66:
					if (tile.TileFrameNumber == 0) return;
					break;
				}
			} else {
				switch ((tile.TileFrameX / 18, tile.TileFrameY / 18)) {
					case (0, 1):
					case (0, 2):
					case (0, 5):
					case (0, 8):
					case (0, 9):
					case (0, 10):
					case (0, 11):

					case (1, 5):

					case (2, 0):
					case (2, 1):
					case (2, 2):

					case (3, 0):
					case (3, 1):
					case (3, 2):
					case (3, 7):

					case (4, 1):
					case (4, 3):
					case (4, 4):
					case (4, 5):

					case (5, 1):

					case (6, 1):
					case (6, 2):
					case (6, 4):
					case (6, 6):
					case (6, 7):
					case (6, 8):
					break;

					default:
					return;
				}
			}
			r = 1;
			g = 0.525f;
			b = 0.298f;
		}
		public AutoLoadingAsset<Texture2D> TopTexture { get; } = typeof(Artifiber_Tree).GetDefaultTMLName("_Top");
		public CustomTilePaintLoader.CustomTileVariationKey TopPaintKey { get; set; }
		public AutoLoadingAsset<Texture2D> TopGlowTexture { get; } = typeof(Artifiber_Tree).GetDefaultTMLName("_Top_Glow");
		public CustomTilePaintLoader.CustomTileVariationKey TopGlowPaintKey { get; set; }
		public AutoLoadingAsset<Texture2D> BranchesTexture { get; } = typeof(Artifiber_Tree).GetDefaultTMLName("_Branches");
		public CustomTilePaintLoader.CustomTileVariationKey BranchesPaintKey { get; set; }
		public AutoLoadingAsset<Texture2D> BranchesGlowTexture { get; } = typeof(Artifiber_Tree).GetDefaultTMLName("_Branches_Glow");
		public CustomTilePaintLoader.CustomTileVariationKey BranchesGlowPaintKey { get; set; }
		AutoLoadingAsset<Texture2D> GlowTexture = typeof(Artifiber_Tree).GetDefaultTMLName("_Glow");
		AutoCastingAsset<Texture2D> IGlowingModTile.GlowTexture => GlowTexture;
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public Color GlowColor => Color.White;
	}
	[LegacyName("Witherleaf_Tree_Sapling")]
	public class Artifiber_Tree_Sapling : SaplingBase, IGlowingModTile {
		public static AutoLoadingAsset<Texture2D> GlowTexture = typeof(Artifiber_Tree_Sapling).GetDefaultTMLName() + "_Glow";
		public Color GlowColor => Color.White;
		AutoCastingAsset<Texture2D> IGlowingModTile.GlowTexture => GlowTexture;
		public override Color MapColor => new(130, 103, 85);
		public override int[] ValidAnchorTypes => Artifiber_Tree.AnchorTypes;
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			if (tile.TileFrameX / 18 != 2) color.DoFancyGlow(Color.OrangeRed.ToVector3(), tile.TileColor);
		}
		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
			drawData.glowTexture = GlowTexture;
			drawData.glowSourceRect = new Rectangle(drawData.tileFrameX, drawData.tileFrameY, 16, 16);
			drawData.glowColor = GlowColor;
		}
		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
			tileFrameX += 54;
		}
		public override void Load() => this.SetupGlowKeys();
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
	}
}