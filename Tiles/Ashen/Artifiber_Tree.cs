using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Ashen {
	[LegacyName("Witherleaf_Tree")]
	public class Artifiber_Tree : ModTree/*, IGlowingModTree*/ {
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
/*
		public AutoLoadingAsset<Texture2D> TopTexture => throw new System.NotImplementedException();
		public AutoLoadingAsset<Texture2D> TopGlowTexture => throw new System.NotImplementedException();
		public AutoCastingAsset<Texture2D> GlowTexture => throw new System.NotImplementedException();

		public (float r, float g, float b) LightEmission => throw new System.NotImplementedException();
		public Color GlowColor => Color.White;*/

		public override void SetStaticDefaults() {
			GrowsOnTileId = AnchorTypes;
			//this.SetupGlowKeys();
		}
		public override int SaplingGrowthType(ref int style) => TileType<Artifiber_Tree_Sapling>();
		public override int DropWood() => ItemType<Artifiber_Item>();

		public override Asset<Texture2D> GetTexture() => Mod.Assets.Request<Texture2D>("Tiles/Ashen/Artifiber_Tree");
		public override Asset<Texture2D> GetTopTextures() => Mod.Assets.Request<Texture2D>("Tiles/Ashen/Artifiber_Tree_Tops");
		public override Asset<Texture2D> GetBranchTextures() => Mod.Assets.Request<Texture2D>("Tiles/Ashen/Artifiber_Tree_Branches");

		public override void SetTreeFoliageSettings(Tile tile, ref int xoffset, ref int treeFrame, ref int floorY, ref int topTextureFrameWidth, ref int topTextureFrameHeight) { }/*
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public CustomTilePaintLoader.CustomTileVariationKey TopPaintKey { get; set; }
		public CustomTilePaintLoader.CustomTileVariationKey TopGlowPaintKey { get; set; }*/
	}
	[LegacyName("Witherleaf_Tree_Sapling")]
	public class Artifiber_Tree_Sapling : SaplingBase {
		public override Color MapColor => new(130, 103, 85);
		public override int[] ValidAnchorTypes => Artifiber_Tree.AnchorTypes;
		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
			tileFrameX += 54;
		}
	}
}