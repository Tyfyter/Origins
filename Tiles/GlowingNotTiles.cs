using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Tiles.Riven;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Tiles {
	public class GlowingNotTiles : GlobalTile {
		public override void PostDraw(int i, int j, int type, SpriteBatch spriteBatch) {
			if (type == TileID.Cactus) {
				Tile tile = Main.tile[i, j];
				WorldGen.GetCactusType(i, j, tile.TileFrameX, tile.TileFrameY, out int sandType);
				if (sandType == ModContent.TileType<Silica>()) {
					Color lightColor = Color.White;// Lighting.GetColor(i, j);
					OriginExtensions.DrawTileGlow(
						Riven_Cactus.GlowTexture,
						new Color((lightColor.R + 255) / 510f, (lightColor.G + 255) / 510f, (lightColor.B + 255) / 510f, 0.5f),
						i,
						j,
						spriteBatch
					);
				}
			}
		}
	}
}
