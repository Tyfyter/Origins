using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using Origins.Tiles.Defiled;
using Origins.World.BiomeData;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Tiles.Ashen {
	public class Ashen_Dungeon_Chest : ModChest, IGlowingModTile {
		public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public Color GlowColor => Color.White;
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			color = Vector3.Max(color, new Vector3(0.394f));
		}
		public override void SetStaticDefaults() {
			if (!Main.dedServ) GlowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
			base.SetStaticDefaults();
			Main.tileLighted[Type] = true;
			LocalizedText name = CreateMapEntryName();
			// name.SetDefault("{$Defiled} Chest");
			AddMapEntry(new Color(176, 63, 10), name, MapChestName);
			name = Language.GetOrRegister(Mod.GetLocalizationKey($"{LocalizationCategory}.{Name}_Locked.MapEntry"));
			// name.SetDefault("Locked {$Defiled} Chest");
			AddMapEntry(new Color(176, 63, 10), name, MapChestName);
			//disableSmartCursor = true;
			AdjTiles = [TileID.Containers];
			keyItem = ModContent.ItemType<Ashen_Key>();
			DustType = Ashen_Biome.DefaultTileDust;
		}
		public override LocalizedText DefaultContainerName(int frameX, int frameY) => CreateMapEntryName();
		public override bool CanUnlockChest(int i, int j) => NPC.downedPlantBoss;
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			r = g = b = 0.01f;
		}
		public override void Load() => this.SetupGlowKeys();
		public Graphics.CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
	}
	public class Ashen_Dungeon_Chest_Item : ModItem {
		public override string Texture => typeof(Defiled_Dungeon_Chest_Item).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			ModCompatSets.AnyChests[Type] = true;
		}
		public override void SetDefaults() {
			Item.width = 26;
			Item.height = 22;
			Item.maxStack = 9999;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.consumable = true;
			Item.value = 500;
			Item.createTile = ModContent.TileType<Ashen_Dungeon_Chest>();
		}
	}
	public class Locked_Ashen_Dungeon_Chest_Item : Ashen_Dungeon_Chest_Item {
		public override string Texture => "Origins/Tiles/Ashen/Ashen_Dungeon_Chest_Item";
		public override void SetDefaults() {
			base.SetDefaults();
			Item.placeStyle = 1;
		}
	}
}
