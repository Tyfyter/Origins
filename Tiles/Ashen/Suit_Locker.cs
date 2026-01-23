using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics;
using Origins.Items.Weapons.Ammo;
using Origins.World.BiomeData;
using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Ashen {
	public class Suit_Locker : OriginTile, IGlowingModTile {
		public static int ID { get; private set; }
		TileItem item;
		public override void Load() {
			this.SetupGlowKeys();
			Mod.AddContent(item = new TileItem(this).WithOnAddRecipes(item => {
				Recipe.Create(item.type)
				.AddIngredient(ItemID.CopperBar)
				.AddIngredient(ModContent.ItemType<Ashen_Torch>())
				.AddIngredient(ModContent.ItemType<Scrap>(), 16)
				.AddTile(ModContent.TileType<Metal_Presser>())
				.Register();
				Recipe.Create(item.type)
				.AddIngredient(ItemID.TinBar)
				.AddIngredient(ModContent.ItemType<Ashen_Torch>())
				.AddIngredient(ModContent.ItemType<Scrap>(), 16)
				.AddTile(ModContent.TileType<Metal_Presser>())
				.Register();
			}));
		}
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			if (tile.TileFrameX < 3 * 18 * 2) color.DoFancyGlow(new Vector3(0.912f, 0.579f, 0f) * (tile.TileFrameY < 2 * 18 ? 1 : 0.125f), tile.TileColor);
		}
		public override void SetStaticDefaults() {
			if (!Main.dedServ) GlowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
			// Properties
			TileID.Sets.CanBeSloped[Type] = false;
			Main.tileLighted[Type] = true;
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = false;
			TileID.Sets.HasOutlines[Type] = false;
			TileID.Sets.DisableSmartCursor[Type] = true;

			// Names
			AddMapEntry(new Color(220, 220, 220), CreateMapEntryName());

			// Placement
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
			TileObjectData.newTile.Direction = TileObjectDirection.None;
			TileObjectData.newTile.FlattenAnchors = true;
			TileObjectData.newTile.RandomStyleRange = 3;
			TileObjectData.addTile(Type);
			ID = Type;
			DustType = Ashen_Biome.DefaultTileDust;
			RegisterItemDrop(item.Type);
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			Tile tile = Main.tile[i, j];
			if (tile.TileFrameX < 3 * 18 * 2 && tile.TileFrameY < 2 * 18) {
				r = 0.0912f;
				g = 0.0579f;
				b = 0f;
			}
		}
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			this.DrawTileGlow(i, j, spriteBatch);
		}
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public Color GlowColor => Color.White;
	}
}
