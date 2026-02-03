using CalamityMod.NPCs.TownNPCs;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Weapons.Ammo;
using Origins.Items.Weapons.Demolitionist;
using Origins.Items.Weapons.Ranged;
using PegasusLib.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.Tiles.Ashen.Beacon_Light_TE_System;
using static Terraria.GameContent.TextureAssets;

namespace Origins.Tiles.Ashen {
	[ReinitializeDuringResizeArrays]
	public class Catwalk : Platform_Tile, ISpecialFrameTile {
		public static bool[] Catwalks = TileID.Sets.Factory.CreateBoolSet();
		public override void OnLoad() {
			Item.OnAddRecipes += item => {
				Recipe.Create(item.type, 2)
				.AddIngredient(ModContent.ItemType<Scrap>())
				.Register();
				Recipe.Create(ModContent.ItemType<Scrap>())
				.AddIngredient(item.type, 2)
				.Register();
			};
			Item.ExtraStaticDefaults += item => {
				AMRSL_Skewer.AmmoCount[Type] = 1f / 4;
				AMRSL_Skewer.AmmoProjectile[Type] = ModContent.ProjectileType<AMRSL_Skewer_Sabot_Scrap>();
			};
		}
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Catwalks[Type] = true;
			DustType = DustID.Lihzahrd;
			HitSound = SoundID.Tink;
			AddMapEntry(new Color(72, 70, 79));
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
			static bool CanConnect(Tile left, Tile right) {
				if (left.IsHalfBlock == right.IsHalfBlock) return true;
				if (left.IsHalfBlock && right.Slope == SlopeType.SlopeDownRight) return true;
				if (right.IsHalfBlock && left.Slope == SlopeType.SlopeDownLeft) return true;
				return false;
			}
			Tile tile = Main.tile[i, j];
			ref short platformFrame = ref tile.TileFrameX;
			platformFrame = 0;
			Tile leftTile = Main.tile[i - 1, j];
			Tile rightTile = Main.tile[i + 1, j];
			Tile downLeft = Main.tile[i - 1, j + 1];
			Tile downRight = Main.tile[i + 1, j + 1];
			Tile upLeft = Main.tile[i - 1, j - 1];
			Tile upRight = Main.tile[i + 1, j - 1];
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
			if (left == Type && !CanConnect(leftTile, tile))
				left = -1;
			if (right == Type && !CanConnect(tile, rightTile))
				right = -1;
			if (left != -1 && left != Type && tile.IsHalfBlock)
				left = -1;
			if (right != -1 && right != Type && tile.IsHalfBlock)
				right = -1;
			if (left == -1 && upLeft.HasTile && upLeft.TileType == Type && upLeft.Slope == SlopeType.SlopeDownLeft)
				left = Type;
			if (right == -1 && upRight.HasTile && upRight.TileType == Type && upRight.Slope == SlopeType.SlopeDownRight)
				right = Type;
			if (left == Type && leftTile.Slope == SlopeType.SlopeDownRight && right != Type)
				right = -1;
			if (right == Type && rightTile.Slope == SlopeType.SlopeDownLeft && left != Type)
				left = -1;
			if (tile.Slope == SlopeType.SlopeDownLeft) {
				if (TileID.Sets.Platforms[rightTile.TileType] && rightTile.Slope == 0 && !rightTile.IsHalfBlock) {
					platformFrame = 468;
				} else if (!downRight.HasTile && (!TileID.Sets.Platforms[downRight.TileType] || downRight.Slope == SlopeType.SlopeDownRight)) {
					if (!leftTile.HasTile && (!TileID.Sets.Platforms[upLeft.TileType] || upLeft.Slope != SlopeType.SlopeDownLeft)) {
						platformFrame = 432;
					} else if (rightTile.HasTile && Catwalks[right] && rightTile.BlockType is BlockType.HalfBlock or BlockType.SlopeDownRight) {
						platformFrame = 504;
					} else {
						platformFrame = 360;
					}
				} else if (!leftTile.HasTile && (!TileID.Sets.Platforms[upLeft.TileType] || upLeft.Slope != SlopeType.SlopeDownLeft)) {
					platformFrame = 396;
				} else {
					platformFrame = 180;
				}
			} else if (tile.Slope == SlopeType.SlopeDownRight) {
				if (TileID.Sets.Platforms[leftTile.TileType] && leftTile.Slope == 0 && !leftTile.IsHalfBlock) {
					platformFrame = 450;
				} else if (!downLeft.HasTile && (!TileID.Sets.Platforms[downLeft.TileType] || downLeft.Slope == SlopeType.SlopeDownLeft)) {
					if (!rightTile.HasTile && (!TileID.Sets.Platforms[upRight.TileType] || upRight.Slope != SlopeType.SlopeDownRight)) {
						platformFrame = 414;
					} else if (leftTile.HasTile && Catwalks[left] && leftTile.BlockType is BlockType.HalfBlock or BlockType.SlopeDownLeft) {
						platformFrame = 486;
					} else {
						platformFrame = 342;
					}
				} else if (!rightTile.HasTile && (!TileID.Sets.Platforms[upRight.TileType] || upRight.Slope != SlopeType.SlopeDownRight)) {
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
		static bool IsCatwalk(Tile tile) => tile.HasTile && Catwalks[tile.TileType];
		public static void UpdateRailingFrame(int i, int j) {
			static bool IsNonCatwalkTile(Tile tile) => tile.HasTile && !Catwalks[tile.TileType] && TileLoader.GetTile(tile.TileType) is not Industrial_Door;
			static bool BlocksConnection(Tile tile) => IsNonCatwalkTile(tile) && (!Main.tileSolid[tile.TileType] || Main.tileNoAttach[tile.TileType]);
			bool canConnectLeft = !BlocksConnection(Main.tile[i - 1, j - 2]);
			bool canConnectRight = !BlocksConnection(Main.tile[i + 1, j - 2]);
			Tile tile = Main.tile[i, j];
			ref byte railingFrame = ref tile.Get<ExtraFrameData>().value;
			byte oldRailingFrame = railingFrame;
			railingFrame = 0;
			if (IsNonCatwalkTile(Main.tile[i, j - 2])) {
				railingFrame = 5;
				return;
			}
			byte tileFrame = (byte)(tile.TileFrameX / 18);
			const int max_connection_dist = 4;
			switch (tileFrame) {
				case 1:
				case 15:
				railingFrame = tileFrame;
				if (!IsCatwalk(Main.tile[i - 1, j])) railingFrame = 5;
				break;
				case 2:
				case 16:
				railingFrame = tileFrame;
				if (!IsCatwalk(Main.tile[i + 1, j])) railingFrame = 5;
				break;

				case 3:
				case 4:
				case 5:
				case 8:
				case 10:
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
				case 27:
				railingFrame = 8;
				break;
				case 20:
				case 22:
				case 24:
				case 26:
				case 28:
				railingFrame = 10;
				break;

				case 12:
				railingFrame = 5;
				if (IsCatwalk(Main.tile[i - 1, j])) goto case 0;
				break;

				case 13:
				railingFrame = 5;
				if (IsCatwalk(Main.tile[i + 1, j])) goto case 0;
				break;

				case 0: {
					if (!IsCatwalk(Main.tile[i - 1, j]) && !IsCatwalk(Main.tile[i + 1, j])) {
						railingFrame = 5;
						break;
					}
					railingFrame = 14;
					int k;
					for (k = 1; k <= max_connection_dist; k++) {
						Tile left = Main.tile[i - k, j];
						if (!IsCatwalk(left)) {
							if (k == 1) {
								left = Main.tile[i - k, j - 1];
								if (left.Slope == SlopeType.SlopeDownLeft) {
									railingFrame = 14;
								} else {
									railingFrame = 3;
								}
								k = int.MaxValue;
							}
							break;
						}
						if (left.Get<ExtraFrameData>().value == 14) break;
						Tile right = Main.tile[i + k, j];
						if (!IsCatwalk(right)) {
							if (k == 1) {
								right = Main.tile[i + k, j - 1];
								if (right.Slope == SlopeType.SlopeDownRight) {
									railingFrame = 14;
								} else {
									railingFrame = 4;
								}
								k = int.MaxValue;
							}
							break;
						}
						if (right.Get<ExtraFrameData>().value == 14) break;
					}
					if (k < max_connection_dist) {
						railingFrame = 6;
					}
					break;
				}
			}
			switch (railingFrame) {
				case 1:
				case 3:
				case 15:
				if (!canConnectLeft) railingFrame = 5;
				break;
				case 2:
				case 4:
				case 16:
				if (!canConnectRight) railingFrame = 5;
				break;

				case 0:
				case 6:
				case 14:
				switch ((canConnectLeft, canConnectRight)) {
					case (false, true):
					railingFrame = 3;
					break;
					case (true, false):
					railingFrame = 4;
					break;
					case (false, false):
					railingFrame = 5;
					break;
				}
				break;
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
			if (tile.IsHalfBlock) pos.Y += 16;
			int railingFrame = tile.Get<ExtraFrameData>().value;
			switch (railingFrame) {
				case 3:
				if (!Main.tile[i - 1, j - 2].HasFullSolidTile() || Main.tileNoAttach[Main.tile[i - 1, j - 2].TileType]) railingFrame = 2;
				break;

				case 4:
				if (!Main.tile[i + 1, j - 2].HasFullSolidTile() || Main.tileNoAttach[Main.tile[i + 1, j - 2].TileType]) railingFrame = 1;
				break;
			}
			Rectangle topFrame = new(railingFrame * 18, 3 * 18, 16, 16);
			switch (railingFrame) {
				case 8:
				pos.Y += 6;
				if (tile.TileFrameX > 24 * 18 && tile.TileFrameX < 27 * 18 && IsCatwalk(Main.tile[i - 1, j])) break;
				topFrame.Y -= 18;
				break;

				case 10:
				pos.Y += 6;
				if (tile.TileFrameX > 24 * 18 && tile.TileFrameX < 27 * 18 && IsCatwalk(Main.tile[i + 1, j])) break;
				topFrame.Y -= 18;
				break;
			}
			const bool draw_above_all = false;
			Lighting.GetCornerColors(i, j - 1, out VertexColors vertices);
			if (draw_above_all) {
				Catwalk_Railing_System.toDraw.Add((
					new Vector4(pos.X, pos.Y + 16, 16, 16),
					new Rectangle(railingFrame * 18, 4 * 18, 16, 16),
					vertices
				));
			} else {
				Main.tileBatch.Draw(
					TextureAssets.Tile[Type].Value,
					new Vector4(pos.X, pos.Y + 16, 16, 16),
					new Rectangle(railingFrame * 18, 4 * 18, 16, 16),
					vertices
				);
			}
			Lighting.GetCornerColors(i, j - 2, out vertices);
			if (draw_above_all) {
				Catwalk_Railing_System.toDraw.Add((
					new Vector4(pos, 16, 16),
					topFrame,
					vertices
				));
			} else {
				Main.tileBatch.Draw(
					TextureAssets.Tile[Type].Value,
					new Vector4(pos, 16, 16),
					topFrame,
					vertices
				);
			}
			return base.PreDraw(i, j, spriteBatch);
		}
	}
	public class Broken_Catwalk : Catwalk {
		public static int ID { get; private set; }
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type, 2)
				.AddIngredient(ModContent.ItemType<Scrap>(), 1)
				.AddCondition(Condition.InGraveyard)
				.Register();
				Recipe.Create(ModContent.ItemType<Scrap>())
				.AddIngredient(item.type, 2)
				.DisableDecraft()
				.Register();
			};
			Item.ExtraStaticDefaults += item => {
				AMRSL_Skewer.AmmoCount[Type] = 1f / 4;
				AMRSL_Skewer.AmmoProjectile[Type] = ModContent.ProjectileType<AMRSL_Skewer_Sabot_Scrap>();
			};
		}
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.tileSolid[Type] = false;
			Main.tileSolidTop[Type] = false;
			ID = Type;
			Main.OnPreDraw += Main_OnPreDraw;
			HitSound = SoundID.Tink;
		}
		public override void Unload() {
			Main.OnPreDraw -= Main_OnPreDraw;
		}
		static void Main_OnPreDraw(GameTime obj) {
			Main.tileSolid[ID] = true;
			Main.tileSolidTop[ID] = true;
		}
	}
	public struct ExtraFrameData : ITileData {
		public byte value;
	}
	public class Catwalk_Railing_System : ModSystem {
		public const string biome_name = "Origins:CatwalkRailing";
		public static List<(Vector4 destination, Rectangle sourceRectangle, VertexColors colors)> toDraw = [];
		internal static RenderTarget2D renderTarget;
		bool drawAny = false;
		public override void Load() {
			Overlays.Scene[biome_name] = new Catwalk_Railing_Overlay();
			if (Main.dedServ) return;
			Main.QueueMainThreadAction(SetupRenderTargets);
			Main.OnResolutionChanged += Resize;
		}
		public override void PostDrawTiles() {
			if (Main.renderNow || Main.renderCount == 0) drawAny = toDraw.Count > 0;
			if (drawAny != (Overlays.Scene[biome_name].Mode != OverlayMode.Inactive)) {
				if (drawAny)
					Overlays.Scene.Activate(biome_name, default);
				else
					Overlays.Scene[biome_name].Deactivate();
			}
		}
		public void Resize(Vector2 _) {
			if (Main.dedServ) return;
			renderTarget.Dispose();
			SetupRenderTargets();
		}
		void SetupRenderTargets() {
			if (renderTarget is not null && !renderTarget.IsDisposed) return;
			int width = Main.graphics.GraphicsDevice.PresentationParameters.BackBufferWidth;
			int height = Main.graphics.GraphicsDevice.PresentationParameters.BackBufferHeight;
			Main.offScreenRange = 192;
			/*if (width + Main.offScreenRange * 2 > 2048) {
				Main.offScreenRange = (2048 - width) / 2;
			}*/
			width += Main.offScreenRange * 2;
			height += Main.offScreenRange * 2;
			renderTarget = new RenderTarget2D(Main.instance.GraphicsDevice, width, height, false, Main.graphics.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);
		}
		public override void Unload() {
			Main.QueueMainThreadAction(renderTarget.Dispose);
			Main.OnResolutionChanged -= Resize;
			renderTarget = null;
		}
		public class Catwalk_Railing_Overlay() : Overlay(EffectPriority.High, RenderLayers.ForegroundWater) {
			public override void Draw(SpriteBatch spriteBatch) {
				if (toDraw.Count > 0) {
					Texture2D texture = TextureAssets.Tile[ModContent.TileType<Catwalk>()].Value;
					RenderTargetBinding[] oldRenderTargets = Main.graphics.GraphicsDevice.GetRenderTargets();
					Main.graphics.GraphicsDevice.SetRenderTarget(renderTarget);
					Main.graphics.GraphicsDevice.Clear(Color.Transparent);
					Main.tileBatch.Begin();
					for (int i = 0; i < toDraw.Count; i++) {
						(Vector4 destination, Rectangle sourceRectangle, VertexColors colors) = toDraw[i];
						Main.tileBatch.Draw(texture, destination, sourceRectangle, colors);
					}
					Main.tileBatch.End();
					Main.graphics.GraphicsDevice.UseOldRenderTargets(oldRenderTargets);
					toDraw.Clear();
				}
				spriteBatch.Draw(renderTarget, Main.sceneTilePos - Main.screenPosition, Color.White);
			}
			public override void Update(GameTime gameTime) { }
			public override void Activate(Vector2 position, params object[] args) { }
			public override void Deactivate(params object[] args) { }
			public override bool IsVisible() => true;
		}
	}
}
