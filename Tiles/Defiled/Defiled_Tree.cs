using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Other.Consumables.Food;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Defiled {
	public class Defiled_Tree : ModTree {
		private static Mod mod => Origins.instance;
		public static Defiled_Tree Instance { get; private set; }
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
			Instance = new Defiled_Tree();
		}

		internal static void Unload() {
			Instance = null;
		}
		public override bool Shake(int x, int y, ref bool createLeaves) {
			if (!Origins.PlantLoader_ShakeTree(x, y, Main.tile[x, y].TileType) && WorldGen.genRand.NextBool(15)) {
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

		public override int SaplingGrowthType(ref int style) {
			style = 0;
			return ModContent.TileType<Defiled_Tree_Sapling>();
		}

		public override int DropWood() {
			return ModContent.ItemType<Defiled_Stone_Item>();//temporary drop type?
		}

		public override Asset<Texture2D> GetTexture() {
			return mod.Assets.Request<Texture2D>("Tiles/Defiled/Defiled_Tree");
		}
		public override Asset<Texture2D> GetTopTextures() {
			return mod.Assets.Request<Texture2D>("Tiles/Defiled/Defiled_Tree_Tops");
		}

		public override Asset<Texture2D> GetBranchTextures() {
			return mod.Assets.Request<Texture2D>("Tiles/Defiled/Defiled_Tree_Branches");
		}

		public override void SetTreeFoliageSettings(Tile tile, ref int xoffset, ref int treeFrame, ref int floorY, ref int topTextureFrameWidth, ref int topTextureFrameHeight) {

		}
	}
	public class Defiled_Tree_Sapling : ModTile {
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
			TileObjectData.newTile.AnchorValidTiles = new[] { ModContent.TileType<Defiled_Grass>(), ModContent.TileType<Defiled_Sand>(), ModContent.TileType<Defiled_Stone>() };
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.DrawFlipHorizontal = true;
			TileObjectData.newTile.WaterPlacement = LiquidPlacement.NotAllowed;
			TileObjectData.newTile.LavaDeath = true;
			TileObjectData.newTile.RandomStyleRange = 3;
			TileObjectData.newTile.StyleMultiplier = 3;
			/*TileObjectData.newSubTile.CopyFrom(TileObjectData.newTile);
			TileObjectData.newSubTile.AnchorValidTiles = new int[] { ModContent.TileType<ExampleSand>() };
			TileObjectData.addSubTile(1);*/
			TileObjectData.addTile(Type);

			ModTranslation name = CreateMapEntryName();
			name.SetDefault("{$Defiled} Sapling");
			AddMapEntry(new Color(200, 200, 200), name);

			TileID.Sets.TreeSapling[Type] = true;
			TileID.Sets.CommonSapling[Type] = true;
			TileID.Sets.SwaysInWindBasic[Type] = true;
			TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]);

			//sapling = true;
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