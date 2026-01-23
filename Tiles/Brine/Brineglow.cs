using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Graphics;
using Origins.Items.Other.Testing;
using Origins.Journal;
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
	public class Brineglow : OriginTile, IGlowingModTile {
		public string[] Categories => [
			WikiCategories.Plant
		];
		public AutoCastingAsset<Texture2D> GlowTexture { get; set; }
		public Color GlowColor => Color.White;
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			if (Glows(tile)) color.DoFancyGlow(new(0, 0, MathHelper.Max(1 - color.Z * 0.5f, 0)), tile.TileColor);
		}
		public static bool Glows(int frameNumX, int frameNumY) {
			switch ((frameNumX, frameNumY)) {
				case (0, 0):
				case (3, 0):
				case (4, 0):
				case (5, 0):
				case (8, 0):

				case (5, 1):
				case (10, 1):
				case (11, 1):
				case (12, 1):

				case (1, 2):
				case (2, 2):
				case (3, 2):
				case (6, 2):
				case (8, 2):
				case (10, 2):
				case (11, 2):

				case (1, 3):
				case (2, 3):
				case (5, 3):
				case (6, 3):
				case (7, 3):
				case (8, 3):
				case (10, 3):

				case (2, 4):
				case (3, 4):
				case (5, 4):
				return true;
			}
			return false;
		}
		public static bool Glows(Tile tile) => Glows(tile.TileFrameX / 18, tile.TileFrameY / 18);
		public override void SetStaticDefaults() {
			if (!Main.dedServ) {
				GlowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
			}
			Main.tileFrameImportant[Type] = true;
			Main.tileObsidianKill[Type] = true;
			Main.tileCut[Type] = true;
			Main.tileNoFail[Type] = true;
			Main.tileSpelunker[Type] = true;
			Main.tileWaterDeath[Type] = false;
			Main.tileLighted[Type] = true;
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
		public override IEnumerable<Item> GetItemDrops(int i, int j) {
			if (Glows(Framing.GetTileSafely(i, j))) yield return new Item(ItemType<Brineglow_Item>());
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			if (Glows(Framing.GetTileSafely(2, 2))) {
				r = 0.19f;
				g = 0.33f;
				b = 0.44f;
			}
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
			bool alreadyGlows = tile.TileFrameX < 0 ? WorldGen.genRand.NextBool(3) : Glows(tile);
			List<(int x, int y)> frames = [
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
				frames.Remove((below.TileFrameX / 18, below.TileFrameY / 18));
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
				frames.Remove((above.TileFrameX / 18, above.TileFrameY / 18));
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
			if (!frames.Contains((tile.TileFrameX / 18, tile.TileFrameY / 18))) {
				int frameNumX = -1;
				int frameNumY = 0;
				while (frames.Count > 0 && (frameNumX == -1 || (Glows(frameNumX, frameNumY) != alreadyGlows))) {
					int index = WorldGen.genRand.Next(frames.Count);
					(frameNumX, frameNumY) = frames[index];
					frames.RemoveAt(index);
				}
				tile.TileFrameX = (short)(frameNumX * 18);
				tile.TileFrameY = (short)(frameNumY * 18);
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
				Rectangle frame = new(0, 0, 16, 16);//2
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
						Main.spriteBatch.Draw(
							this.GetGlowTexture(tile.TileColor),
							position,
							frame,
							Color.White,
							0,
							new Vector2(0, 0),
							1f,
							spriteEffects,
						0f);
						position.Y += 16;
					}

					/*float windGridPush = Main.instance.TilesRenderer.GetWindGridPush(i, k, 20, 0.01f);
					ref short wind = ref tile.Get<TileExtraVisualData>().TileFrameX;
					windGridPush = wind / 128f;
					frame.X = tile.TileFrameX;
					bool glows = Glows(tile);
					for (int l = 0; l < 8; l++) {
						//offset += windGridPush;
						position.X += (windGridPush + lastWindGridPush) * 1.5f;
						frame.Y = tile.TileFrameY + (7 - l) * 2;
						//Dust.NewDustPerfect(position + Main.screenPosition, 6, Vector2.Zero).noGravity = true;
						Main.spriteBatch.Draw(
							TextureAssets.MagicPixel.Value,
							position,
							new Rectangle(0, 0, 1, 1),
							new Color(l / 8f, frame.X / (float)tileDrawTexture.Width, frame.Y / (float)tileDrawTexture.Height),
							0,
							new Vector2(0, 0),
							new Vector2(16, 2),
							spriteEffects,
						0f);
						if (glows) {
							Lighting.AddLight(position + Main.screenPosition - inexplicableOffset, 0, 0, 0.25f);
						}
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
						Main.spriteBatch.Draw(
							GlowTexture,
							position,
							frame,
							Color.White,
							0,
							new Vector2(0, 0),
							1f,
							spriteEffects, 
						0f);
						position.Y -= 2;
					}
					lastWindGridPush = windGridPush;
					wind = (short)((wind / 8f) * 7);
					ref short nextWind = ref Framing.GetTileSafely(i, k - 1).Get<TileExtraVisualData>().TileFrameX;
					nextWind = (short)MathHelper.Clamp(nextWind + wind * 0.1f, -64, 64);
					//Main.instance.TilesRenderer.Wind.GetWindTime(i, k, 20, out int windTime, out int dirX, out int dirY);
					//if (windTime > 1 && dirX != 0 && dirY != 0) WindMethods.SetWindTime(Main.instance.TilesRenderer.Wind, i, k - 1, windTime, dirX, dirY);
					//*/
				}
			}
			return false;
		}
		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
			//offsetY = -2; // This is -1 for tiles using StyleAlch, but vanilla sets to -2 for herbs, which causes a slight visual offset between the placement preview and the placed tile. 
		}

		public override bool IsTileSpelunkable(int i, int j) => true;
		public override void Load() => this.SetupGlowKeys();
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
	}
	public class Brineglow_Item : ModItem, IJournalEntrySource<Brineglow_Item.Brineglow_Entry> {
		public class Brineglow_Entry : JournalEntry {
			public override string TextKey => "Brineglow";
			public override JournalSortIndex SortIndex => new("Brine_Fiend", 2);
		}
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 5;
		}
		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
			Item.value = Item.sellPrice(copper: 30);
			Item.rare = ItemRarityID.Orange;
		}
	}
	public class Brineglow_Debug_Item : TestingItem {
		public override string Texture => "Origins/Tiles/Brine/Brineglow_Item";
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.TitaniumOre);
			Item.createTile = TileType<Brineglow>();

			Item.maxStack = 9999;
		}
	}
}
