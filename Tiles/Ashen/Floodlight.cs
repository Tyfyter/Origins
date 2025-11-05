using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics;
using Origins.World.BiomeData;
using PegasusLib;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Ashen {
	public class Floodlight : OriginTile, IComplexMineDamageTile, IGlowingModTile {
		protected AutoLoadingAsset<Texture2D> glowTexture;
		public static int ID { get; private set; }
		TileItem Item;
		protected int width, height;
		public override void Load() {
			Mod.AddContent(Item = new(this));
			this.SetupGlowKeys();
		}
		public override void SetStaticDefaults() {
			// Properties
			TileID.Sets.CanBeSloped[Type] = false;
			Main.tileLighted[Type] = true;
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = false;
			TileID.Sets.HasOutlines[Type] = false;
			TileID.Sets.DisableSmartCursor[Type] = true;

			// Names
			AddMapEntry(new Color(255, 90, 30), CreateMapEntryName());

			// Placement
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
			TileObjectData.newTile.Width = 6;
			TileObjectData.newTile.Height = 11;
			TileObjectData.newTile.CoordinateHeights = Enumerable.Repeat(16, TileObjectData.newTile.Height - 1).Concat([18]).ToArray();
			TileObjectData.newTile.Origin = new Point16(TileObjectData.newTile.Width / 2, TileObjectData.newTile.Height - 1);
			TileObjectData.newTile.Direction = TileObjectDirection.None;
			TileObjectData.newTile.FlattenAnchors = true;
			width = TileObjectData.newTile.Width;
			height = TileObjectData.newTile.Height;
			TileObjectData.addTile(Type);
			ID = Type;
			DustType = Ashen_Biome.DefaultTileDust;
			glowTexture = Texture + "_Glow";
			RegisterItemDrop(Item.Type);
		}
		public override void HitWire(int i, int j) {
			Tile tile = Main.tile[i, j];
			int leftX = i - ((tile.TileFrameX / 18) % width);
			int topY = j - ((tile.TileFrameY / 18) % height);
			int offset = IsOn(tile) ? 18 : -18;
			short frameAdjustment = (short)(offset * width);

			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					Main.tile[leftX + x, topY + y].TileFrameX += frameAdjustment;
					Wiring.SkipWire(leftX + x, topY + y);
				}
			}

			// Avoid trying to send packets in singleplayer.
			if (Main.netMode != NetmodeID.SinglePlayer) {
				NetMessage.SendTileSquare(-1, leftX, topY, width, height, TileChangeType.None);
			}
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			if (ShouldGlow(Main.tile[i, j])) {
				r = 5f;
				g = 3.1f;
				b = 0.5f;
			}
		}
		public bool IsOn(Tile tile) => tile.TileFrameX < width * 18;
		public bool ShouldGlow(Tile tile) {
			if (!IsOn(tile)) return false;
			int frameY = tile.TileFrameY / 18;
			return frameY >= 1 && frameY <= 4;
		}
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			this.DrawTileGlow(i, j, spriteBatch);
		}
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public AutoCastingAsset<Texture2D> GlowTexture => glowTexture;
		public Color GlowColor => Color.White;
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			if (ShouldGlow(tile)) color = Vector3.Max(color, new Vector3(0.5f, 0.31f, 0f) * 3);
		}
	}
}
