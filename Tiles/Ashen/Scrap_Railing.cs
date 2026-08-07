using Origins.Items.Weapons.Ammo;
using System;
using System.Text;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Tiles.Ashen; 
public class Scrap_Railing : Platform_Tile {
	public static int ID { get; private set; }
	public override void OnLoad() {
		Item.OnAddRecipes += (item) => {
			Recipe.Create(item.type, 4)
			.AddIngredient<Scrap>()
			.Register();
			Recipe.Create(ModContent.ItemType<Scrap>())
			.AddIngredient(item.type, 4)
			.DisableDecraft()
			.Register();
		};
		Init();
	}
	(Pattern pattern, (short x, short y) frame)[] Patterns { get; set; }
	void Init() => Patterns = [
		("""
		_____
		_____
		__*__
		_____
		_____
		""", (0, 0)),
		("""
		_____
		_____
		__+__
		__O__
		_____
		""", (4, 3)),
		#region no block connections
		("""
		_____
		__B__
		_R+L_
		__T__
		_____
		""", (0, 1)),
		("""
		_____
		__B__
		_R+__
		__T__
		_____
		""", (1, 1)),
		("""
		_____
		__B__
		__+L_
		__T__
		_____
		""", (2, 1)),
		("""
		_____
		__B__
		_R+L_
		_____
		_____
		""", (3, 1)),
		("""
		_____
		__B__
		__+__
		__T__
		_____
		""", (4, 1)),
		("""
		_____
		__B__
		_R+__
		_____
		_____
		""", (5, 1)),
		("""
		_____
		__B__
		__+L_
		_____
		_____
		""", (6, 1)),
		("""
		_____
		_____
		_R+L_
		__T__
		_____
		""", (0, 2)),
		("""
		_____
		_____
		_R+__
		__T__
		_____
		""", (1, 2)),
		("""
		_____
		_____
		__+L_
		__T__
		_____
		""", (2, 2)),
		("""
		_____
		_____
		_R+L_
		_____
		_____
		""", (7, 2)),
		("""
		_____
		_____
		__+__
		__T__
		_____
		""", (4, 2)),
		("""
		_____
		_____
		_R+__
		_____
		_____
		""", (5, 2)),
		("""
		_____
		_____
		__+L_
		_____
		_____
		""", (6, 2)),
		#endregion
		("""
		_____
		__*__
		__+__
		__O__
		_____
		""", (1, 3)),
		#region slopes
		("""
		_____
		_____
		__/__
		_____
		_____
		""", (9, 0)),
		("""
		___/_
		__/__
		__/__
		_____
		_____
		""", (9, 0)),
		("""
		_____
		_____
		__/l_
		__+__
		_____
		""", (8, 0)),
		("""
		_____
		___/_
		__/__
		__+__
		_____
		""", (8, 1)),
		("""
		_____
		___/_
		_+/__
		__+__
		_____
		""", (8, 2)),
		("""
		_____
		__/__
		__+__
		_____
		_____
		""", (8, 3)),
		("""
		_____
		___/_
		__/__
		__/__
		_____
		""", (9, 1)),
		("""
		_____
		_____
		__/*_
		__*__
		_____
		""", (12, 0)),
		("""
		_____
		_____
		_*\__
		__*__
		_____
		""", (13, 0)),
		("""
		_____
		__*__
		__\*_
		_____
		_____
		""", (12, 1)),
		("""
		_____
		__*__
		_*/__
		_____
		_____
		""", (13, 1))
		#endregion
	];
	public override void SetStaticDefaults() {
		ID = Type;
		base.SetStaticDefaults();
		TileID.Sets.Platforms[Type] = false;
		TileID.Sets.CanPlaceNextToNonSolidTile[Type] = true;
		TileID.Sets.CanBeSloped[Type] = true;
		TileID.Sets.HasSlopeFrames[Type] = true;
		Catwalk.OverrideTileNoAttach[Type] = false;
		Main.tileSolidTop[Type] = false;
		Main.tileSolid[Type] = false;
		DustType = DustID.Lihzahrd;
		RegisterItemDrop(Item.Type);
		HitSound = SoundID.Tink;
	}
	public override bool Slope(int i, int j) {
		Tile tile = Main.tile[i, j];
		switch (tile.Slope) {
			case SlopeType.Solid:
			tile.Slope = SlopeType.SlopeDownLeft;
			break;
			case SlopeType.SlopeDownLeft:
			tile.Slope = SlopeType.SlopeDownRight;
			break;
			default:
			case SlopeType.SlopeDownRight:
			tile.Slope = SlopeType.Solid;
			break;
		}
		if (!WorldGen.gen) {
			WorldGen.KillTile(i, j, fail: true, effectOnly: true);
			SoundEngine.PlaySound(SoundID.Dig, new(i * 16 + 8, j * 16 + 8));
			WorldGen.SquareTileFrame(i, j);
		}
		if (NetmodeActive.MultiplayerClient) NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 7, i, j, 1f);
		return false;
	}
	public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
		offsetY = 0;
		switch (tileFrameX / 18) {
			case 8:
			case 10:
			offsetY = 8;
			break;
			case 9:
			case 11:
			offsetY = 0;
			break;
		}
		if (tileFrameX / 18 <= 4 && tileFrameY / 18 >= 3) height += 2;
	}
