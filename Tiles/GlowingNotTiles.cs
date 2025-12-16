using CalamityMod.Buffs.Potions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics;
using Origins.Reflection;
using Origins.Tiles.Ashen;
using Origins.Tiles.Defiled;
using Origins.Tiles.Riven;
using Origins.World.BiomeData;
using PegasusLib;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles {
	public class GlowingNotTiles : GlobalTile {
		public override void SetStaticDefaults() {
			Main.tileLighted[TileID.Trees] = true;
		}
		public override void PostDraw(int i, int j, int type, SpriteBatch spriteBatch) {
			if (type == TileID.Cactus) {
				Tile tile = Main.tile[i, j];
				WorldGen.GetCactusType(i, j, tile.TileFrameX, tile.TileFrameY, out int sandType);
				if (sandType == ModContent.TileType<Silica>()) {
					float glowValue = Riven_Hive.NormalGlowValue.GetValue();
					Color glowColor = new(glowValue, glowValue, glowValue, glowValue);
					OriginExtensions.DrawTileGlow(
						CustomTilePaintLoader.TryGetTileAndRequestIfNotReady(Riven_Cactus.GlowPaintKey, tile.TileColor, Riven_Cactus.GlowTexture),
						glowColor,
						i,
						j,
						spriteBatch
					);
				}
			} else if (type == TileID.DyePlants) {
				Tile tile = Main.tile[i, j];
				if (tile.TileFrameX == 204 || tile.TileFrameX == 202) {
					WorldGen.GetCactusType(i, j, tile.TileFrameX, tile.TileFrameY, out int sandType);
					if (sandType == ModContent.TileType<Silica>()) {
						float glowValue = Riven_Hive.NormalGlowValue.GetValue();
						Color glowColor = new(glowValue, glowValue, glowValue, glowValue);
						OriginExtensions.DrawTileGlow(
							CustomTilePaintLoader.TryGetTileAndRequestIfNotReady(Riven_Cactus.FruitGlowPaintKey, tile.TileColor, Riven_Cactus.FruitGlowTexture),
							glowColor,
							i,
							j,
							spriteBatch
						);
					}
				}
			} else if (OriginExtensions.GetTreeType(i, j) is ITree treeType) {
				if (treeType is IGlowingModTree glowingTree) {
					Tile tile = Main.tile[i, j];
					if (tile.TileFrameX != 22 || tile.TileFrameY < 198) {
						OriginExtensions.DrawTileGlow(
							glowingTree.GetGlowTexture(tile.TileColor),
							glowingTree.GlowColor,
							i,
							j,
							spriteBatch
						);
					} else {
						int treeFrame = WorldGen.GetTreeFrame(tile);
						int treeStyle = 0;
						if (WorldGen.GetCommonTreeFoliageData(i, j, 0, ref treeFrame, ref treeStyle, out _, out int topTextureFrameWidth, out int topTextureFrameHeight)) {
							Vector2 offset = new(Main.offScreenRange, Main.offScreenRange);
							Vector2 unscaledPosition = Main.Camera.UnscaledPosition;
							Vector2 zero = Vector2.Zero;
							Vector2 position = new Vector2(i * 16 - (int)unscaledPosition.X + 8, j * 16 - (int)unscaledPosition.Y + 16) + zero + offset;
							float windFactor = 0;
							if (tile.WallType == WallID.None) {
								windFactor = Main.instance.TilesRenderer.GetWindCycle(i, j, Main.instance.TilesRenderer._treeWindCounter);
								position.X += windFactor * 2f;
								position.Y += System.Math.Abs(windFactor) * 2f;
							}
							Rectangle frame = new(treeFrame * (topTextureFrameWidth + 2), 0, topTextureFrameWidth, topTextureFrameHeight);
							float rotation = windFactor * 0.08f;
							spriteBatch.Draw(
								glowingTree.GetTopTexture(tile.TileColor),
								position,
								frame,
								Lighting.GetColor(i, j),
								rotation,
								new Vector2(topTextureFrameWidth / 2, topTextureFrameHeight),
								1f,
								SpriteEffects.None,
							0f);
							spriteBatch.Draw(
								glowingTree.GetTopGlowTexture(tile.TileColor),
								position,
								frame,
								glowingTree.GlowColor,
								rotation,
								new Vector2(topTextureFrameWidth / 2, topTextureFrameHeight),
								1f,
								SpriteEffects.None,
							0f);
						}
					}
				}
			}
		}
		public override void DrawEffects(int i, int j, int type, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
			if (Main.tile[i, j].TileFrameX == 66 && Main.tile[i, j].TileFrameY == 242) {

			}
			if (OriginExtensions.GetTreeType(i, j) is Petrified_Tree) {
				Tile tile = Main.tile[i, j];
				if (tile.TileFrameX is 22 or 44 or 66 && tile.TileFrameY >= 198) {
					Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.CustomNonSolid);
				}
			}
		}
		public override void SpecialDraw(int i, int j, int type, SpriteBatch spriteBatch) {
			Tile tile = Main.tile[i, j];
			if (tile.TileFrameY >= 198 && OriginExtensions.GetTreeType(i, j) is Petrified_Tree) {
				int treeFrame = WorldGen.GetTreeFrame(tile);
				int treeStyle = 0;
				if (WorldGen.GetCommonTreeFoliageData(i, j, 0, ref treeFrame, ref treeStyle, out _, out int topTextureFrameWidth, out int topTextureFrameHeight)) {
					Vector2 position = new Vector2(i * 16, j * 16) - Main.screenPosition;
					float windFactor = 0;
					if (tile.WallType == WallID.None) {
						windFactor = Main.instance.TilesRenderer.GetWindCycle(i, j, Main.instance.TilesRenderer._treeWindCounter);
					}
					switch (tile.TileFrameX) {
						case 22: {
							position += new Vector2(8, 16);
							Vector2 offset = new(Main.offScreenRange, Main.offScreenRange);
							Vector2 unscaledPosition = Main.Camera.UnscaledPosition;
							Vector2 zero = Vector2.Zero;
							Rectangle frame = new(treeFrame * (topTextureFrameWidth + 2), 0, topTextureFrameWidth, topTextureFrameHeight);
							float rotation = windFactor * 0.08f;
							position.X += windFactor * 2f;
							position.Y += Math.Abs(windFactor) * 2f;
							TangelaVisual.DrawTangela(
								Petrified_Tree.topsTangelaTexture,
								position,
								frame,
								rotation,
								new Vector2(topTextureFrameWidth / 2, topTextureFrameHeight),
								Vector2.One,
								SpriteEffects.None,
								i + j * 787
							);
							break;
						}
						case 44: {
							Vector2 position2 = position + new Vector2(16f, 12f);
							position2.X += windFactor;
							position2.X += Math.Abs(windFactor) * 2f;
							TangelaVisual.DrawTangela(
								Petrified_Tree.branchesTangelaTexture,
								position2,
								new Rectangle(0, treeFrame * 42, 40, 40),
								windFactor * 0.06f,
								new Vector2(40f, 24f),
								Vector2.One,
								SpriteEffects.None,
								i + j * 787
							);
							break;
						}
						case 66: {
							Vector2 position2 = position + new Vector2(0f, 18f);
							position2.X += windFactor;
							position2.X -= Math.Abs(windFactor) * 2f;
							TangelaVisual.DrawTangela(
								Petrified_Tree.branchesTangelaTexture,
								position2,
								new Rectangle(42, treeFrame * 42, 40, 40),
								windFactor * 0.06f,
								new Vector2(0f, 30f),
								Vector2.One,
								SpriteEffects.None,
								i + j * 787
							);
							break;
						}
					}
				}
			}
		}
		public override void ModifyLight(int i, int j, int type, ref float r, ref float g, ref float b) {
			if (OriginExtensions.GetTreeType(i, j) is IGlowingModTree glowingTree) {
				(r, g, b) = glowingTree.LightEmission;
			}
		}
	}
}
