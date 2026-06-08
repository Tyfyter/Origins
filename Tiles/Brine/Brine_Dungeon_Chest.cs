using Origins.Items.Materials;
using Origins.Items.Weapons.Melee;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Tiles.Brine {
	public class Brine_Dungeon_Chest : ModChest, IItemObtainabilityProvider {
		protected override bool CanBeLocked => true;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			AddMapEntry(new Color(15, 86, 88), CreateMapEntryName(), MapChestName);
			AddMapEntry(new Color(15, 86, 88), Language.GetOrRegister(Mod.GetLocalizationKey($"{LocalizationCategory}.{Name}_Locked.MapEntry")), MapChestName);
			AdjTiles = [TileID.Containers];
			keyItem = ModContent.ItemType<Brine_Key>();
			DustType = DustID.GreenMoss;
			RegisterItemDrop(ModContent.ItemType<Brine_Dungeon_Chest_Item>());
			ModLoader.GetMod(nameof(AltLibrary)).Call("adddungeonchest",
				Type,
				ModContent.ItemType<The_Foot>(),
				1
			);
		}
		public override LocalizedText DefaultContainerName(int frameX, int frameY) => CreateMapEntryName();
		public override bool CanUnlockChest(int i, int j) => NPC.downedPlantBoss;
		public override bool LockChest(int i, int j, ref short frameXAdjustment, ref bool manual) => NPC.downedPlantBoss && base.LockChest(i, j, ref frameXAdjustment, ref manual);
		IEnumerable<int> IItemObtainabilityProvider.ProvideItemObtainability() => [ModContent.ItemType<The_Foot>()];
	}
	public class Brine_Dungeon_Chest_Item : ModItem {
		public override void SetStaticDefaults() {
			ModCompatSets.AnyChests[Type] = true;
		}
		public override void SetDefaults() {
			Item.width = 26;
			Item.height = 22;
			Item.maxStack = Item.CommonMaxStack;
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