#if DEBUG
	public override bool RightClick(int i, int j) {
		Main.NewText(Pattern.GeneratePattern(i, j));
		WorldGen.TileFrame(i, j, true);
		return true;
	}
#endif
	public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
		Init();
		int bestPattern = -1;
		int bestQuality = -1;
		for (int k = 0; k < Patterns.Length; k++) {
			if (Maximize(ref bestQuality, Patterns[k].pattern.MatchQuality(i, j))) bestPattern = k;
		}
		if (bestPattern == -1) return false;
		Tile tile = Main.tile[i, j];
		(short x, short y) = (tile.TileFrameX, tile.TileFrameY);
		(tile.TileFrameX, tile.TileFrameY) = Patterns[bestPattern].frame;
		tile.TileFrameX *= 18;
		tile.TileFrameY *= 18;
		if (x != tile.TileFrameX || y != tile.TileFrameY) FrameSurrounding(i, j);
		return false;
	}
	void FrameSurrounding(int i, int j) {
		for (int x = -2; x <= 2; x++) {
			for (int y = -2; y <= 2; y++) {
				if (x == 0 && y == 0) continue;
				if (Framing.GetTileSafely(x + i, y + j) is not Tile { HasTile: true } otherTile) continue;
				if (otherTile.TileType == Type || Catwalk.Catwalks[otherTile.TileType]) WorldGen.TileFrame(x + i, y + j);
			}
		}
	}
	public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
		if (!fail && !effectOnly) FrameSurrounding(i, j);
	}
	static bool CanRailingAttachTo(int i, int j, Pattern.TileKind kind) {
		if (Main.tile[i, j + 2] is Tile { HasTile: true } catwalk && Catwalk.Catwalks[catwalk.TileType]) return true;
		Tile tile = Main.tile[i, j];
		if (!tile.HasUnactuatedTile) return false;
		if (tile.TileType == Scrap_Railing.ID) return true;
		switch (kind) {
			case Pattern.TileKind.CanConnectTop or Pattern.TileKind.NoConnectTop:
			if (TileID.Sets.Platforms[tile.TileType] || Main.tileSolidTop[tile.TileType]) return true;
			if (tile.TopSlope) return false;
			break;
			case Pattern.TileKind.CanConnectBottom or Pattern.TileKind.NoConnectBottom:
			if (tile.BottomSlope) return false;
			break;
			case Pattern.TileKind.CanConnectLeft or Pattern.TileKind.NoConnectLeft:
			if (TileID.Sets.Platforms[tile.TileType]) return false;
			if (tile.LeftSlope) return false;
			break;
			case Pattern.TileKind.CanConnectRight or Pattern.TileKind.NoConnectRight:
			if (TileID.Sets.Platforms[tile.TileType]) return false;
			if (tile.RightSlope) return false;
			break;
		}
		if (Main.tileSolid[tile.TileType]) return true;
		if (Catwalk.OverrideTileNoAttach[tile.TileType].HasValue) return !Catwalk.OverrideTileNoAttach[tile.TileType].Value;
		return Main.tileSolid[tile.TileType] && !Main.tileNoAttach[tile.TileType];
	}
	/// <summary>
	/// Key:
	/// Railings:<br/>
	/// +: <see cref="SlopeType.Solid"/><br/>
	/// \: <see cref="SlopeType.SlopeDownLeft"/><br/>
	/// /: <see cref="SlopeType.SlopeDownRight"/><para/>
	/// Other:<br/>
	/// _: Ignore<br/>
	/// O: not a railing, top can be connected to<br/>
	/// T: top can be connected to<br/>
	/// B: bottom can be connected to<br/>
	/// L: left side can be connected to<br/>
	/// R: right side can be connected to<br/>
	/// t: top can not be connected to<br/>
	/// b: bottom can not be connected to<br/>
	/// l: left side can not be connected to<br/>
	/// r: right side can not be connected to<br/>
	/// </summary>
	readonly struct Pattern {
		readonly TileKind[] Layout { get; init; }
		readonly static Regex sanityCheck = new("^([_TtBbLlRrO+*/\\\\]{5}\n){4}[_TtBbLlRrO+*/\\\\]{5}$", RegexOptions.Compiled);
		public static string GeneratePattern(int x, int y) {
			StringBuilder pattern = new();
			for (int j = -2; j <= 2; j++) {
				pattern.Append('\n');
				for (int i = -2; i <= 2; i++) {
					Tile tile = Framing.GetTileSafely(x + i, y + j);
					if (tile.HasTile && tile.TileType == ID) {
						switch (tile.Slope) {
							case SlopeType.Solid:
							pattern.Append('+');
							break;
							case SlopeType.SlopeDownLeft:
							pattern.Append('\\');
							break;
							case SlopeType.SlopeDownRight:
							pattern.Append('/');
							break;
						}
					} else {
						pattern.Append('_');
					}
				}
			}
			return pattern.ToString();
		}
		public int MatchQuality(int x, int y) {
			int quality = 0;
			for (int j = -2; j <= 2; j++) {
				for (int i = -2; i <= 2; i++) {
					Tile tile = Framing.GetTileSafely(x + i, y + j);
					TileKind kind = Layout[i + 2 + (j + 2) * 5];
					switch (kind) {
						case TileKind.Ignore:
						continue;

						case TileKind.AnyRailing:
						if (!tile.HasTile || tile.TileType != ID) return -1;
						quality++;
						break;

						case TileKind.SolidRailing:
						if (tile.BlockType != BlockType.Solid) return -1;
						quality++;
						goto case TileKind.AnyRailing;

						case TileKind.LeftSlopeRailing:
						if (tile.BlockType != BlockType.SlopeDownLeft) return -1;
						quality++;
						goto case TileKind.AnyRailing;

						case TileKind.RightSlopeRailing:
						if (tile.BlockType != BlockType.SlopeDownRight) return -1;
						quality++;
						goto case TileKind.AnyRailing;

						case TileKind.CanConnectTop:
						case TileKind.CanConnectBottom:
						case TileKind.CanConnectLeft:
						case TileKind.CanConnectRight:
						if (!CanRailingAttachTo(x + i, y + j, kind)) return -1;
						break;

						case TileKind.NoConnectTop:
						case TileKind.NoConnectBottom:
						case TileKind.NoConnectLeft:
						case TileKind.NoConnectRight:
						if (CanRailingAttachTo(x + i, y + j, kind)) return -1;
						break;

						case TileKind.SolidTileTop:
						if (tile.TileType == ID || !CanRailingAttachTo(x + i, y + j, TileKind.CanConnectTop)) return -1;
						break;
					}
				}
			}
			return quality;
		}
		public static implicit operator Pattern(string value) {
			if (!sanityCheck.IsMatch(value)) throw new ArgumentException("Invalid layout", nameof(value));
			TileKind[] layout = new TileKind[5 * 5];
			int i = 0;
			foreach (char c in value) {
				if (c == '\n') continue;
				layout[i] = c switch {
					'_' => TileKind.Ignore,
					'*' => TileKind.AnyRailing,
					'+' => TileKind.SolidRailing,
					'\\' => TileKind.LeftSlopeRailing,
					'/' => TileKind.RightSlopeRailing,

					'T' => TileKind.CanConnectTop,
					't' => TileKind.NoConnectTop,

					'B' => TileKind.CanConnectBottom,
					'b' => TileKind.NoConnectBottom,

					'L' => TileKind.CanConnectLeft,
					'l' => TileKind.NoConnectLeft,

					'R' => TileKind.CanConnectRight,
					'r' => TileKind.NoConnectRight,
					'O' => TileKind.SolidTileTop,
					_ => throw new ArgumentException("Invalid layout", nameof(value))
				};
				i++;
			}
			layout[4] = TileKind.Ignore;
			return new() { Layout = layout };
		}
		public override string ToString() {
			StringBuilder pattern = new();
			for (int i = 0; i < Layout.Length; i++) {
				if (i > 0 && i % 5 == 0) pattern.Append('\n');
				switch (Layout[i]) {
					case TileKind.Ignore:
					pattern.Append('_');
					break;
					case TileKind.AnyRailing:
					pattern.Append('*');
					break;
					case TileKind.SolidRailing:
					pattern.Append('+');
					break;
					case TileKind.LeftSlopeRailing:
					pattern.Append('\\');
					break;
					case TileKind.RightSlopeRailing:
					pattern.Append('/');
					break;
					case TileKind.CanConnectTop:
					pattern.Append('T');
					break;
					case TileKind.NoConnectTop:
					pattern.Append('t');
					break;
					case TileKind.CanConnectBottom:
					pattern.Append('B');
					break;
					case TileKind.NoConnectBottom:
					pattern.Append('b');
					break;
					case TileKind.CanConnectLeft:
					pattern.Append('L');
					break;
					case TileKind.NoConnectLeft:
					pattern.Append('l');
					break;
					case TileKind.CanConnectRight:
					pattern.Append('R');
					break;
					case TileKind.NoConnectRight:
					pattern.Append('r');
					break;
					case TileKind.SolidTileTop:
					pattern.Append('O');
					break;
				}
			}
			return pattern.ToString();
		}
		public enum TileKind {
			Ignore,
			AnyRailing,
			SolidRailing,
			LeftSlopeRailing,
			RightSlopeRailing,
			CanConnectTop,
			CanConnectBottom,
			CanConnectLeft,
			CanConnectRight,
			NoConnectTop,
			NoConnectBottom,
			NoConnectLeft,
			NoConnectRight,
			SolidTileTop,
		}
	}
}
