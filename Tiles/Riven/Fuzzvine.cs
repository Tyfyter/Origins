using AltLibrary.Common.Hooks;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Riven {
	public class Fuzzvine : OriginTile, IRivenTile {
		int[] AnchorTiles;
		public override void SetStaticDefaults() {
			AnchorTiles = [
				ModContent.TileType<Riven_Grass>(),
				ModContent.TileType<Riven_Jungle_Grass>(),
				ModContent.TileType<Spug_Flesh>()
			];
			//Main.tileFrameImportant[Type] = true;
			Main.tileObsidianKill[Type] = true;
			Main.tileCut[Type] = true;
			Main.tileNoFail[Type] = true;
			Main.tileWaterDeath[Type] = false;
			TileID.Sets.TileCutIgnore.Regrowth[Type] = true;
			TileID.Sets.IsVine[Type] = true;
			TileID.Sets.ReplaceTileBreakDown[Type] = true;
			TileID.Sets.VineThreads[Type] = true;
			TileID.Sets.ReplaceTileBreakUp[Type] = true;
			TileID.Sets.IgnoredInHouseScore[Type] = true;
			TileID.Sets.IgnoredByGrowingSaplings[Type] = true;
			TileID.Sets.TileCutIgnore.IgnoreDontHurtNature[Type] = true;
			TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]); // Make this tile interact with golf balls in the same way other plants do

			LocalizedText name = CreateMapEntryName();
			AddMapEntry(new Color(37, 109, 128), name);

			HitSound = SoundID.Grass;
			DustType = DustID.BlueMoss;
			AltVines.AddVine(Type, AnchorTiles);
			OriginsSets.Tiles.MinionSlowdown[Type] = 0.25f;
		}
		public override void RandomUpdate(int i, int j) {
			Tile below = Framing.GetTileSafely(i, j + 1);
			if (!below.HasTile) {
				const int max_length = 9;
				int count = 1;
				for (int k = 1; k < max_length; k++) {
					if (!Framing.GetTileSafely(i, j - k).TileIsType(Type)) break;
					count++;
					if (count >= max_length) return;
				}
				if (WorldGen.genRand.NextBool(1 + count / 2)) {
					below.TileType = Type;
					below.HasTile = true;
					WorldGen.TileFrame(i, j, true);
				}
			}
		}
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			Tile above = Framing.GetTileSafely(i, j - 1);
			if (!above.HasTile || (above.TileType != Type && (!AnchorTiles.Contains(above.TileType) || !above.HasSolidFace(TileExtenstions.TileSide.Bottom)))) {
				WorldGen.KillTile(i, j);
				return false;
			}
			DoFrame(i, j, resetFrame);
			return false;
		}
		public void DoFrame(int i, int j, bool resetFrame) {
			Tile tile = Framing.GetTileSafely(i, j);
			if (tile.TileType != Type) return;
			Tile above = Framing.GetTileSafely(i, j - 1);
			Tile below = Framing.GetTileSafely(i, j + 1);
			if (!below.TileIsType(Type)) {
				tile.TileFrameY = 2 * 18;
				if (resetFrame) tile.TileFrameNumber = above.TileFrameNumber;
			} else if (!above.TileIsType(Type)) {
				tile.TileFrameY = 0 * 18;
				if (resetFrame) {
					tile.TileFrameNumber = Main.rand.Next(3);
					DoFrame(i, j + 1, true);
				}
			} else {
				tile.TileFrameY = 1 * 18;
				if (resetFrame) {
					tile.TileFrameNumber = above.TileFrameNumber;
					DoFrame(i, j + 1, true);
				}
			}
			tile.TileFrameX = (short)(tile.TileFrameNumber * 18);
		}
		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
			if (!Framing.GetTileSafely(i, j - 1).TileIsType(Type)) {
				Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.Vine);
			}
			return false;
		}
	}
	public class Fuzzvine_Lorg : OriginTile, IRivenTile {
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileObsidianKill[Type] = true;
			Main.tileCut[Type] = true;
			Main.tileNoFail[Type] = true;
			Main.tileWaterDeath[Type] = false;
			TileID.Sets.TileCutIgnore.Regrowth[Type] = true;
			TileID.Sets.ReplaceTileBreakDown[Type] = true;
			TileID.Sets.MultiTileSway[Type] = true;
			TileID.Sets.ReplaceTileBreakUp[Type] = true;
			TileID.Sets.IgnoredInHouseScore[Type] = true;
			TileID.Sets.IgnoredByGrowingSaplings[Type] = true;
			TileID.Sets.TileCutIgnore.IgnoreDontHurtNature[Type] = true;
			TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]); // Make this tile interact with golf balls in the same way other plants do

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2Top);
			TileObjectData.newTile.Width = 2;
			TileObjectData.newTile.Height = 5;
			TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.PlanterBox, TileObjectData.newTile.Width, 0);
			TileObjectData.newTile.AnchorValidTiles = [
				ModContent.TileType<Riven_Grass>(),
				ModContent.TileType<Riven_Jungle_Grass>(),
				ModContent.TileType<Spug_Flesh>()
			];
			TileObjectData.newTile.CoordinateHeights = [..Enumerable.Repeat(16, TileObjectData.newTile.Height)];
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.RandomStyleRange = 0;
			TileObjectData.addTile(Type);

			LocalizedText name = CreateMapEntryName();
			AddMapEntry(new Color(37, 109, 128), name);

			HitSound = SoundID.Grass;
			DustType = DustID.BlueMoss;
			OriginsSets.Tiles.MinionSlowdown[Type] = 0.50f;
		}
		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
			if (TileObjectData.IsTopLeft(Main.tile[i, j])) {
				Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.MultiTileVine);
			}
			return false;
		}
	}
}
