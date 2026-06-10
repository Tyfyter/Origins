using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.World.BiomeData;
using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Origins.Core.MultiTypeMultiTile;

namespace Origins.Tiles.Ashen {
	public class Mother_Unit : OriginTile, IMultiTypeMultiTile {
		public static int ID { get; private set; }
		public static ShapeMap Shape => field = field || new ShapeMap(
			new() {
				['X'] = (ushort)ModContent.TileType<Mother_Unit>()
			}, 
			"XXXX  |  XXXX",
			"XXXXX | XXXXX",
			"XXXXX | XXXXX",
			"XXXXX | XXXXX",
			"XXXXXX|XXXXXX",
			"XXXXXX|XXXXXX",
			"XXXXXX|XXXXXX",
			" XXXXX|XXXXX ",
			" XXXXX|XXXXX ",
			" XXXXX|XXXXX ",
			" XXXXX|XXXXX ",
			"XXXXXX|XXXXXX",
			"XXXXXX|XXXXXX",
			"XXXXXX|XXXXXX",
			"XXXXXX|XXXXXX"
		);
		public override void Load() => new TileItem(this, true).RegisterItem();
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
			TileObjectData.newTile.Width = 6;
			TileObjectData.newTile.SetHeight(15);
			TileObjectData.newTile.SetOriginBottomCenter();
			TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;
			TileObjectData.newTile.HookPlaceOverride = Shape.Place;
			TileObjectData.newTile.AnchorBottom = new(AnchorType.SolidTile | AnchorType.SolidWithTop, TileObjectData.newTile.Width, 0);
			TileObjectData.newTile.FlattenAnchors = true;
			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
			TileObjectData.addAlternate(1);
			TileObjectData.addTile(Type);
			ID = Type;
			HitSound = SoundID.Tink;
			DustType = Ashen_Biome.DefaultTileDust;
		}
		public override bool PreDrawPlacementPreview(int i, int j, SpriteBatch spriteBatch, ref Rectangle frame, ref Vector2 position, ref Color color, bool validPlacement, ref SpriteEffects spriteEffects) {
			position.Y += 2;
			return base.PreDrawPlacementPreview(i, j, spriteBatch, ref frame, ref position, ref color, validPlacement, ref spriteEffects);
		}
		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
			offsetY = 2;
		}
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			if (IsUnanchored(i, j)) {
				WorldGen.KillTile(i, j);
				return false;
			}
			return base.TileFrame(i, j, ref resetFrame, ref noBreak);
		}
		public static bool IsUnanchored(int i, int j) {
			Tile tile = Main.tile[i, j];
			return IsUnanchored(i, j, tile.TileFrameX, tile.TileFrameY, TileObjectData.GetTileStyle(tile));
		}

		public static bool IsUnanchored(int i, int j, int frameX, int frameY, int style) {
			if (frameY >= 5 * 18) return false;
			Tile anchor = default;
			switch (style) {
				case 0:
				if (frameX != 0) return false;
				anchor = Main.tile[i - 1, j];
				if (anchor.RightSlope) return true;
				break;

				case 1:
				if (frameX < (Shape.Width - 1) * 18) return false;
				anchor = Main.tile[i + 1, j];
				if (anchor.LeftSlope) return true;
				break;
			}
			if (!IsValidAnchor(anchor)) return true;
			return false;
			static bool IsValidAnchor(Tile anchor) => anchor.HasTile && (Main.tileSolid[anchor.TileType] && !Main.tileSolidTop[anchor.TileType]) && !Main.tileNoAttach[anchor.TileType];
		}
		public bool IsValidTile(Tile tile, int left, int top, int style) => Shape.Matches(tile, left, top, style);
		public bool ShouldBlockPlacement(Tile tile, int left, int top, int style) {
			Point pos = tile.GetTilePosition();
			if (!Shape[pos.X - left, pos.Y - top, style]) return false;
			if (IsUnanchored(pos.X, pos.Y, (pos.X - left) * 18, (pos.Y - top) * 18, style)) return true;
			return MultiTypeMultiTile.NormallyBlocksPlacement(tile);
		}
		public bool ShouldBreak(int x, int y, int left, int top, int style) => Shape[x - left, y - top, style];
	}
}
