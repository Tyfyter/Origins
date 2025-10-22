using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics;
using Origins.World;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Tiles.Ashen {
	public class Modular_Light_Fixture : OriginTile, IGlowingModTile {
		public static Vector3 LightColor => new(1.05f, 0.75f, 0f);
		public override void SetStaticDefaults() {
			// Properties
			Main.tileLighted[Type] = true;
			Main.tileFrameImportant[Type] = false;
			Main.tileSolid[Type] = false;
			Main.tileNoAttach[Type] = true;
			Main.tileBlockLight[Type] = false;
			TileID.Sets.CanPlaceNextToNonSolidTile[Type] = true;
			//TileID.Sets.DontDrawTileSliced[Type] = true;

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
				return tile.HasTile && (Main.tileSolid[tile.TileType] || tile.TileType == Type);
			}
			short litMod = 0;
			if (tile.TileFrameY >= 18 * 3) litMod = 18 * 3;

			AreaAnalysis bigAnalysis = AreaAnalysis.March(i, j, [new(0, 1), new(0, -1), new(1, 0), new(-1, 0)], pos => Framing.GetTileSafely(pos).TileIsType(Type), a => a.MaxY > a.MinY + 1);
			if (!bigAnalysis.Broke && bigAnalysis.MaxX - bigAnalysis.MinX > 3 && bigAnalysis.MaxY != bigAnalysis.MinY && bigAnalysis.Counted.Count == (bigAnalysis.MaxX + 1 - bigAnalysis.MinX) * (bigAnalysis.MaxY + 1 - bigAnalysis.MinY)) {
				for (int x = bigAnalysis.MinX; x <= bigAnalysis.MaxX; x++) {
					for (int y = bigAnalysis.MinY; y <= bigAnalysis.MaxY; y++) {
						tile = Framing.GetTileSafely(x, y);
						if (x == bigAnalysis.MinX) {
							tile.TileFrameX = 5 * 18;
						} else if (x == bigAnalysis.MinX + 1) {
							tile.TileFrameX = 6 * 18;
						} else if (x == bigAnalysis.MaxX - 1) {
							tile.TileFrameX = 8 * 18;
						} else if (x == bigAnalysis.MaxX) {
							tile.TileFrameX = 9 * 18;
						} else {
							tile.TileFrameX = 7 * 18;
						}
						tile.TileFrameY = litMod;
						if (y != bigAnalysis.MinY) tile.TileFrameY += 18;
					}
				}
				return false;
			} else if (tile.TileFrameX >= 5 * 18 && tile.TileFrameY - litMod < 18 * 2) {
				for (int k = 0; k < bigAnalysis.Counted.Count; k++) {
					Point pos = bigAnalysis.Counted[k];
					if (pos.X == i && pos.Y == j) continue;
					if (tile.TileFrameX >= 5 * 18 && tile.TileFrameY - litMod < 18 * 2) {
						OriginSystem.QueueTileFrames(pos.X, pos.Y);
					}
				}
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
				tile.TileFrameY += litMod;
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
				tile.TileFrameY += litMod;
				return false;
			}
			if (CheckTile(-1, 0) && !(CheckTile(-1, -1) || CheckTile(-1, 1))) {
				tile.TileFrameY = 2 * 18;
				if (CheckTile(1, 0) && !(CheckTile(1, -1) || CheckTile(1, 1))) {
					tile.TileFrameX = 6 * 18;
				} else {
					tile.TileFrameX = 7 * 18;
				}
				tile.TileFrameY += litMod;
				return false;
			}
			if (CheckTile(1, 0) && !(CheckTile(1, -1) || CheckTile(1, 1))) {
				tile.TileFrameY = 2 * 18;
				if (CheckTile(-1, 0) && !(CheckTile(-1, -1) || CheckTile(-1, 1))) {
					tile.TileFrameX = 6 * 18;
				} else {
					tile.TileFrameX = 5 * 18;
				}
				tile.TileFrameY += litMod;
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
			tile.TileFrameY += litMod;
			return false;
		}
		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
			Tile tile = Framing.GetTileSafely(i, j);
			if (tile.TileFrameX == 0) {
				bool CheckTileSolid(int x, int y) {
					Tile tile = Framing.GetTileSafely(i + x, j + y);
					return tile.HasTile && (Main.tileSolid[tile.TileType] || tile.TileType == Type);
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
		public override void HitWire(int i, int j) {
			Tile tile = Framing.GetTileSafely(i, j);
			short litMod = 18 * 3;
			if (tile.TileFrameY >= 18 * 3) litMod = 0;
			foreach (Point pos in CrawlEntireLight(i, j)) {
				Wiring.SkipWire(pos.X, pos.Y);
				tile = Framing.GetTileSafely(pos.X, pos.Y);
				tile.TileFrameY %= 18 * 3;
				tile.TileFrameY += litMod;
			}
		}
		static IEnumerable<Point> GetConnected(int i, int j) {
			Tile tile = Framing.GetTileSafely(i, j);
			switch ((tile.TileFrameX / 18, (tile.TileFrameY / 18) % 3)) {
				case (3, 0):
				yield return new(i, j + 1);
				break;
				case (3, 1):
				yield return new(i, j - 1);
				break;

				case (4, 0):
				yield return new(i, j + 1);
				break;
				case (4, 1):
				yield return new(i, j + 1);
				yield return new(i, j - 1);
				break;
				case (4, 2):
				yield return new(i, j - 1);
				break;

				case (5, 2):
				yield return new(i + 1, j);
				break;
				case (6, 2):
				yield return new(i + 1, j);
				yield return new(i - 1, j);
				break;
				case (7, 2):
				yield return new(i - 1, j);
				break;
			}
			yield break;
		}
		public HashSet<Point> CrawlEntireLight(int i, int j) {
			HashSet<Point> walked = [];
			Stack<Point> queue = new();
			queue.Push(new(i, j));
			while (queue.TryPop(out Point connected)) {
				if (Framing.GetTileSafely(connected).TileIsType(Type) && walked.Add(connected)) {
					foreach (Point item in GetConnected(connected.X, connected.Y)) {
						queue.Push(item);
					}
				}
			}
			return walked;
		}
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			if (OriginsModIntegrations.FancyLighting is null) this.DrawTileGlow(i, j, spriteBatch);
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			if (Main.tile[i, j].TileFrameY < 18 * 3) {
				(r, g, b) = LightColor;
			}
		}
		public override void Load() {
			this.SetupGlowKeys();
			Mod.AddContent(new TileItem(this));
		}
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public Color GlowColor => Color.White;
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) => color = Vector3.Max(color, LightColor);
	}
}
