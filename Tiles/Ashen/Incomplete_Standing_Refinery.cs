using Humanizer;
using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.Graphics;
using Origins.NPCs.Ashen;
using Origins.World.BiomeData;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Origins.Core.MultiTypeMultiTile;

namespace Origins.Tiles.Ashen {
	public class Incomplete_Standing_Refinery : OriginTile, IMultiTypeMultiTile, Repairboy.IReparableTile, IGlowingModTile {
		readonly Sound ambientSound = EnvironmentSounds.Register<Sound>();
		public static int ID { get; private set; }
		public static AutoLoadingAsset<Texture2D> GlowTexture = typeof(Incomplete_Standing_Refinery).GetDefaultTMLName() + "_Glow";
		public Color GlowColor => Color.White;
		AutoCastingAsset<Texture2D> IGlowingModTile.GlowTexture => GlowTexture;
		public static ShapeMap Shape => field = field || new ShapeMap(
			new() {
				['X'] = (ushort)ModContent.TileType<Incomplete_Standing_Refinery>(),
				['S'] = (ushort)ModContent.TileType<Incomplete_Standing_Refinery>(),
				['+'] = (ushort)ModContent.TileType<Truss_Block>()
			},
			" + XX+XX XXX     |     XXX XX+XX + ",
			" + XX+XXXXXX     |     XXXXXX+XX + ",
			" +  X+XXX+XX     |     XX+XXX+X  + ",
			"+++++++++++++X   |   X+++++++++++++",
			" +   +XXX+XXXXXX | XXXXXX+XXX+   + ",
			" +SXX+XXX+XXXXXXX|XXXXXXX+XXX+XXS+ ",
			" +SXX+XXX+XXX+XXX|XXX+XXX+XXX+XXS+ ",
			" + XX+XXX+XXX+XX | XX+XXX+XXX+XX + ",
			"+++++++++++++++  |  +++++++++++++++",
			" +XXX+XXX+XXXX   |   XXXX+XXX+XXX+ ",
			" +XXX+XXX+XXXX   |   XXXX+XXX+XXX+ ",
			" +XXX+XXX+XXXX   |   XXXX+XXX+XXX+ ",
			" +XXX+XXX+XXXXX  |  XXXXX+XXX+XXX+ ",
			"++++++++++++++++ | ++++++++++++++++",
			" +XXX+XXX+XXXXX  |  XXXXX+XXX+XXX+ ",
			"  XXXXXXXXXXXXX  |  XXXXXXXXXXXXX  ",
			"   XXXXXXXXXXXX  |  XXXXXXXXXXXX   ",
			"   XXXXXXXXXXXX  |  XXXXXXXXXXXX   ",
			"X XXXXXXXXXXXXX  |  XXXXXXXXXXXXX X",
			"XXXXXXXXXXXXXXX  |  XXXXXXXXXXXXXXX",
			"XXXXXXXXXXXXXXX  |  XXXXXXXXXXXXXXX",
			"   XXXXXXXXXXXX  |  XXXXXXXXXXXX   ",
			"   XXXXXXXXXXXX  |  XXXXXXXXXXXX   ",
			"   XXXXXXXXXXXX  |  XXXXXXXXXXXX   ",
			"   XXXXXXXXXXXX  |  XXXXXXXXXXXX   ",
			"XXXXXXXXXXXXXXX  |  XXXXXXXXXXXXXXX",
			"XXXXXXXXXXXXXXX  |  XXXXXXXXXXXXXXX",
			"XXXXXXXXXXXXXXX  |  XXXXXXXXXXXXXXX",
			"XXXXXXXXXXXXXXXXX|XXXXXXXXXXXXXXXXX"
		);
		public static ShapeMap GlowShape => field = field || new ShapeMap(
			new() {
				['1'] = 1
			},
			"         11      |      11         ",
			"                 |                 ",
			"                 |                 ",
			"                 |                 ",
			"                 |                 ",
			"      11   11    |    11   11      ",
			"                 |                 ",
			"                 |                 ",
			"                 |                 ",
			"                 |                 ",
			"                 |                 ",
			"                 |                 ",
			"                 |                 ",
			"                 |                 ",
			"                 |                 ",
			"                 |                 ",
			"      11         |         11      ",
			"           1111  |  1111           ",
			"                 |                 ",
			"                 |                 ",
			"    11           |           11    ",
			"       1         |         1       ",
			"      11         |         11      ",
			"      11         |         11      ",
			"       1         |         1       ",
			"           1111  |  1111           ",
			"                 |                 ",
			"                 |                 ",
			"                 |                 "
		);
		public override void Load() {
			new TileItem(this, true).RegisterItem();
			this.SetupGlowKeys();
		}
		void IGlowingModTile.FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			Point16 pos = new(tile.GetTilePosition());
			int style = 0;
			int style2 = 0;
			TileObjectData.GetTileInfo(tile, ref style, ref style2);
			style = style * TileObjectData.GetTileData(tile).StyleMultiplier + style2;
			pos -= TileObjectData.TopLeft(pos);

