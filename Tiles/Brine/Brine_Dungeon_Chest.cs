using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Localization;
using Origins.Items.Materials;
using Origins.Items.Weapons.Melee;

namespace Origins.Tiles.Brine {
	public class Brine_Dungeon_Chest : ModChest {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			AddMapEntry(new Color(15, 86, 88), CreateMapEntryName(), MapChestName);
			AddMapEntry(new Color(15, 86, 88), Language.GetOrRegister(Mod.GetLocalizationKey($"{LocalizationCategory}.{Name}_Locked.MapEntry")), MapChestName);
			//disableSmartCursor = true;
			AdjTiles = [TileID.Containers];
			keyItem = ModContent.ItemType<Brine_Key>();
			DustType = DustID.GreenMoss;
			ModLoader.GetMod(nameof(AltLibrary)).Call("adddungeonchest",
				Type,
				ModContent.ItemType<The_Foot>(),
				1
			);
		}
		public override LocalizedText DefaultContainerName(int frameX, int frameY) => CreateMapEntryName();
		public override bool CanUnlockChest(int i, int j) => NPC.downedPlantBoss;
	}
	public class Brine_Dungeon_Chest_Item : ModItem {
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
			Item.createTile = ModContent.TileType<Brine_Dungeon_Chest>();
		}
	}
	public class Locked_Brine_Dungeon_Chest_Item : Brine_Dungeon_Chest_Item {
		public override string Texture => "Origins/Tiles/Brine/Brine_Dungeon_Chest_Item";
		public override void SetDefaults() {
			base.SetDefaults();
			Item.placeStyle = 1;
		}
	}
}
