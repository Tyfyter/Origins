using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.Tiles.ComplexFrameTile;
using static Origins.Tiles.ComplexFrameTile.MultiTileMergeOverlay.TextureData;
using CustomTileVariationKey = Origins.Graphics.CustomTilePaintLoader.CustomTileVariationKey;

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
		public static int TileItem<TTile>() where TTile : ModTile, ITileWithItem => ModContent.GetInstance<TTile>().Item.Type;
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
		public const string merge = "Origins/Tiles/Overlays/Merge/";
		TileOverlay[] overlays;
		protected sealed override void SetDefaults() {
			overlays = [.. GetOverlays()];
			for (int i = 0; i < overlays.Length; i++) {
				TileOverlay tileOverlay = overlays[i];
				tileOverlay.SetupTexture();
				LateSetupActions.Add(() => tileOverlay.SetupOther(Type));
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
			TileMergeOverlay.ResetSkipMask();
			for (int k = 0; k < overlays.Length; k++) {
				overlays[k].Draw(i, j, tile, spriteBatch);
			}
		}
		//down:╷
		//up:╵
		//left:╴
		//right:╶
		// ┼ │ ─ ┌┐ └┘ ├ ┤ ┬ ┴
		static readonly Dictionary<char, Direction> directionsByDirection = new() {
			['╷'] = Direction.Down,
			['╵'] = Direction.Up,
			['╴'] = Direction.Left,
			['╶'] = Direction.Right,
			['┼'] = Direction.Down | Direction.Up | Direction.Left | Direction.Right,
			['│'] = Direction.Down | Direction.Up,
			['─'] = Direction.Left | Direction.Right,
			['┌'] = Direction.Down | Direction.Right,
			['┐'] = Direction.Down | Direction.Left,
			['└'] = Direction.Up | Direction.Right,
			['┘'] = Direction.Up | Direction.Left,
			['├'] = Direction.Down | Direction.Up | Direction.Right,
			['┤'] = Direction.Down | Direction.Up | Direction.Left,
			['┬'] = Direction.Down | Direction.Left | Direction.Right,
			['┴'] = Direction.Up | Direction.Left | Direction.Right
		};
		static readonly string[] directionSymbolsByFrame = [
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
		static Direction[,] directionsByFrame;
		public static Direction GetDirectionsByFrame(int frameX, int frameY) {
			if (directionsByFrame is null) {
				directionsByFrame = new Direction[directionSymbolsByFrame[0].Length, directionSymbolsByFrame.Length];
				for (int j = 0; j < directionSymbolsByFrame.Length; j++) {
					for (int i = 0; i < directionSymbolsByFrame[j].Length; i++) {
						directionsByDirection.TryGetValue(directionSymbolsByFrame[j][i], out directionsByFrame[i, j]);
					}
				}
			}
			return directionsByFrame[frameX, frameY];
		}
		[Flags]
		public enum Direction : byte {
			Down = 1 << 0,
			Up = 1 << 1,
			Left = 1 << 2,
			Right = 1 << 3
		}
		public abstract class TileOverlay {
			protected abstract string TexturePath { get; }
			protected Asset<Texture2D> Texture { get; private set; }
			protected TreePaintingSettings settings;
			protected CustomTilePaintLoader.CustomTileVariationKey PaintKey { get; private set; }
			public virtual void SetupTexture() {
				Texture = ModContent.Request<Texture2D>(TexturePath);
				PaintKey = CustomTilePaintLoader.CreateKey();
			}
			public Texture2D GetPaintedTexture(int paintColor) => CustomTilePaintLoader.TryGetTileAndRequestIfNotReady(PaintKey, paintColor, Texture);
			public virtual void SetupOther(int type) { }
			public abstract void Draw(int i, int j, Tile tile, SpriteBatch spriteBatch);
		}
		public class TileMergeOverlay(string texturePath, bool[] tileTypes, int parentFrameHeight = 270) : TileOverlay {
			public TileMergeOverlay(string texturePath, int tileType, int parentFrameHeight = 270) : this(texturePath, [tileType], parentFrameHeight) { }
			public TileMergeOverlay(string texturePath, int[] tileTypes, int parentFrameHeight = 270) : this(texturePath, TileID.Sets.Factory.CreateBoolSet(tileTypes), parentFrameHeight) { }
			protected override string TexturePath => texturePath;
			public override void SetupOther(int type) {
				for (int i = 0; i < tileTypes.Length; i++) {
					if (!tileTypes[i]) continue;
					Main.tileMerge[type][i] = true;
					Main.tileMerge[i][type] = true;
				}
			}
			public override void Draw(int i, int j, Tile tile, SpriteBatch spriteBatch) {
				Vector2 offset = new(Main.offScreenRange, Main.offScreenRange);
				if (Main.drawToScreen) {
					offset = Vector2.Zero;
				}
				Vector2 position = new Vector2(i * 16f, j * 16f) + offset - Main.screenPosition;
				Color color = GetColor(Lighting.GetColor(i, j));
				Rectangle frame = new(0, 0, 16, 16);
				void Do(IEnumerable<Direction> directions) {
					foreach (Direction direction in directions) {
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
							if (blendTile.RightSlope || blendTile.IsHalfBlock) continue;
							break;
							case Direction.Right:
							blendTile = Framing.GetTileSafely(i + 1, j);
							if (blendTile.LeftSlope || blendTile.IsHalfBlock) continue;
							break;
							default:
							continue;
						}
						if (!blendTile.HasTile || !tileTypes[blendTile.TileType]) continue;
						int dirIndex = 0;
						int dir = (int)direction;
						while (dir > 1) {
							dirIndex++;
							dir >>= 1;
						}
						frame.X = dirIndex * 18;
						frame.Y = blendTile.TileFrameNumber * 18;
						Texture2D texture = GetPaintedTexture(blendTile.TileColor);
						spriteBatch.Draw(texture, position, frame, color, 0f, default, 1f, SpriteEffects.None, 0f);
						skipMask &= ~direction;
					}
				}
				Direction directions = GetDirectionsByFrame(tile.TileFrameX / 18, (tile.TileFrameY % parentFrameHeight) / 18) & skipMask;
				Do(directions.GetFlags());
				if (Texture.Width() > 72) {
					bool down = directions.HasFlag(Direction.Down);
					bool up = directions.HasFlag(Direction.Up);
					bool left = directions.HasFlag(Direction.Left);
					bool right = directions.HasFlag(Direction.Right);
					if (down && right) {
						Tile blendTile = Framing.GetTileSafely(i + 1, j + 1);
						if (blendTile.HasTile && tileTypes[blendTile.TileType] && blendTile.BlockType is BlockType.Solid or BlockType.SlopeUpLeft) {
							frame.X = 4 * 18;
							frame.Y = blendTile.TileFrameNumber * 18;
							Texture2D texture = GetPaintedTexture(blendTile.TileColor);
							spriteBatch.Draw(texture, position, frame, color, 0f, default, 1f, SpriteEffects.None, 0f);
						}
					}
					if (down && left) {
						Tile blendTile = Framing.GetTileSafely(i - 1, j + 1);
						if (blendTile.HasTile && tileTypes[blendTile.TileType] && blendTile.BlockType is BlockType.Solid or BlockType.SlopeUpLeft) {
							frame.X = 5 * 18;
							frame.Y = blendTile.TileFrameNumber * 18;
							Texture2D texture = GetPaintedTexture(blendTile.TileColor);
							spriteBatch.Draw(texture, position, frame, color, 0f, default, 1f, SpriteEffects.None, 0f);
						}
					}
					if (up && right) {
						Tile blendTile = Framing.GetTileSafely(i + 1, j - 1);
						if (blendTile.HasTile && tileTypes[blendTile.TileType] && blendTile.Slope is SlopeType.Solid or SlopeType.SlopeDownLeft) {
							frame.X = 6 * 18;
							frame.Y = blendTile.TileFrameNumber * 18;
							Texture2D texture = GetPaintedTexture(blendTile.TileColor);
							spriteBatch.Draw(texture, position, frame, color, 0f, default, 1f, SpriteEffects.None, 0f);
						}
					}
					if (up && left) {
						Tile blendTile = Framing.GetTileSafely(i - 1, j - 1);
						if (blendTile.HasTile && tileTypes[blendTile.TileType] && blendTile.Slope is SlopeType.Solid or SlopeType.SlopeDownRight) {
							frame.X = 7 * 18;
							frame.Y = blendTile.TileFrameNumber * 18;
							Texture2D texture = GetPaintedTexture(blendTile.TileColor);
							spriteBatch.Draw(texture, position, frame, color, 0f, default, 1f, SpriteEffects.None, 0f);
						}
					}
				}
			}
			public virtual Color GetColor(Color color) => color;
			public static void ResetSkipMask() {
				skipMask = ~(Direction)0;
			}
			static Direction skipMask;
		}
		public class MultiTileMergeOverlay : TileOverlay {
			protected override string TexturePath { get; }
			readonly TextureData[] textures;
			readonly int parentFrameHeight;
			public MultiTileMergeOverlay(params (int tileType, string texture)[] overlays) : this(270, overlays) { }
			public MultiTileMergeOverlay(int parentFrameHeight, params (int tileType, string texture)[] overlays) {
				this.parentFrameHeight = parentFrameHeight;
				textures = TileID.Sets.Factory.CreateCustomSet<TextureData>(default);
				for (int i = 0; i < overlays.Length; i++) textures[overlays[i].tileType] = new(overlays[i].texture);
			}
			public override void SetupTexture() {
				for (int i = 0; i < textures.Length; i++) {
					textures[i].SetupPaintKey();
				}
			}
			public override void SetupOther(int type) {
				for (int i = 0; i < textures.Length; i++) {
					if (!textures[i].Exists) continue;
					Main.tileMerge[type][i] = true;
					Main.tileMerge[i][type] = true;
				}
			}
			public override void Draw(int i, int j, Tile tile, SpriteBatch spriteBatch) {
				Vector2 offset = new(Main.offScreenRange, Main.offScreenRange);
				if (Main.drawToScreen) offset = Vector2.Zero;
				Vector2 position = new Vector2(i * 16f, j * 16f) + offset - Main.screenPosition;
				Color color = Lighting.GetColor(i, j);
				const int threshold = 15;
				bool wasTooDark = color.R + color.G + color.B < threshold;
				color = GetColor(color);
				if (wasTooDark && color.R + color.G + color.B < threshold) return;
				Rectangle frame = new(0, 0, 16, 16);
				Direction directions = GetDirectionsByFrame(tile.TileFrameX / 18, (tile.TileFrameY % parentFrameHeight) / 18);
				Asset<Texture2D>[] cornerMergeTypes = new Asset<Texture2D>[4];
				for (int dirIndex = 0; dirIndex < 4; dirIndex++) {
					Tile blendTile;
					switch ((Direction)(1 << dirIndex) & directions) {
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
						if (blendTile.RightSlope || blendTile.IsHalfBlock) continue;
						break;
						case Direction.Right:
						blendTile = Framing.GetTileSafely(i + 1, j);
						if (blendTile.LeftSlope || blendTile.IsHalfBlock) continue;
						break;
						default:
						continue;
					}
					if (!blendTile.HasTile) continue;
					TextureData texture = textures[blendTile.TileType];
					if (!texture.Exists) continue;
					if (texture.HasCorners) cornerMergeTypes[dirIndex] = texture.Texture;
					frame.X = dirIndex * 18;
					frame.Y = blendTile.TileFrameNumber * 18;
					spriteBatch.Draw(texture.GetTexture(blendTile.TileColor), position, frame, color, 0f, default, 1f, SpriteEffects.None, 0f);
				}
				if (cornerMergeTypes[0] is not null) {
					if (cornerMergeTypes[0] == cornerMergeTypes[3]) {
						Tile blendTile = Framing.GetTileSafely(i + 1, j + 1);
						if (blendTile.HasTile && blendTile.BlockType is BlockType.Solid or BlockType.SlopeUpLeft) {
							TextureData texture = textures[blendTile.TileType];
							if (cornerMergeTypes[0] == texture.Texture) {
								frame.X = 4 * 18;
								frame.Y = blendTile.TileFrameNumber * 18;
								spriteBatch.Draw(texture.GetTexture(blendTile.TileColor), position, frame, color, 0f, default, 1f, SpriteEffects.None, 0f);
							}
						}
					}
					if (cornerMergeTypes[0] == cornerMergeTypes[2]) {
						Tile blendTile = Framing.GetTileSafely(i - 1, j + 1);
						if (blendTile.HasTile && blendTile.BlockType is BlockType.Solid or BlockType.SlopeUpLeft) {
							TextureData texture = textures[blendTile.TileType];
							if (cornerMergeTypes[0] == texture.Texture) {
								frame.X = 5 * 18;
								frame.Y = blendTile.TileFrameNumber * 18;
								spriteBatch.Draw(texture.GetTexture(blendTile.TileColor), position, frame, color, 0f, default, 1f, SpriteEffects.None, 0f);
							}
						}
					}
				}
				if (cornerMergeTypes[1] is not null) {
					if (cornerMergeTypes[1] == cornerMergeTypes[3]) {
						Tile blendTile = Framing.GetTileSafely(i + 1, j - 1);
						if (blendTile.HasTile && blendTile.Slope is SlopeType.Solid or SlopeType.SlopeDownLeft) {
							TextureData texture = textures[blendTile.TileType];
							if (cornerMergeTypes[1] == texture.Texture) {
								frame.X = 6 * 18;
								frame.Y = blendTile.TileFrameNumber * 18;
								spriteBatch.Draw(texture.GetTexture(blendTile.TileColor), position, frame, color, 0f, default, 1f, SpriteEffects.None, 0f);
							}
						}
					}
					if (cornerMergeTypes[1] == cornerMergeTypes[2]) {
						Tile blendTile = Framing.GetTileSafely(i - 1, j - 1);
						if (blendTile.HasTile && blendTile.Slope is SlopeType.Solid or SlopeType.SlopeDownRight) {
							TextureData texture = textures[blendTile.TileType];
							if (cornerMergeTypes[1] == texture.Texture) {
								frame.X = 7 * 18;
								frame.Y = blendTile.TileFrameNumber * 18;
								spriteBatch.Draw(texture.GetTexture(blendTile.TileColor), position, frame, color, 0f, default, 1f, SpriteEffects.None, 0f);
							}
						}
					}
				}
			}
			public virtual Color GetColor(Color color) => color;
			public record struct TextureData(Asset<Texture2D> Texture, PaintHolder PaintKey) {
				static readonly Dictionary<Asset<Texture2D>, PaintHolder> paintKeys = [];
				public TextureData(string texturePath) : this(ModContent.Request<Texture2D>(texturePath), default) { }
				public readonly bool Exists => Texture is not null;
				public readonly bool HasCorners => Texture.Width() > 72;
				public void SetupPaintKey() {
					if (!Exists) return;
					if (!paintKeys.TryGetValue(Texture, out PaintHolder paintKey)) paintKeys[Texture] = paintKey = new();
					PaintKey = paintKey;
				}
				public readonly Texture2D GetTexture(int paintColor) => PaintKey.GetTexture(Texture, paintColor);
				public class PaintHolder {
					CustomTileVariationKey key = CustomTilePaintLoader.CreateKey();
					readonly Texture2D[] textures = new Texture2D[PaintID.NegativePaint];
					public Texture2D GetTexture(Asset<Texture2D> Texture, int paintColor) {
						if (paintColor == PaintID.None) return Texture.Value;
						paintColor--;
						if (textures[paintColor] is null || textures[paintColor] == Asset<Texture2D>.DefaultValue) {
							textures[paintColor] = CustomTilePaintLoader.TryGetTileAndRequestIfNotReady(key, paintColor + 1, Texture);
						}
						return textures[paintColor];
					}
				}
			}
		}
		public class CornerMergeOverlay(ModTile tile, string textureOverride = null, int parentFrameHeight = 270) : TileOverlay {
			protected override string TexturePath => textureOverride ?? (tile.Texture + "_Inverse_Edges");
			public override void Draw(int i, int j, Tile tile, SpriteBatch spriteBatch) {
				Direction directions = GetDirectionsByFrame(tile.TileFrameX / 18, (tile.TileFrameY % parentFrameHeight) / 18);
				bool ShouldMerge(int i, int j) {
					Tile corner = Main.tile[i, j];
					if (!corner.HasTile) return false;
					return (tile.TileType == corner.TileType) || Main.tileMerge[tile.TileType][corner.TileType];
				}
				bool upLeft = directions.HasFlag(Direction.Up) && directions.HasFlag(Direction.Left) && !ShouldMerge(i - 1, j - 1);
				bool upRight = directions.HasFlag(Direction.Up) && directions.HasFlag(Direction.Right) && !ShouldMerge(i + 1, j - 1);
				bool downLeft = directions.HasFlag(Direction.Down) && directions.HasFlag(Direction.Left) && !ShouldMerge(i - 1, j + 1);
				bool downRight = directions.HasFlag(Direction.Down) && directions.HasFlag(Direction.Right) && !ShouldMerge(i + 1, j + 1);
				switch ((upLeft, upRight, downRight, downLeft)) {
					case (true, true, false, false):
					upLeft = upRight = false;
					break;
					case (false, true, true, false):
					upRight = downRight = false;
					break;
					case (false, false, true, true):
					downRight = downLeft = false;
					break;
					case (true, false, false, true):
					downLeft = upLeft = false;
					break;
				}
				if (!(upLeft || upRight || downRight || downLeft)) return;
				Vector2 pos = new Vector2(i * 16, j * 16) - Main.screenPosition;
				if (!Main.drawToScreen) {
					pos.X += Main.offScreenRange;
					pos.Y += Main.offScreenRange;
				}
				Lighting.GetCornerColors(i, j, out VertexColors vertices);
				Vector4 destination = new(pos, 16, 16);
				Texture2D texture = GetPaintedTexture(tile.TileColor);
				if (upLeft) Main.tileBatch.Draw(
					texture,
					destination,
					new Rectangle(0, 0, 16, 16),
					vertices
				);
				if (upRight) Main.tileBatch.Draw(
					texture,
					destination,
					new Rectangle(18, 0, 16, 16),
					vertices
				);
				if (downLeft) Main.tileBatch.Draw(
					texture,
					destination,
					new Rectangle(0, 18, 16, 16),
					vertices
				);
				if (downRight) Main.tileBatch.Draw(
					texture,
					destination,
					new Rectangle(18, 18, 16, 16),
					vertices
				);
			}
		}
	}
	[ReinitializeDuringResizeArrays]
	internal class VanillaTileOverlays : GlobalTile {
		static ComplexFrameTile.TileOverlay[][] forcedTileOverlays = TileID.Sets.Factory.CreateCustomSet<ComplexFrameTile.TileOverlay[]>(null);
		internal static void AddOverlay(int type, ComplexFrameTile.TileOverlay overlay) {
			ComplexFrameTile.TileOverlay[] overlays = forcedTileOverlays[type];
			overlays ??= [];
			Array.Resize(ref overlays, overlays.Length + 1);
			overlays[^1] = overlay;
			overlay.SetupTexture();
			OriginTile.LateSetupActions.Add(() => overlay.SetupOther(type));
			forcedTileOverlays[type] = overlays;
		}
		public override void PostDraw(int i, int j, int type, SpriteBatch spriteBatch) {
			if (forcedTileOverlays[type] is not ComplexFrameTile.TileOverlay[] overlays) return;
			Tile tile = Framing.GetTileSafely(i, j);
			if (!TileDrawing.IsVisible(tile)) return;
			TileMergeOverlay.ResetSkipMask();
			for (int k = 0; k < overlays.Length; k++) {
				overlays[k].Draw(i, j, tile, spriteBatch);
			}
		}
	}
	public interface ITileWithItem {
		public ModItem Item { get; }
	}
}
