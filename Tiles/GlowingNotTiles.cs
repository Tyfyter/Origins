using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Tiles.Riven;
using Origins.World.BiomeData;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Tiles {
	public class GlowingNotTiles : GlobalTile {
		public override void PostDraw(int i, int j, int type, SpriteBatch spriteBatch) {
			if (type == TileID.Cactus) {
				Tile tile = Main.tile[i, j];
				WorldGen.GetCactusType(i, j, tile.TileFrameX, tile.TileFrameY, out int sandType);
				if (sandType == ModContent.TileType<Silica>()) {
					float glowValue = Riven_Hive.NormalGlowValue.GetValue();
					Color glowColor = new Color(glowValue, glowValue, glowValue, glowValue);
					OriginExtensions.DrawTileGlow(
						Riven_Cactus.GlowTexture,
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
						Color glowColor = new Color(glowValue, glowValue, glowValue, glowValue);
						OriginExtensions.DrawTileGlow(
							Riven_Cactus.FruitGlowTexture,
							glowColor,
							i,
							j,
							spriteBatch
						);
					}
				}
			} else if (OriginExtensions.GetTreeType(i, j) is IGlowingModTile glowingTree) {
				OriginExtensions.DrawTileGlow(
					glowingTree.GlowTexture,
					glowingTree.GlowColor,
					i,
					j,
					spriteBatch
				);
			}
		}
	}
}
