using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Brine {
    public class Underwater_Vine : OriginTile {
        public string[] Categories => [
            "Plant"
        ];
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileObsidianKill[Type] = true;
			Main.tileCut[Type] = true;
			Main.tileNoFail[Type] = true;
			Main.tileSpelunker[Type] = true;
			Main.tileWaterDeath[Type] = false;
			TileID.Sets.ReplaceTileBreakUp[Type] = true;
			TileID.Sets.IgnoredInHouseScore[Type] = true;
			TileID.Sets.IgnoredByGrowingSaplings[Type] = true;
			TileID.Sets.TileCutIgnore.IgnoreDontHurtNature[Type] = true;
			TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]); // Make this tile interact with golf balls in the same way other plants do

			LocalizedText name = CreateMapEntryName();
			AddMapEntry(new Color(37, 128, 109), name);

			TileObjectData.newTile.CopyFrom(TileObjectData.StyleAlch);
			TileObjectData.newTile.WaterDeath = false;
			TileObjectData.newTile.WaterPlacement = LiquidPlacement.OnlyInFullLiquid;
			TileObjectData.newTile.AnchorTop = new(AnchorType.SolidTile | AnchorType.AlternateTile, 1, 0);
			TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
			TileObjectData.newTile.AnchorValidTiles = [
				TileType<Peat_Moss>(),
				TileType<Baryte>(),
				TileID.Mud,
			];
			TileObjectData.newTile.AnchorAlternateTiles = [
				Type
			];
			TileObjectData.addTile(Type);

			HitSound = SoundID.Grass;
			DustType = DustID.GrassBlades;
		}
		public override void RandomUpdate(int i, int j) {
			if (!Framing.GetTileSafely(i, j + 1).HasTile) {
				const int min_chance = 6;
				int count = 1;
				for (int k = 1; k < min_chance; k++) {
					if (!Framing.GetTileSafely(i, j - k).TileIsType(Type)) break;
					count++;
					if (count >= min_chance) break;
				}
				if (WorldGen.genRand.NextBool(1 + count / 2) && TileObject.CanPlace(i, j + 1, Type, 0, 0, out TileObject objectData, false, checkStay: true)) {
					objectData.style = 0;
					objectData.alternate = 0;
					objectData.random = 0;
					TileObject.Place(objectData);
					WorldGen.TileFrame(i, j);
				}
			}
		}
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			Tile tile = Framing.GetTileSafely(i, j);
			Tile below = Framing.GetTileSafely(i, j - 1);
			if (tile.LiquidAmount < 255 || tile.LiquidType != LiquidID.Water || !below.TileIsType(Type) && !below.TileIsType(TileType<Peat_Moss>()) && !below.TileIsType(TileType<Baryte>())) {
				WorldGen.KillTile(i, j);
				return false;
			}
			List<(short x, short y)> frames = [
				(0, 0),
				(4, 0),
				(5, 0),
				(10, 0),
				(11, 0),

				(0, 1),
				(1, 1),
				(2, 1),
				(3, 1),
				(4, 1),
				(5, 1),
				(6, 1),
				(7, 1),
				(8, 1),
				(10, 1),
				(11, 1),

				(0, 2),
				(4, 2),
				(5, 2),
				(6, 2),
				(7, 2),
				(8, 2),
				(10, 2),
				(11, 2),
			];
			if (below.TileIsType(Type)) {
				frames.Remove(((short)(below.TileFrameX / 18), (short)(below.TileFrameY / 18)));
			} else {
				frames = [
					(1, 2),
					(2, 2),
					(3, 2),

					(6, 3),
					(7, 3),
					(8, 3),

					(0, 4),
					(1, 4),
					(2, 4),
					(3, 4),
					(4, 4),
					(5, 4),
				];
			}
			Tile above = Framing.GetTileSafely(i, j + 1);
			if (above.TileIsType(Type)) {
				frames.Remove(((short)(above.TileFrameX / 18), (short)(above.TileFrameY / 18)));
			} else {
				frames = [
					(1, 0),
					(2, 0),
					(3, 0),
					(6, 0),
					(7, 0),
					(8, 0),
					(9, 0),
					(12, 0),

					(9, 1),
					(12, 1),

					(9, 2),
					(12, 2),

					(0, 3),
					(1, 3),
					(2, 3),
					(3, 3),
					(4, 3),
					(5, 3),
					(9, 3),
					(10, 3),
					(11, 3),

					(6, 4),
					(7, 4),
					(8, 4),
				];
			}
			if (!frames.Contains(((short)(tile.TileFrameX / 18), (short)(tile.TileFrameY / 18)))) {
				(tile.TileFrameX, tile.TileFrameY) = WorldGen.genRand.Next(frames);
				tile.TileFrameX *= 18;
				tile.TileFrameY *= 18;
			}
			return false;
		}
		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
			if (Framing.GetTileSafely(i, j - 1).TileType != Type) {
				bool spelunk = Main.LocalPlayer.findTreasure;
				bool isActiveAndNotPaused = !Main.gamePaused && Main.instance.IsActive;
				Texture2D tileDrawTexture = TextureAssets.Tile[Type].Value;
				SpriteEffects spriteEffects = SpriteEffects.FlipVertically;
				if (i % 2 == 0) {
					spriteEffects |= SpriteEffects.FlipHorizontally;
				}

				Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange, Main.offScreenRange);
				Vector2 position = new Vector2(i, j) * 16 - Main.screenPosition + zero;
				//position.Y += 2;
				Rectangle frame = new Rectangle(0, 0, 16, 16);//2
				//float lastWindGridPush = 0;
				for (int k = j; Framing.GetTileSafely(i, k).TileType == Type; k++) {
					Tile tile = Main.tile[i, k];
					if (TileDrawing.IsVisible(tile)) {
						Color color = Lighting.GetColor(i, k);
						if (spelunk) {
							if (color.R < 200) {
								color.R = 200;
							}
							if (color.G < 170) {
								color.G = 170;
							}
							if (isActiveAndNotPaused && Main.rand.NextBool(60)) {
								Dust dust = Dust.NewDustDirect(new Vector2(i * 16, k * 16), 16, 16, DustID.TreasureSparkle, 0f, 0f, 150, default(Color), 0.3f);
								dust.fadeIn = 1f;
								dust.velocity *= 0.1f;
								dust.noLight = true;
							}
						}
						//Dust.NewDustPerfect(position + Main.screenPosition - zero, DustID.AmberBolt, Vector2.Zero).noGravity = true;
						frame.X = tile.TileFrameX;
						frame.Y = tile.TileFrameY;
						Main.spriteBatch.Draw(
							tileDrawTexture,
							position,
							frame,
							color,
							0,
							new Vector2(0, 0),
							1f,
							spriteEffects,
						0f);
						position.Y += 16;
					}
				}
			}
			return false;
		}
		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
			//offsetY = -2; // This is -1 for tiles using StyleAlch, but vanilla sets to -2 for herbs, which causes a slight visual offset between the placement preview and the placed tile. 
		}
	}
}
