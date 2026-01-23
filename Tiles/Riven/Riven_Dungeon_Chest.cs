using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using Origins.World.BiomeData;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Tiles.Riven {
	public class Riven_Dungeon_Chest : ModChest, IGlowingModTile {
		public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public Color GlowColor => Color.White;
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			color.DoFancyGlow(new Vector3(0f, 0.25f, 0.5f), tile.TileColor);
		}
		public override void SetStaticDefaults() {
			if (!Main.dedServ) {
				GlowTexture = Mod.Assets.Request<Texture2D>("Tiles/Riven/Riven_Dungeon_Chest_Glow");
			}
			base.SetStaticDefaults();
			Main.tileLighted[Type] = true;
			LocalizedText name = CreateMapEntryName();
			// name.SetDefault("Riven Chest");
			AddMapEntry(new Color(200, 200, 200), name, MapChestName);
			name = Language.GetOrRegister(Mod.GetLocalizationKey($"{LocalizationCategory}.{Name}_Locked.MapEntry"));
			// name.SetDefault("Locked Riven Chest");
			AddMapEntry(new Color(140, 140, 140), name, MapChestName);
			//disableSmartCursor = true;
			AdjTiles = [TileID.Containers];
			keyItem = ModContent.ItemType<Riven_Key>();
			DustType = Riven_Hive.DefaultTileDust;
		}
		public override LocalizedText DefaultContainerName(int frameX, int frameY) => CreateMapEntryName();
		public override bool CanUnlockChest(int i, int j) => NPC.downedPlantBoss;
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			this.DrawChestGlow(i, j, spriteBatch);
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			r = 0f;
			g = 0.05f;
			b = 0.1f;
		}
		public override void Load() => this.SetupGlowKeys();
		public Graphics.CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
	}
	public class Riven_Dungeon_Chest_Item : ModItem {
		static short glowmask;
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			ModCompatSets.AnyChests[Type] = true;
		}
		public override void SetDefaults() {
			Item.width = 32;
			Item.height = 32;
			Item.maxStack = 9999;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.consumable = true;
			Item.value = 500;
			Item.createTile = ModContent.TileType<Riven_Dungeon_Chest>();
			Item.glowMask = glowmask;
		}
	}
	public class Locked_Riven_Dungeon_Chest_Item : Riven_Dungeon_Chest_Item {
		public override string Texture => "Origins/Tiles/Riven/Riven_Dungeon_Chest_Item";
		public override void SetDefaults() {
			base.SetDefaults();
			Item.placeStyle = 1;
		}
	}
}
