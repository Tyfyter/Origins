using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.Graphics.Effects;
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
		public const string merge = "Origins/Tiles/MergerOverlays/";
		TileOverlay[] overlays;
		protected sealed override void SetDefaults() {
			overlays = [..GetOverlays()];
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
			for (int k = 0; k < overlays.Length; k++) {
				overlays[k].Draw(i, j, tile, spriteBatch);
			}
		}
		public abstract class TileOverlay {
			protected abstract string TexturePath { get; }
			protected Asset<Texture2D> Texture { get; private set; }
			protected TreePaintingSettings settings;
			protected CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; private set; }
			public void SetupTexture() {
				Texture = ModContent.Request<Texture2D>(TexturePath);
				GlowPaintKey = CustomTilePaintLoader.CreateKey();
			}
			public virtual void SetupOther(int type) { }
			public abstract void Draw(int i, int j, Tile tile, SpriteBatch spriteBatch);
		}
		public class TileMergeOverlay(string texturePath, bool[] tileTypes, int parentFrameHeight = 270) : TileOverlay {
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
			public override void SetupOther(int type) {
				for (int i = 0; i < tileTypes.Length; i++) {
					if (!tileTypes[i]) continue;
					Main.tileMerge[type][i] = true;
					Main.tileMerge[i][type] = true;
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
						Vector2 position = new Vector2(i * 16f, j * 16f) + offset - Main.screenPosition;
						Color color = GetColor(Lighting.GetColor(i, j));
						Texture2D texture = CustomTilePaintLoader.TryGetTileAndRequestIfNotReady(GlowPaintKey, blendTile.TileColor, Texture);
						spriteBatch.Draw(texture, position, frame, color, 0f, default, 1f, SpriteEffects.None, 0f);
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
			for (int k = 0; k < overlays.Length; k++) {
				overlays[k].Draw(i, j, tile, spriteBatch);
			}
		}
	}
}
