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
		internal static List<Action> LateSetupActions { get; set; } = [];
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
				TileOverlay tileOverlay = overlays[i];
				tileOverlay.SetupTexture();
				LateSetupActions.Add(() => tileOverlay.SetupOther(this));
			}
			SetStaticDefaults();
		}
		protected virtual IEnumerable<TileOverlay> GetOverlays() {
			yield break;
		}
		public new virtual void SetStaticDefaults() { }
		public static void DrawVertexLit(int i, int j) {
			Tile tileCache = Main.tile[i, j];
			ushort typeCache = tileCache.TileType;
			bool IsSameHalfBlock(Tile tile) => tile.HasTile && tile.IsHalfBlock && tile.TileType == typeCache;
			bool IsSameBottom(Tile tile) => tile.HasTile && tile.TileType == typeCache && !tile.BottomSlope;
			short tileFrameX = tileCache.TileFrameX;
			short tileFrameY = tileCache.TileFrameY;
			int tileTop = 0;
			int tileWidth = 16;
			int tileHeight = 16;
			int addFrY = Main.tileFrame[typeCache] * 38;
			int addFrX = 0;
			SpriteEffects tileSpriteEffect = SpriteEffects.None;
			TileLoader.SetSpriteEffects(i, j, typeCache, ref tileSpriteEffect);
			TileLoader.SetDrawPositions(i, j, ref tileWidth, ref tileTop, ref tileHeight, ref tileFrameX, ref tileFrameY);
			TileLoader.SetAnimationFrame(typeCache, i, j, ref addFrX, ref addFrY);
			if (tileCache.BlockType == BlockType.Solid && !TileID.Sets.Platforms[typeCache] && !TileID.Sets.IgnoresNearbyHalfbricksWhenDrawn[typeCache] && Main.tileSolid[typeCache] && !TileID.Sets.NotReallySolid[typeCache]) {
				bool left = IsSameHalfBlock(Main.tile[i - 1, j]);
				bool right = IsSameHalfBlock(Main.tile[i + 1, j]);
				if (left || right) {
					Vector2 pos = new Vector2(i * 16, j * 16) - Main.screenPosition;
					if (!Main.drawToScreen) {
						pos.X += Main.offScreenRange;
						pos.Y += Main.offScreenRange;
					}
					Texture2D texture = Main.instance.TilesRenderer.GetTileDrawTexture(tileCache, i, j);
					Lighting.GetCornerColors(i, j, out VertexColors vertices);
					BlockType blockType = tileCache.BlockType;
					DrawVertexLit(blockType, texture, new(IsSameBottom(Main.tile[i, j - 1]) ? 90 : 126 + addFrX, addFrY, tileWidth, 8), new(pos, tileWidth, 8), vertices);
					pos.Y += 8;
					tileFrameY += 8;
					DrawVertexLit(blockType, texture, new(tileFrameX + addFrX, tileFrameY + addFrY, tileWidth, 8), new(pos, tileWidth, 8), vertices);
					return;
				}
			}
			DrawVertexLit(i, j, Main.instance.TilesRenderer.GetTileDrawTexture(tileCache, i, j), new(tileFrameX + addFrX, tileFrameY + addFrY, tileWidth, tileHeight), tileWidth, tileHeight);
		}
		public static void DrawVertexLit(int i, int j, Texture2D texture, Rectangle frame, float width = 16, float height = 16) {
			Vector2 pos = new Vector2(i * 16, j * 16) - Main.screenPosition;
			if (!Main.drawToScreen) {
				pos.X += Main.offScreenRange;
				pos.Y += Main.offScreenRange;
			}
			Lighting.GetCornerColors(i, j, out VertexColors vertices);
			DrawVertexLit(Main.tile[i, j].BlockType, texture, frame, new(pos, width, height), vertices);
		}
		public static void DrawVertexLit(BlockType blockType, Texture2D texture, Rectangle frame, Vector4 destination, VertexColors vertices) {
			switch (blockType) {
				case BlockType.Solid:
				Main.tileBatch.Draw(
					texture,
					destination,
					frame,
					vertices
				);
				break;
				case BlockType.HalfBlock:
				frame.Height -= 12;
				destination.Y += 8;
				destination.W -= 12;
				Main.tileBatch.Draw(
					texture,
					destination,
					frame,
					vertices
				);
				frame.Y += 8;
				destination.Y += 4;
				Main.tileBatch.Draw(
					texture,
					destination,
					frame,
					vertices
				);
				break;
				case BlockType.SlopeDownLeft:
				DrawSlope(false, true);
				break;
				case BlockType.SlopeDownRight:
				DrawSlope(true, true);
				break;
				case BlockType.SlopeUpLeft:
				DrawSlope(false, false);
				break;
				case BlockType.SlopeUpRight:
				DrawSlope(true, false);
				break;
			}
			void Cull(int left = 0, int right = 0, int top = 0, int bottom = 0) {
				destination.X += left;
				destination.Z -= left;
				frame.X += left;
				frame.Width -= left;

				destination.Z -= right;
				frame.Width -= right;

				destination.Y += top;
				destination.W -= top;
				frame.Y += top;
				frame.Height -= top;

				destination.W -= bottom;
				frame.Height -= bottom;

				(vertices.TopLeftColor, vertices.TopRightColor, vertices.BottomLeftColor, vertices.BottomRightColor) = (
					Color.Lerp(vertices.TopLeftColor, vertices.TopRightColor, left / 16f),
					Color.Lerp(vertices.TopRightColor, vertices.TopLeftColor, right / 16f),
					Color.Lerp(vertices.BottomLeftColor, vertices.BottomRightColor, left / 16f),
					Color.Lerp(vertices.BottomRightColor, vertices.BottomLeftColor, right / 16f)
				);

				(vertices.TopLeftColor, vertices.BottomLeftColor, vertices.TopRightColor, vertices.BottomRightColor) = (
					Color.Lerp(vertices.TopLeftColor, vertices.BottomLeftColor, top / 16f),
					Color.Lerp(vertices.BottomLeftColor, vertices.TopLeftColor, bottom / 16f),
					Color.Lerp(vertices.TopRightColor, vertices.BottomRightColor, top / 16f),
					Color.Lerp(vertices.BottomRightColor, vertices.TopRightColor, bottom / 16f)
				);
			}
			void DrawSlope(bool right, bool bottom) {
				VertexColors _vertices = vertices;
				Vector4 _destination = destination;
				Rectangle _source = frame;
				for (int i = 2; i <= 12; i += 2) {
					vertices = _vertices;
					destination = _destination;
					frame = _source;
					int _left = (right ? 16 - i : i) - 2;
					int _right = right ? i - 2 : 16 - i;
					int _top = bottom ? i : 0;
					int _bottom = bottom ? 0 : i;
					if (bottom) _top = int.Max(_top, 4);
					else _bottom = int.Max(_bottom, 4);
					Cull(_left, _right, _top, _bottom);
					Main.tileBatch.Draw(
						texture,
						destination,
						frame,
						vertices
					);
				}
			}
		}
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			Tile tile = Framing.GetTileSafely(i, j);
			if (!TileDrawing.IsVisible(tile)) return;
			_ = this;
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
		protected class TileMergeOverlay(string texturePath, bool[] tileTypes, int parentFrameHeight = 270) : TileOverlay {
			public TileMergeOverlay(string texturePath, int tileType, int parentFrameHeight = 270) : this(texturePath, [tileType], parentFrameHeight) { }
			public TileMergeOverlay(string texturePath, int[] tileTypes, int parentFrameHeight = 270) : this(texturePath, TileID.Sets.Factory.CreateBoolSet(tileTypes), parentFrameHeight) { }
			protected override string TexturePath => texturePath;
			//down:╷
			//up:╵
			//left:╴
			//right:╶
			// ┼ │ ─ ┌┐ └┘ ├ ┤ ┬ ┴
			static readonly Dictionary<char, Direction[]> directionsByDirection = new() {
				['╷'] = [Direction.Down],
				['╵'] = [Direction.Up],
				['╴'] = [Direction.Left],
				['╶'] = [Direction.Right],
				['┼'] = [Direction.Down, Direction.Up, Direction.Left, Direction.Right],
				['│'] = [Direction.Down, Direction.Up],
				['─'] = [Direction.Left, Direction.Right],
				['┌'] = [Direction.Down, Direction.Right],
				['┐'] = [Direction.Down, Direction.Left],
				['└'] = [Direction.Up, Direction.Right],
				['┘'] = [Direction.Up, Direction.Left],
				['├'] = [Direction.Down, Direction.Up, Direction.Right],
				['┤'] = [Direction.Down, Direction.Up, Direction.Left],
				['┬'] = [Direction.Down, Direction.Left, Direction.Right],
				['┴'] = [Direction.Up, Direction.Left, Direction.Right]
			};
			static readonly string[] directionsByFrame = [
				"├┬┬┬┤│╷╷╷╶┼┼╴───",
				"├┼┼┼┤│┼┼┼╶┼┼╴───",
				"├┴┴┴┤│┼┼┼╶┼┼╴│││",
				"┌┐┌┐┌┐╵╵╵    │││",
				"└┘└┘└┘───       ",
				"┼┼┌┐└┘ ╵┴┴┴╷╶   ",
				"┼┼└┘└┘ ╵┬┬┬╷╶   ",
				"┼┼┌┐└┘ ╵┤├│╷╶   ",
				"┼┼└┘┌┐ ╷┤├│╵╴   ",
				"┼┼┌┐┌┐ ╷┤├│╵╴   ",
				"┼┼└┘┌┐ ╷───╵╴   ",
				"┌┌┌┐┐┐          ",
				"└└└┘┘┘          ",
				"                ",
				"                "
			];
			public override void SetupOther(ModTile tile) {
				for (int i = 0; i < tileTypes.Length; i++) {
					if (!tileTypes[i]) continue;
					Main.tileMerge[tile.Type][i] = true;
					Main.tileMerge[i][tile.Type] = true;
				}
			}
			public override void Draw(int i, int j, Tile tile, SpriteBatch spriteBatch) {
				void Do(params Direction[] directions) {
					for (int k = 0; k < directions.Length; k++) {
						Direction direction = directions[k];
						Tile blendTile;
						switch (direction) {
							case Direction.Down:
							blendTile = Framing.GetTileSafely(i, j + 1);
							if (blendTile.TopSlope || blendTile.IsHalfBlock) continue;
							break;
							case Direction.Up:
							blendTile = Framing.GetTileSafely(i, j - 1);
							if (blendTile.BottomSlope) continue;
							break;
							case Direction.Left:
							blendTile = Framing.GetTileSafely(i - 1, j);
							if (blendTile.LeftSlope || blendTile.IsHalfBlock) continue;
							break;
							case Direction.Right:
							blendTile = Framing.GetTileSafely(i + 1, j);
							if (blendTile.RightSlope || blendTile.IsHalfBlock) continue;
							break;
							default:
							continue;
						}
						if (!blendTile.HasTile || !tileTypes[blendTile.TileType]) continue;
						Rectangle frame = new((int)(direction) * 18, blendTile.TileFrameNumber * 18, 16, 16);
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
						Color color = GetColor(Lighting.GetColor(i, j));
						Texture2D texture = CustomTilePaintLoader.TryGetTileAndRequestIfNotReady(GlowPaintKey, blendTile.TileColor, Texture);
						switch (blendTile.BlockType) {
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
				if (directionsByDirection.TryGetValue(directionsByFrame[(tile.TileFrameY % parentFrameHeight) / 18][tile.TileFrameX / 18], out Direction[] directions)) {
					Do(directions);
				}
			}
			public virtual Color GetColor(Color color) => color;
			enum Direction {
				Down,
				Up,
				Left,
				Right
			}
		}
	}
}
