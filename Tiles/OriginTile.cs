using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Tiles {
	public abstract class OriginTile : ModTile {
		public static List<OriginTile> IDs { get; internal set; }
		public static List<int> DefiledTiles { get; internal set; }
		public int mergeID = -1;
		public override void Load() {
			if (IDs != null) {
				IDs.Add(this);
			} else {
				IDs = new List<OriginTile>() { this };
			}
			if (this is IDefiledTile) {
				if (DefiledTiles != null) {
					DefiledTiles.Add(Type);
				} else {
					DefiledTiles = [Type];
				}
			}
		}
		[Obsolete]
		protected void AddDefiledTile() { }
	}
	//temp solution
	public interface IDefiledTile { }
	public interface IRivenTile { }
	public interface IAshenTile { }
	/// <summary>
	/// exists solely to let a class override SetStaticDefaults without replacing base functionality provided by ComplexFrameTile
	/// </summary>
	public abstract class BufferBaseTile : OriginTile {
		public sealed override void SetStaticDefaults() {
			SetDefaults();
		}
		protected virtual void SetDefaults() { }

	}
	public abstract class ComplexFrameTile : BufferBaseTile {
		TileOverlay[] overlays;
		protected sealed override void SetDefaults() {
			overlays = [..GetOverlays()];
			for (int i = 0; i < overlays.Length; i++) {
				overlays[i].SetupTexture();
				overlays[i].SetupOther(this);
			}
			SetStaticDefaults();
		}
		protected virtual IEnumerable<TileOverlay> GetOverlays() {
			yield break;
		}
		public new virtual void SetStaticDefaults() { }
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			Tile tile = Framing.GetTileSafely(i, j);
			if (!TileDrawing.IsVisible(tile)) return;
			for (int k = 0; k < overlays.Length; k++) {
				overlays[k].Draw(i, j, tile, spriteBatch);
			}
		}
		protected abstract class TileOverlay {
			protected abstract string TexturePath { get; }
			protected Asset<Texture2D> Texture { get; private set; }
			protected TreePaintingSettings settings;
			protected CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; private set; }
			public void SetupTexture() {
				Texture = ModContent.Request<Texture2D>(TexturePath);
				GlowPaintKey = CustomTilePaintLoader.CreateKey();
			}
			public virtual void SetupOther(ModTile tile) { }
			public abstract void Draw(int i, int j, Tile tile, SpriteBatch spriteBatch);
		}
		protected class TileMergeOverlay(string texturePath, int tileType, int parentFrameHeight = 270) : TileOverlay {
			protected override string TexturePath => texturePath;
			public override void SetupOther(ModTile tile) {
				Main.tileMerge[tile.Type][tileType] = true;
				Main.tileMerge[tileType][tile.Type] = true;
			}
			public override void Draw(int i, int j, Tile tile, SpriteBatch spriteBatch) {
				void Do(params int[] directions) {
					for (int k = 0; k < directions.Length; k++) {
						int direction = directions[k];
						Tile tile;
						switch (direction) {
							case 0:
							tile = Framing.GetTileSafely(i, j + 1);
							if (tile.TopSlope || tile.IsHalfBlock) return;
							break;
							case 1:
							tile = Framing.GetTileSafely(i, j - 1);
							if (tile.BottomSlope) return;
							break;
							case 2:
							tile = Framing.GetTileSafely(i - 1, j);
							if (tile.LeftSlope || tile.IsHalfBlock) return;
							break;
							case 3:
							tile = Framing.GetTileSafely(i + 1, j);
							if (tile.RightSlope || tile.IsHalfBlock) return;
							break;
							default:
							return;
						}
						if (!tile.TileIsType(tileType)) return;
						Rectangle frame = new(direction * 18, tile.TileFrameNumber * 18, 16, 16);
						Vector2 offset = new(Main.offScreenRange, Main.offScreenRange);
						if (Main.drawToScreen) {
							offset = Vector2.Zero;
						}
						int posYFactor = -2;
						int flatY = 0;
						int kScaleY = 2;
						int flatX = 14;
						int kScaleX = -2;
						Vector2 position = new Vector2(i * 16f, j * 16f) + offset - Main.screenPosition;
						Color color = Lighting.GetColor(i, j);
						Texture2D texture = Texture.Value;
						switch (tile.BlockType) {
							case BlockType.HalfBlock:
							case BlockType.Solid:
							spriteBatch.Draw(texture, position, frame, color, 0f, default, 1f, SpriteEffects.None, 0f);
							break;
							case BlockType.SlopeDownLeft://1
							posYFactor = 0;
							kScaleY = 0;
							flatX = 0;
							kScaleX = 2;
							goto case BlockType.SlopeUpRight;
							case BlockType.SlopeDownRight://2
							posYFactor = 0;
							kScaleY = 0;
							flatX = 14;
							kScaleX = -2;
							goto case BlockType.SlopeUpRight;
							case BlockType.SlopeUpLeft://3
							flatX = 0;
							kScaleX = 2;
							goto case BlockType.SlopeUpRight;

							case BlockType.SlopeUpRight://4
							for (int l = 0; l < 8; l++) {
								Main.spriteBatch.Draw(
									texture,
									position + new Vector2(flatX + kScaleX * k, k * 2 + posYFactor * k),
									new Rectangle(frame.X + flatX + kScaleX * k, frame.Y + flatY + kScaleY * k, 2, frame.Height - 2 * k),
									color,
									0f,
									Vector2.Zero,
									1f,
									0,
									0f
								);
							}
							break;
						}
					}
				}
				switch ((tile.TileFrameX / 18, (tile.TileFrameY % parentFrameHeight) / 18)) {
					case (0, 0) or (0, 1) or (0, 2):
					Do(0, 1, 3);
					break;
					case (1, 0) or (2, 0) or (3, 0):
					Do(0, 2, 3);
					break;
					case (1, 1) or (2, 1) or (3, 1):
					Do(0, 1, 2, 3);
					break;
					case (1, 2) or (2, 2) or (3, 2):
					Do(1, 2, 3);
					break;
					case (4, 0) or (4, 1) or (4, 2):
					Do(0, 1, 2);
					break;

					case (5, 0) or (5, 1) or (5, 2):
					Do(0, 1);
					break;

					case (6, 0) or (7, 0) or (8, 0):
					Do(0);
					break;
					case (6, 1) or (7, 1) or (8, 1):
					Do(0, 1, 2, 3);
					break;
					case (6, 2) or (7, 2) or (8, 2):
					Do(0, 1, 2, 3);
					break;
					case (6, 3) or (7, 3) or (8, 3):
					Do(1);
					break;

					case (9, 0) or (9, 1) or (9, 2):
					Do(3);
					break;
					case (10, 0) or (10, 1) or (10, 2):
					Do(0, 1, 2, 3);
					break;
					case (11, 0) or (11, 1) or (11, 2):
					Do(0, 1, 2, 3);
					break;
					case (12, 0) or (12, 1) or (12, 2):
					Do(2);
					break;


					case (0, 3) or (2, 3) or (4, 3):
					Do(0, 3);
					break;
					case (1, 3) or (3, 3) or (5, 3):
					Do(0, 2);
					break;
					case (0, 4) or (2, 4) or (4, 4):
					Do(1, 3);
					break;
					case (1, 4) or (3, 4) or (5, 4):
					Do(1, 2);
					break;

					case (6, 4) or (7, 4) or (8, 4):
					Do(2, 3);
					break;
				}
			}
		}
	}
}
