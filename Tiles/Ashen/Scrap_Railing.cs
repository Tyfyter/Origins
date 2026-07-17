using Origins.Items.Weapons.Ammo;
using System;
using System.Text.RegularExpressions;
using Terraria;
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
		__+__
		__O__
		_____
		""", (0, 0)),
		#region no block connections
		("""
		__+__
		_+++_
		__+__
		_____
		""", (0, 1)),
		("""
		__+__
		_++__
		__+__
		_____
		""", (1, 1)),
		("""
		__+__
		__++_
		__+__
		_____
		""", (2, 1)),
		("""
		__+__
		_+++_
		_____
		_____
		""", (3, 1)),
		("""
		__+__
		__+__
		__+__
		_____
		""", (4, 1)),
		("""
		__+__
		_++__
		_____
		_____
		""", (5, 1)),
		("""
		__+__
		__++_
		_____
		_____
		""", (6, 1)),
		("""
		_____
		_+++_
		__+__
		_____
		""", (0, 2)),
		("""
		_____
		_++__
		__+__
		_____
		""", (1, 2)),
		("""
		_____
		__++_
		__+__
		_____
		""", (2, 2)),
		("""
		_____
		_+++_
		_____
		_____
		""", (7, 2)),
		("""
		_____
		__+__
		__+__
		_____
		""", (4, 2)),
		("""
		_____
		_++__
		_____
		_____
		""", (5, 2)),
		("""
		_____
		__++_
		_____
		_____
		""", (6, 2)),
		#endregion
		("""
		__+__
		__+__
		__O__
		_____
		""", (1, 3)),
		#region slopes
		("""
		__+__
		_++__
		__/__
		_____
		""", (8, 3)),
		("""
		_____
		__+__
		_++__
		__/__
		""", (8, 0)),
		("""
		_____
		__++_
		_++__
		__/__
		""", (8, 1)),
		("""
		_____
		_+++_
		_++__
		__/__
		""", (8, 2)),
		("""
		__+__
		_++/_
		_____
		_____
		""", (9, 0)),
		("""
		___+_
		__++/
		_____
		__X__
		""", (9, 1)),
		("""
		__++_
		__++/
		_____
		__X__
		""", (6, 1)),
		#endregion
	];
	public override void SetStaticDefaults() {
		ID = Type;
		base.SetStaticDefaults();
		TileID.Sets.Platforms[Type] = false;
		TileID.Sets.CanPlaceNextToNonSolidTile[Type] = true;
		TileID.Sets.HasSlopeFrames[Type] = true;
		Main.tileSolidTop[Type] = false;
		Main.tileSolid[Type] = false;
		DustType = DustID.Lihzahrd;
		RegisterItemDrop(Item.Type);
		HitSound = SoundID.Tink;
	}
	public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
		offsetY = 2;
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
	}
	public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
		Init();
		int bestPattern = -1;
		int bestQuality = -1;
		for (int k = 0; k < Patterns.Length; k++) {
			if (Maximize(ref bestQuality, Patterns[k].pattern.MatchQuality(i, j))) bestPattern = k;
		}
		if (bestPattern == -1) return false;
		Tile tile = Main.tile[i, j];
		(tile.TileFrameX, tile.TileFrameY) = Patterns[bestPattern].frame;
		tile.TileFrameX *= 18;
		tile.TileFrameY *= 18;
		return false;
	}
	public static bool CanRailingAttachTo(int i, int j) {
		Tile tile = Main.tile[i, j];
		if (tile.TileType == Scrap_Railing.ID) return true;
		if (!tile.HasTile) return false;
		if (tile.HasFullSolidTile()) return true;
		if (TileID.Sets.Platforms[tile.TileType]) return true;
		if (Catwalk.OverrideTileNoAttach[tile.TileType].HasValue) return !Catwalk.OverrideTileNoAttach[tile.TileType].Value;
		return Main.tileSolid[tile.TileType] && !Main.tileNoAttach[tile.TileType];
	}
	readonly struct Pattern {
		readonly TileKind[] Layout { get; init; }
		readonly static Regex sanityCheck = new("^([_OX+\\/]{5}\n){3}[_OX+\\/]{5}$", RegexOptions.Compiled);
		public int MatchQuality(int x, int y) {
			int quality = 0;
			for (int j = -1; j <= 2; j++) {
				for (int i = -2; i <= 2; i++) {
					Tile tile = Framing.GetTileSafely(x + i, y + j);
					switch (Layout[i + 2 + (j + 1) * 5]) {
						case TileKind.Ignore:
						continue;

						case TileKind.Railing:
						if (!tile.HasTile || (tile.TileType != ID)) return -1;
						quality++;
						break;

						case TileKind.LeftSlope:
						if (tile.BlockType != BlockType.SlopeDownLeft) return -1;
						goto case TileKind.CanConnect;

						case TileKind.RightSlope:
						if (tile.BlockType != BlockType.SlopeDownRight) return -1;
						goto case TileKind.CanConnect;

						case TileKind.CanConnect:
						if (!CanRailingAttachTo(x + i, y + j)) return -1;
						break;

						case TileKind.NoConnect:
						if (CanRailingAttachTo(x + i, y + j)) return -1;
						break;
					}
					quality++;
				}
			}
			return quality;
		}
		public static implicit operator Pattern(string value) {
			if (!sanityCheck.IsMatch(value)) throw new ArgumentException("Invalid layout", nameof(value));
			TileKind[] layout = new TileKind[5 * 4];
			int i = 0;
			foreach (char c in value) {
				switch (c) {
					case '_':
					layout[i] = TileKind.Ignore;
					break;
					case '+':
					layout[i] = TileKind.Railing;
					break;
					case 'O':
					layout[i] = TileKind.CanConnect;
					break;
					case 'X':
					layout[i] = TileKind.NoConnect;
					break;
					case '\\':
					layout[i] = TileKind.LeftSlope;
					break;
					case '/':
					layout[i] = TileKind.RightSlope;
					break;

					default:
					continue;
				}
				i++;
			}
			layout[4] = TileKind.Ignore;
			return new() { Layout = layout };
		}
		enum TileKind {
			Ignore,
			Railing,
			CanConnect,
			NoConnect,
			LeftSlope,
			RightSlope,
		}
	}
}
