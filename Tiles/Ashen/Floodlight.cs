using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.Graphics;
using Origins.Items.Tools.Wiring;
using Origins.Items.Weapons.Ammo;
using Origins.World.BiomeData;
using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ObjectData;

namespace Origins.Tiles.Ashen {
	public class Floodlight : OriginTile, IComplexMineDamageTile, IGlowingModTile, IAshenWireTile, IMultiTypeMultiTile {
		protected AutoLoadingAsset<Texture2D> glowTexture;
		public static int ID { get; private set; }
		protected int width, height;
		public override void Load() {
			new TileItem(this)
			.WithExtraStaticDefaults(this.DropTileItem)
			.WithOnAddRecipes(item => {
				Recipe.Create(item.type)
				.AddRecipeGroup(RecipeGroupID.IronBar, 4)
				.AddIngredient<Ashen_Torch>(8)
				.AddIngredient<Scrap>(15)
				.AddTile<Metal_Presser>()
				.Register();
			}).RegisterItem();
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
			TileObjectData.newTile.SetHeight(11, 18);
			TileObjectData.newTile.SetOriginBottomCenter();
			TileObjectData.newTile.Direction = TileObjectDirection.None;
			TileObjectData.newTile.HookPlaceOverride = MultiTypeMultiTile.PlaceWhereTrue(TileObjectData.newTile, IsPart);
			TileObjectData.newTile.AnchorBottom = new(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
			TileObjectData.newTile.FlattenAnchors = true;
			width = TileObjectData.newTile.Width;
			height = TileObjectData.newTile.Height;
			TileObjectData.addTile(Type);
			ID = Type;
			DustType = Ashen_Biome.DefaultTileDust;
			glowTexture = Texture + "_Glow";
		}
		public override void HitWire(int i, int j) {
			UpdatePowerState(i, j, IsPowered(i, j));
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
			if (ShouldGlow(tile)) color.DoFancyGlow(new Vector3(0.5f, 0.31f, 0f) * 3, tile.TileColor);
		}
		public bool IsPowered(int i, int j) {
			TileObjectData data = TileObjectData.GetTileData(Main.tile[i, j]);
			TileUtils.GetMultiTileTopLeft(i, j, data, out int left, out int top);
			for (int x = 0; x < data.Width; x++) {
				for (int y = 0; y < data.Height; y++) {
					if (!IsPart(x, y)) continue;
					if (Main.tile[left + x, top + y].Get<Ashen_Wire_Data>().AnyPower) return true;
				}
			}
			return false;
		}
		public void UpdatePowerState(int i, int j, bool powered) {
			int frameSize = width * 18;
			bool wasPowered = Main.tile[i, j].TileFrameX >= frameSize;
			powered ^= true;
			if (powered == wasPowered) return;
			TileObjectData data = TileObjectData.GetTileData(Main.tile[i, j]);
			TileUtils.GetMultiTileTopLeft(i, j, data, out int left, out int top);
			for (int x = 0; x < data.Width; x++) {
				for (int y = 0; y < data.Height; y++) {
					if (!IsPart(x, y)) continue;
					Tile tile = Main.tile[left + x, top + y];
					tile.TileFrameX = (short)(tile.TileFrameX % frameSize + (powered ? frameSize : 0));
				}
			}
			if (!NetmodeActive.SinglePlayer) NetMessage.SendTileSquare(-1, left, top, data.Width, data.Height);
		}
		public static bool IsPart(int i, int j) {
			switch (i) {
				case 2:
				case 3:
				return true;
			}
			switch (j) {
				case 0:
				case 5:
				case 6:
				case 7:
				case 8:
				case 9:
				return false;
			}
			return true;
		}

		public bool IsValidTile(Tile tile, int left, int top) {
			(int i, int j) = tile.GetTilePosition();
			i -= left;
			j -= top;
			if (IsPart(i, j)) {
				if (tile.TileType != Type) return false;
				Tile topLeft = Main.tile[left, top];
				return tile.TileFrameX == topLeft.TileFrameX + i * 18 && tile.TileFrameY == topLeft.TileFrameY + j * 18;
			}
			return true;
		}
		public bool ShouldBreak(int x, int y, int left, int top) => IsPart(x - left, y - top);
	}
}
