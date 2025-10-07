﻿using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Ashen {
	public class Modular_Light_Fixture : OriginTile, IGlowingModTile {
		public Color GlowColor { get; }
		public override void SetStaticDefaults() {
			// Properties
			Main.tileLighted[Type] = true;
			Main.tileFrameImportant[Type] = false;
			Main.tileSolid[Type] = true;
			Main.tileNoAttach[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;
			TileID.Sets.DisableSmartInteract[Type] = true;
			TileID.Sets.AllowLightInWater[Type] = true;
			TileID.Sets.DontDrawTileSliced[Type] = true;

			DustType = DustID.Smoke;
			AdjTiles = [TileID.Torches];

			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);

			// Etc
			AddMapEntry(new Color(255, 200, 120));

			if (!Main.dedServ) GlowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
		}
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			Tile tile = Framing.GetTileSafely(i, j);
			bool CheckTile(int x, int y) => Framing.GetTileSafely(i + x, j + y).TileIsType(Type);
			bool CheckTileSolid(int x, int y) {
				Tile tile = Framing.GetTileSafely(i + x, j + y);
				return tile.HasTile && Main.tileSolid[tile.TileType];
			}
			if (CheckTile(0, 1)) {
				if (CheckTile(0, -1)) {
					tile.TileFrameX = 4 * 18;
					tile.TileFrameY = 18;
					if (Framing.GetTileSafely(i, j - 1).TileFrameX == 3 * 18) WorldGen.TileFrame(i, j - 1);
					if (Framing.GetTileSafely(i, j + 1).TileFrameX == 3 * 18) WorldGen.TileFrame(i, j + 1);
				} else if (!CheckTile(0, 2)) {
					tile.TileFrameX = 3 * 18;
					tile.TileFrameY = 0;
					if (Framing.GetTileSafely(i, j + 1).TileFrameX == 4 * 18) WorldGen.TileFrame(i, j + 1);
				} else {
					tile.TileFrameX = 4 * 18;
					tile.TileFrameY = 0;
				}
				return false;
			}
			if (CheckTile(0, -1)) {
				if (!CheckTile(0, -2)) {
					tile.TileFrameX = 3 * 18;
					tile.TileFrameY = 18;
					if (Framing.GetTileSafely(i, j - 1).TileFrameX == 4 * 18) WorldGen.TileFrame(i, j - 1);
				} else {
					tile.TileFrameX = 4 * 18;
					tile.TileFrameY = 2 * 18;
				}
				return false;
			}
			if (CheckTile(-1, 0) && !(CheckTile(-1, -1) || CheckTile(-1, 1))) {
				tile.TileFrameY = 2 * 18;
				if (CheckTile(1, 0) && !(CheckTile(1, -1) || CheckTile(1, 1))) {
					tile.TileFrameX = 6 * 18;
				} else {
					tile.TileFrameX = 7 * 18;
				}
				return false;
			}
			if (CheckTile(1, 0) && !(CheckTile(1, -1) || CheckTile(1, 1))) {
				tile.TileFrameY = 2 * 18;
				if (CheckTile(-1, 0) && !(CheckTile(-1, -1) || CheckTile(-1, 1))) {
					tile.TileFrameX = 6 * 18;
				} else {
					tile.TileFrameX = 5 * 18;
				}
				return false;
			}
			bool above = CheckTileSolid(0, -1);
			bool below = CheckTileSolid(0, 1);
			bool left = CheckTileSolid(-1, 0);
			bool right = CheckTileSolid(1, 0);
			if (above && below) {
				tile.TileFrameX = 18 * 2;
				tile.TileFrameY = 0;
			} else if (left && right) {
				tile.TileFrameX = 18;
				tile.TileFrameY = 0;
			} else if (above || below) {
				tile.TileFrameX = 0;
				tile.TileFrameY = 0;
			} else if (left || right) {
				tile.TileFrameX = 0;
				tile.TileFrameY = 18;
			} else {
				WorldGen.KillTile(i, j);
			}
			return false;
		}
		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
			Tile tile = Framing.GetTileSafely(i, j);
			if (tile.TileFrameX == 0) {
				bool CheckTileSolid(int x, int y) {
					Tile tile = Framing.GetTileSafely(i + x, j + y);
					return tile.HasTile && Main.tileSolid[tile.TileType];
				}
				if (tile.TileFrameY == 0) {
					if (CheckTileSolid(0, 1)) {
						drawData.tileSpriteEffect ^= SpriteEffects.FlipVertically;
					}
				} else {
					if (CheckTileSolid(1, 0)) {
						drawData.tileSpriteEffect ^= SpriteEffects.FlipHorizontally;
					}
				}
			}
		}
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			this.DrawTileGlow(i, j, spriteBatch);
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			r = 1.05f;
			g = 0.75f;
			b = 0f;
		}
		public override void Load() {
			this.SetupGlowKeys();
			Mod.AddContent(new TileItem(this));
		}
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
	}
}