			if (!GlowShape[pos.X, pos.Y, style]) return;
			color.DoFancyGlow(GlowColor.ToVector3() * new Vector3(1, 0.32f, 0), tile.TileColor);
		}
		public override void SetStaticDefaults() {
			// Properties
			TileID.Sets.CanBeSloped[Type] = false;
			Main.tileFrameImportant[Type] = true;
			Main.tileLighted[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = false;

			// Names
			AddMapEntry(new Color(154, 56, 11));

			// Placement
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
			TileObjectData.newTile.Width = 17;
			TileObjectData.newTile.SetHeight(29);
			TileObjectData.newTile.SetOriginBottomCenter();
			TileObjectData.newTile.Direction = TileObjectDirection.PlaceRight;
			TileObjectData.newTile.HookPlaceOverride = Shape.Place;
			TileObjectData.newTile.AnchorBottom = new(AnchorType.SolidTile | AnchorType.SolidWithTop, TileObjectData.newTile.Width, 0);
			TileObjectData.newTile.FlattenAnchors = true;
			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
			TileObjectData.addAlternate(1);
			TileObjectData.addTile(Type);
			ID = Type;
			HitSound = SoundID.Tink;
			DustType = Ashen_Biome.DefaultTileDust;
		}
		public bool IsValidTile(Tile tile, int left, int top, int style) {
			if (Shape.Matches(tile, left, top, style)) return true;
			(int x, int y) = tile.GetTilePosition();
			x -= left;
			y -= top;
			return Shape[x, y, style] == '+'
				&& Shape[x + 1, y, style] != 'X'
				&& Shape[x - 1, y, style] != 'X'
				&& Shape[x, y + 1, style] != 'X'
				&& Shape[x, y - 1, style] != 'X';
		}

		public bool ShouldBlockPlacement(Tile tile, int left, int top, int style) {
			Point pos = tile.GetTilePosition();
			if (!Shape[pos.X - left, pos.Y - top, style]) return false;
			return MultiTypeMultiTile.NormallyBlocksPlacement(tile);
		}
		public bool ShouldBreak(int x, int y, int left, int top, int style) => Shape[x - left, y - top, style];
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) => this.DrawTileGlow(i, j, spriteBatch);
		public override void NearbyEffects(int i, int j, bool closer) {
			if (closer) return;
			ambientSound.TrySetNearest(new(i * 16 + 8, j * 16 + 8));
		}
		class Sound : AEnvironmentSound {
			SlotId droning;
			public override void UpdateSound(Vector2 position) {
				droning.PlaySoundIfInactive(Origins.Sounds.StandingRefinery, position, playingSound => {
					if (GetPosition() is not Vector2 pos) return false;
					const float extra_falloff_start = 16 * 20;
					playingSound.Volume = 1f / float.Max(pos.DistanceSQ(Main.Camera.Center) / (extra_falloff_start * extra_falloff_start), 1);
					DrownOut.ApplyNext(playingSound, Utils.Remap(playingSound.Volume, 0, 2, 1, 1f / 6), SoundType.Music);
					return true;
				});
			}
		}
		int Repairboy.IReparableTile.RepairboyLimit => 7;
		bool Repairboy.IReparableTile.NeedsRepair(int i, int j, ref float cost, ref Rectangle hitbox) => true;
		void Repairboy.IReparableTile.Repair(int i, int j) { }
		bool Repairboy.IReparableTile.ShouldAlwaysHaveRepairboys => true;
		public Point MainTileOffset => new(8, 9);
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }

	}
}
