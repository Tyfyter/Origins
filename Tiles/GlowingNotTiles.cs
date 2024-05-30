using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics;
using Origins.Reflection;
using Origins.Tiles.Riven;
using Origins.World.BiomeData;
using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

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
					Color glowColor = new Color(glowValue, glowValue, glowValue, glowValue);
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
			} else if (OriginExtensions.GetTreeType(i, j) is IGlowingModTree glowingTree) {
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
						Vector2 offset = new Vector2(Main.offScreenRange, Main.offScreenRange);
						Vector2 unscaledPosition = Main.Camera.UnscaledPosition;
						Vector2 zero = Vector2.Zero;
						Vector2 position = new Vector2(i * 16 - (int)unscaledPosition.X + 8, j * 16 - (int)unscaledPosition.Y + 16) + zero + offset;
						float windFactor = 0;
						if (tile.WallType == WallID.None) {
							windFactor = Main.instance.TilesRenderer.GetWindCycle(i, j, TileDrawingMethods._treeWindCounter.GetValue(Main.instance.TilesRenderer));
							position.X += windFactor * 2f;
							position.Y += System.Math.Abs(windFactor) * 2f;
						}
						Rectangle frame = new Rectangle(treeFrame * (topTextureFrameWidth + 2), 0, topTextureFrameWidth, topTextureFrameHeight);
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
		public override void ModifyLight(int i, int j, int type, ref float r, ref float g, ref float b) {
			if (OriginExtensions.GetTreeType(i, j) is IGlowingModTree glowingTree) {
				(r, g, b) = glowingTree.LightEmission;
			}
		}
	}
}
