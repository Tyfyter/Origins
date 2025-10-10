using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.World.BiomeData;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Ashen {
	public class Fortified_Steel_Block1 : OriginTile, IAshenTile {
		public virtual Color MapColor => FromHexRGB(0x7a391a);
		public override void SetStaticDefaults() {
			Origins.PotType.Add(Type, ((ushort)TileType<Ashen_Pot>(), 0, 0));
			Origins.PileType.Add(Type, ((ushort)TileType<Ashen_Foliage>(), 0, 6));
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileMergeDirt[Type] = false;
			TileID.Sets.Stone[Type] = false;
			TileID.Sets.Conversion.Stone[Type] = false;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			TileID.Sets.CanBeClearedDuringOreRunner[Type] = true;
			AddMapEntry(MapColor);

			MinPick = 65;
			MineResist = 2;
			HitSound = SoundID.Tink;
			DustType = Ashen_Biome.DefaultTileDust;
		}
		public static void DrawTilePattern(int i, int j, Texture2D patternTexture) {
			Vector2 pos = new Vector2(i * 16, j * 16) - Main.screenPosition;
			if (!Main.drawToScreen) {
				pos.X += Main.offScreenRange;
				pos.Y += Main.offScreenRange;
			}
			Lighting.GetCornerColors(i, j, out VertexColors vertices);
			Vector4 destination = new(pos, 16, 16);
			Rectangle source = new((i * 16) % patternTexture.Width, (j * 16) % patternTexture.Height, 16, 16);
			Tile tile = Main.tile[i, j];
			switch (tile.BlockType) {
				default: {
					int cullLeft = 0;
					int cullRight = 0;
					int cullTop = 0;
					int cullBottom = 0;
					if (tile.IsHalfBlock) SetCull(top: 12);
					switch ((tile.TileFrameX / 18, tile.TileFrameY / 18)) {
						case (0, 3):
						case (0, 4):
						case (2, 3):
						case (2, 4):
						case (4, 3):
						case (4, 4):
						SetCull(left: 4);
						break;
						case (1, 3):
						case (1, 4):
						case (3, 3):
						case (3, 4):
						case (5, 3):
						case (5, 4):
						SetCull(right: 4);
						break;

						case (1, 0):
						case (2, 0):
						case (3, 0):
						SetCull(top: 4);
						break;

						case (6, 0):
						case (7, 0):
						case (8, 0):
						SetCull(top: 4);
						break;
						case (6, 3):
						case (7, 3):
						case (8, 3):
						SetCull(bottom: 4);
						break;

						case (9, 0):
						case (9, 1):
						case (9, 2):
						SetCull(left: 4);
						break;
						case (12, 0):
						case (12, 1):
						case (12, 2):
						SetCull(right: 4);
						break;

						case (9, 3):
						case (10, 3):
						case (11, 3):
						SetCull(left: 4, right: 4);
						break;
					}
					Cull(cullLeft, cullRight, cullTop, cullBottom);
					Main.tileBatch.Draw(
						patternTexture,
						destination,
						source,
						vertices
					);
					void SetCull(int left = 0, int right = 0, int top = 0, int bottom = 0) {
						cullLeft = int.Max(left, cullLeft);
						cullRight = int.Max(right, cullRight);
						cullTop = int.Max(top, cullTop);
						cullBottom = int.Max(bottom, cullBottom);
					}
					break;
				}
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
				source.X += left;
				source.Width -= left;

				destination.Z -= right;
				source.Width -= right;

				destination.Y += top;
				destination.W -= top;
				source.Y += top;
				source.Height -= top;

				destination.W -= bottom;
				source.Height -= bottom;

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
				Rectangle _source = source;
				for (int i = 2; i <= 12; i += 2) {
					vertices = _vertices;
					destination = _destination;
					source = _source;
					int _left = (right ? 16 - i : i) - 2;
					int _right = right ? i - 2 : 16 - i;
					int _top = bottom ? i : 0;
					int _bottom = bottom ? 0 : i;
					if (bottom) _top = int.Max(_top, 4);
					else _bottom = int.Max(_bottom, 4);
					Cull(_left, _right, _top, _bottom);
					Main.tileBatch.Draw(
						patternTexture,
						destination,
						source,
						vertices
					);
				}
			}
		}
	}
	public class Fortified_Steel_Block2 : Fortified_Steel_Block1 {
		AutoLoadingAsset<Texture2D> patternTexture = typeof(Fortified_Steel_Block2).GetDefaultTMLName() + "_BG";
		public override Color MapColor => FromHexRGB(0x281a15);
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			MinPick = 100;
		}
		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
			DrawTilePattern(i, j, patternTexture);
			return base.PreDraw(i, j, spriteBatch);
		}
	}
	public class Fortified_Steel_Block3 : Fortified_Steel_Block1 {
		AutoLoadingAsset<Texture2D> patternTexture = typeof(Fortified_Steel_Block3).GetDefaultTMLName() + "_BG";
		public override Color MapColor => FromHexRGB(0x30131c);
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			MinPick = 210;
		}
		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
			DrawTilePattern(i, j, patternTexture);
			return base.PreDraw(i, j, spriteBatch);
		}
	}
	public class Fortified_Steel_Block1_Item : ModItem, ICustomWikiStat {
		public virtual int MinePower => 65;
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
			ItemTrader.ChlorophyteExtractinator.AddOption_FromAny(ItemID.StoneBlock, Type);
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Fortified_Steel_Block1>());
		}
		public LocalizedText PageTextMain => WikiPageExporter.GetDefaultMainPageText(this)
			.WithFormatArgs(MinePower,
			Language.GetText("Mods.Origins.Generic.Ashen_Factory"),
			"Stone"
		);
	}
	public class Fortified_Steel_Block2_Item : Fortified_Steel_Block1_Item {
		public override string Texture => base.Texture.Replace("2", "1");
		public override int MinePower => 100;
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Fortified_Steel_Block2>());
		}
	}
	public class Fortified_Steel_Block3_Item : Fortified_Steel_Block1_Item {
		public override string Texture => base.Texture.Replace("3", "1");
		public override int MinePower => 210;
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Fortified_Steel_Block3>());
		}
	}
}
