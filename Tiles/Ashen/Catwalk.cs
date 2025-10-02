using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Tiles.Ashen {
	[ReinitializeDuringResizeArrays]
	public class Catwalk : Platform_Tile, ISpecialFrameTile {
		public static bool[] Catwalks = TileID.Sets.Factory.CreateBoolSet();
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				/*Recipe.Create(item.type, 2)
				.AddIngredient(, 1)
				.Register();
				Recipe.Create()
				.AddIngredient(item.type, 2)
				.Register();*/
			};
		}
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Catwalks[Type] = true;
			DustType = DustID.Lihzahrd;
		}
		// For some reason this runs after tile framing for platforms
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			Main.tileSolid[Broken_Catwalk.ID] = true;
			Main.tileSolidTop[Broken_Catwalk.ID] = true;
			UpdatePlatformFrame(i, j);
			Main.tileSolid[Broken_Catwalk.ID] = false;
			Main.tileSolidTop[Broken_Catwalk.ID] = false;
			if (Main.tile[i, j].TileFrameX == 0) UpdateRailingFrame(i, j);
			OriginSystem.QueueSpecialTileFrames(i, j);
			return false;
		}

		public void SpecialFrame(int i, int j) {
			UpdateRailingFrame(i, j);
		}
		public void UpdatePlatformFrame(int i, int j) {
			Tile tile = Main.tile[i, j];
			ref short platformFrame = ref tile.TileFrameX;
			platformFrame = 0;
			Tile leftTile = Main.tile[i - 1, j];
			Tile rightTile = Main.tile[i + 1, j];
			Tile tile6 = Main.tile[i - 1, j + 1];
			Tile tile7 = Main.tile[i + 1, j + 1];
			Tile tile8 = Main.tile[i - 1, j - 1];
			Tile tile9 = Main.tile[i + 1, j - 1];
			int left = -1;
			int right = -1;
			if (leftTile != null && leftTile.HasTile) 
				left = (Main.tileStone[leftTile.TileType] ? 1 : ((!TileID.Sets.Platforms[leftTile.TileType]) ? leftTile.TileType : Type));
			if (rightTile != null && rightTile.HasTile) 
				right = (Main.tileStone[rightTile.TileType] ? 1 : ((!TileID.Sets.Platforms[rightTile.TileType]) ? rightTile.TileType : Type));
			if (right >= 0 && !Main.tileSolid[right]) 
				right = -1;
			if (left >= 0 && !Main.tileSolid[left]) 
				left = -1;
			if (left == Type && leftTile.IsHalfBlock != tile.IsHalfBlock) 
				left = -1;
			if (right == Type && rightTile.IsHalfBlock != tile.IsHalfBlock) 
				right = -1;
			if (left != -1 && left != Type && tile.IsHalfBlock) 
				left = -1;
			if (right != -1 && right != Type && tile.IsHalfBlock) 
				right = -1;
			if (left == -1 && tile8.HasTile && tile8.TileType == Type && tile8.Slope == SlopeType.SlopeDownLeft) 
				left = Type;
			if (right == -1 && tile9.HasTile && tile9.TileType == Type && tile9.Slope == SlopeType.SlopeDownRight) 
				right = Type;
			if (left == Type && leftTile.Slope == SlopeType.SlopeDownRight && right != Type) 
				right = -1;
			if (right == Type && rightTile.Slope == SlopeType.SlopeDownLeft && left != Type) 
				left = -1;
			if (tile.Slope == SlopeType.SlopeDownLeft) {
				if (TileID.Sets.Platforms[rightTile.TileType] && rightTile.Slope == 0 && !rightTile.IsHalfBlock) {
					platformFrame = 468;
				} else if (!tile7.HasTile && (!TileID.Sets.Platforms[tile7.TileType] || tile7.Slope == SlopeType.SlopeDownRight)) {
					if (!leftTile.HasTile && (!TileID.Sets.Platforms[tile8.TileType] || tile8.Slope != SlopeType.SlopeDownLeft)) {
						platformFrame = 432;
					} else {
						platformFrame = 360;
					}
				} else if (!leftTile.HasTile && (!TileID.Sets.Platforms[tile8.TileType] || tile8.Slope != SlopeType.SlopeDownLeft)) {
					platformFrame = 396;
				} else {
					platformFrame = 180;
				}
			} else if (tile.Slope == SlopeType.SlopeDownRight) {
				if (TileID.Sets.Platforms[leftTile.TileType] && leftTile.Slope == 0 && !leftTile.IsHalfBlock) {
					platformFrame = 450;
				} else if (!tile6.HasTile && (!TileID.Sets.Platforms[tile6.TileType] || tile6.Slope == SlopeType.SlopeDownLeft)) {
					if (!rightTile.HasTile && (!TileID.Sets.Platforms[tile9.TileType] || tile9.Slope != SlopeType.SlopeDownRight)) {
						platformFrame = 414;
					} else {
						platformFrame = 342;
					}
				} else if (!rightTile.HasTile && (!TileID.Sets.Platforms[tile9.TileType] || tile9.Slope != SlopeType.SlopeDownRight)) {
					platformFrame = 378;
				} else {
					platformFrame = 144;
				}
			} else if (left == Type && right == Type) {
				if (leftTile.Slope == SlopeType.SlopeDownRight && rightTile.Slope == SlopeType.SlopeDownLeft) {
					platformFrame = 252;
				} else if (leftTile.Slope == SlopeType.SlopeDownRight) {
					platformFrame = 216;
				} else if (rightTile.Slope == SlopeType.SlopeDownLeft) {
					platformFrame = 234;
				} else {
					platformFrame = 0;
				}
			} else if (left == Type && right == -1) {
				if (leftTile.Slope == SlopeType.SlopeDownRight) {
					platformFrame = 270;
				} else {
					platformFrame = 18;
				}
			} else if (left == -1 && right == Type) {
				if (rightTile.Slope == SlopeType.SlopeDownLeft) {
					platformFrame = 288;
				} else {
					platformFrame = 36;
				}
			} else if (left != Type && right == Type) {
				platformFrame = 54;
			} else if (left == Type && right != Type) {
				platformFrame = 72;
			} else if (left != Type && left != -1 && right == -1) {
				platformFrame = 108;
			} else if (left == -1 && right != Type && right != -1) {
				platformFrame = 126;
			} else {
				platformFrame = 90;
			}
			if (Main.tile[i, j - 1] != null && Main.tileRope[Main.tile[i, j - 1].TileType]) {
				WorldGen.TileFrame(i, j - 1);
			}
			if (Main.tile[i, j + 1] != null && Main.tileRope[Main.tile[i, j + 1].TileType]) {
				WorldGen.TileFrame(i, j + 1);
			}
		}
		public static void UpdateRailingFrame(int i, int j) {
			static bool IsCatwalk(Tile tile) => tile.HasTile && Catwalks[tile.TileType];
			Tile tile = Main.tile[i, j];
			ref byte railingFrame = ref tile.Get<ExtraFrameData>().value;
			byte oldRailingFrame = railingFrame;
			railingFrame = 0;
			byte tileFrame = (byte)(tile.TileFrameX / 18);
			const int max_connection_dist = 8;
			switch (tileFrame) {
				case 1:
				case 2:
				case 3:
				case 4:
				case 8:
				case 10:
				case 15:
				case 16:
				railingFrame = tileFrame;
				break;

				case 6:
				case 7:
				railingFrame = 5;
				break;

				case 19:
				case 21:
				case 23:
				case 25:
				railingFrame = 8;
				break;
				case 20:
				case 22:
				case 24:
				case 26:
				railingFrame = 10;
				break;

				case 0: {
					int k;
					for (k = 1; k <= max_connection_dist; k++) {
						Tile left = Main.tile[i - k, j];
						if (!IsCatwalk(left)) {
							if (k == 1) {
								left = Main.tile[i - k, j - 1];
								if (left.TileFrameX == 10 * 18) {
									railingFrame = 0;
								} else {
									railingFrame = 4;
								}
								k = int.MaxValue;
							}
							break;
						}
						if (left.Get<ExtraFrameData>().value == 0) break;
						Tile right = Main.tile[i + k, j];
						if (!IsCatwalk(right)) {
							if (k == 1) {
								left = Main.tile[i + k, j - 1];
								if (left.TileFrameX == 8 * 18) {
									railingFrame = 0;
								} else {
									railingFrame = 3;
								}
								k = int.MaxValue;
							}
							break;
						}
						if (right.Get<ExtraFrameData>().value == 0) break;
					}
					if (k < max_connection_dist) {
						railingFrame = 6;
					}
					break;
				}
			}
			if (railingFrame != 6 && railingFrame != oldRailingFrame) {
				int k;
				for (k = 1; k < max_connection_dist; k++) {
					Tile left = Main.tile[i - k, j];
					byte l = left.Get<ExtraFrameData>().value;
					if (!IsCatwalk(left) || l != 6) break;
				}
				if (k >= max_connection_dist) {
					OriginSystem.QueueSpecialTileFrames(i - k, j);
				}
				for (k = 1; k < max_connection_dist; k++) {
					Tile right = Main.tile[i + k, j];
					if (!IsCatwalk(right) || right.Get<ExtraFrameData>().value != 6) break;
				}
				if (k >= max_connection_dist) {
					OriginSystem.QueueSpecialTileFrames(i + k, j);
				}
			}
		}
		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
			Tile tile = Main.tile[i, j];
			Vector2 pos = new Vector2(i * 16, (j - 2) * 16) - Main.screenPosition;
			if (!Main.drawToScreen) {
				pos.X += Main.offScreenRange;
				pos.Y += Main.offScreenRange;
			}
			int railingFrame = tile.Get<ExtraFrameData>().value;
			switch (railingFrame) {
				case 3:
				if (!Main.tile[i - 1, j - 2].HasFullSolidTile()) railingFrame = 2;
				break;

				case 4:
				if (!Main.tile[i + 1, j - 2].HasFullSolidTile()) railingFrame = 1;
				break;
			}
			Rectangle topFrame = new(railingFrame * 18, 4 * 18, 16, 16);
			switch (railingFrame) {
				case 8:
				case 10:
				pos.Y += 6;
				if (tile.TileFrameX > 24 * 18) break;
				topFrame.Y -= 18;
				break;
			}
			Lighting.GetCornerColors(i, j - 1, out VertexColors vertices);
			Main.tileBatch.Draw(
				TextureAssets.Tile[Type].Value,
				new Vector4(pos.X, pos.Y + 16, 16, 16),
				new Rectangle(railingFrame * 18, 5 * 18, 16, 16),
				vertices
			);
			Lighting.GetCornerColors(i, j - 2, out vertices);
			Main.tileBatch.Draw(
				TextureAssets.Tile[Type].Value,
				new Vector4(pos, 16, 16),
				topFrame,
				vertices
			);
			return base.PreDraw(i, j, spriteBatch);
		}
	}
	public class Broken_Catwalk : Catwalk {
		public override string Texture => typeof(Catwalk).GetDefaultTMLName();
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.tileSolid[Type] = false;
			Main.tileSolidTop[Type] = false;
			ID = Type;
			Main.OnPreDraw += Main_OnPreDraw;
		}
		static void Main_OnPreDraw(GameTime obj) {
			Main.tileSolid[ID] = true;
			Main.tileSolidTop[ID] = true;
		}
		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
			drawData.tileCache.TileFrameY = 18;
		}
	}
	public struct ExtraFrameData : ITileData {
		public byte value;
	}
}
