using Microsoft.Xna.Framework.Graphics;
using Origins.Tiles.Riven;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Dawn {
	public class Eden_Tree : ModTree {
		private Mod mod => Origins.instance;

		public static Eden_Tree Instance { get; private set; }
		public override TreePaintingSettings TreeShaderSettings => new();

		internal static void Load() {
			Instance = new Eden_Tree();
		}

		internal static void Unload() {
			Instance = null;
		}

		/*public override int CreateDust() {
			return ModContent.DustType<>();
		}*/

		public override int DropWood() {
			return ModContent.ItemType<Eden_Wood_Item>();
		}

		public override Asset<Texture2D> GetTexture() {
			return mod.Assets.Request<Texture2D>("Tiles/Dawn/Eden_Tree");
		}

		public override void SetStaticDefaults() {
			GrowsOnTileId = new int[] {
				ModContent.TileType<Riven_Flesh>(), //temporary
				ModContent.TileType<Riven_Grass>()
			};
		}

		public override Asset<Texture2D> GetTopTextures() {
			return mod.Assets.Request<Texture2D>("Tiles/Dawn/Eden_Tree_Tops");
		}

		public override Asset<Texture2D> GetBranchTextures() {
			return mod.Assets.Request<Texture2D>("Tiles/Dawn/Eden_Tree_Branches");
		}

		public override void SetTreeFoliageSettings(Tile tile, ref int xoffset, ref int treeFrame, ref int floorY, ref int topTextureFrameWidth, ref int topTextureFrameHeight) {

		}
	}
	public class Eden_Tree_Sapling : ModTile {
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = true;

			TileObjectData.newTile.Width = 1;
			TileObjectData.newTile.Height = 2;
			TileObjectData.newTile.Origin = new Point16(0, 1);
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
			TileObjectData.newTile.UsesCustomCanPlace = true;
			TileObjectData.newTile.CoordinateHeights = new[] { 16, 18 };
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.AnchorValidTiles = new[] { ModContent.TileType<Riven_Flesh>() }; //temporary
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.DrawFlipHorizontal = true;
			TileObjectData.newTile.WaterPlacement = LiquidPlacement.NotAllowed;
			TileObjectData.newTile.LavaDeath = true;
			TileObjectData.newTile.RandomStyleRange = 3;
			TileObjectData.newTile.StyleMultiplier = 3;
			TileObjectData.addTile(Type);

			ModTranslation name = CreateMapEntryName();
			name.SetDefault("Eden Sapling");

			AdjTiles = new int[] { TileID.Saplings };
		}

		public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;

		public override void RandomUpdate(int i, int j) {
			// A random chance to slow down growth
			if (WorldGen.genRand.NextBool(20)) {
				Tile tile = Framing.GetTileSafely(i, j); // Safely get the tile at the given coordinates
				bool growSucess; // A bool to see if the tree growing was successful.

				// Style 0 is for the ExampleTree sapling, and style 1 is for ExamplePalmTree, so here we check frameX to call the correct method.
				// Any pixels before 54 on the tilesheet are for ExampleTree while any pixels above it are for ExamplePalmTree
				if (tile.TileFrameX < 54)
					growSucess = WorldGen.GrowTree(i, j);
				else
					growSucess = WorldGen.GrowPalmTree(i, j);

				// A flag to check if a player is near the sapling
				bool isPlayerNear = WorldGen.PlayerLOS(i, j);

				//If growing the tree was a success and the player is near, show growing effects
				if (growSucess && isPlayerNear)
					WorldGen.TreeGrowFXCheck(i, j);
			}
		}

		public override void SetSpriteEffects(int i, int j, ref SpriteEffects effects) {
			if (i % 2 == 1)
				effects = SpriteEffects.FlipHorizontally;
		}
	}
}