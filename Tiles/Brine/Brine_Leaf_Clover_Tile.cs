using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Terraria;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Brine {
	public class Brine_Leaf_Clover_Tile : OriginTile {
        public string[] Categories => [
			WikiCategories.Plant
		];
        public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileObsidianKill[Type] = true;
			Main.tileCut[Type] = true;
			Main.tileNoFail[Type] = true;
			Main.tileWaterDeath[Type] = false;
			TileID.Sets.ReplaceTileBreakUp[Type] = true;
			TileID.Sets.IgnoredInHouseScore[Type] = true;
			TileID.Sets.IgnoredByGrowingSaplings[Type] = true;
			TileID.Sets.TileCutIgnore.IgnoreDontHurtNature[Type] = true;
			TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]); // Make this tile interact with golf balls in the same way other plants do

			AddMapEntry(new Color(28, 128, 56), CreateMapEntryName());

			TileObjectData.newTile.CopyFrom(TileObjectData.StyleAlch);
			TileObjectData.newTile.AnchorAlternateTiles = [];
			TileObjectData.newTile.WaterDeath = false;
			TileObjectData.newTile.WaterPlacement = Terraria.Enums.LiquidPlacement.OnlyInFullLiquid;
			TileObjectData.newTile.AnchorValidTiles = [
				TileType<Peat_Moss>(),
				TileType<Baryte>()
			];
			TileObjectData.addTile(Type);

			HitSound = SoundID.Grass;
			DustType = DustID.JungleGrass;
			RegisterItemDrop(ItemType<Items.Accessories.Brine_Leafed_Clover_4>());
		}

		public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) {
			if (i % 2 == 0) {
				spriteEffects = SpriteEffects.FlipHorizontally;
			}
		}

		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
			offsetY = -2; // This is -1 for tiles using StyleAlch, but vanilla sets to -2 for herbs, which causes a slight visual offset between the placement preview and the placed tile. 
		}

		public override bool IsTileSpelunkable(int i, int j) => true;
		public static void TryGrowOnTile(int i, int j) {
			if (!Framing.GetTileSafely(i, j - 1).HasTile && WorldGen.genRand.NextBool(1000)) {
				if (TileObject.CanPlace(i, j - 1, TileType<Brine_Leaf_Clover_Tile>(), 0, 0, out TileObject objectData, false, checkStay: true)) {
					objectData.style = 0;
					objectData.alternate = 0;
					objectData.random = 0;
					TileObject.Place(objectData);
				}
			}
		}
	}
}
