using AltLibrary.Common.AltBiomes;
using Origins.Items.Materials;
using Origins.Items.Weapons.Melee;
using Origins.Tiles.Brine;
using Origins.World.BiomeData;
using System;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.CrossMod.WaasephisFishingPlus.Items.Consumables {
	public class Ashen_Lock_Box : WaasephisFishingLockBox<Ashen_Alt_Biome> { }
	public class Brine_Lock_Box : WaasephisFishingLockBox<The_Foot, Brine_Dungeon_Chest, Brine_Key> { }
	public class Defiled_Lock_Box : WaasephisFishingLockBox<Defiled_Wastelands_Alt_Biome> { }
	public class Riven_Lock_Box : WaasephisFishingLockBox<Riven_Hive_Alt_Biome> { }
	#region base classes
	public abstract class WaasephisFishingLockBox : ModItem {
		public abstract int LootItem { get; }
		public abstract int ChestItem { get; }
		public abstract int KeyItem { get; }
		public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("WaasephisFishingPlus");
		public override void SetDefaults() {
			Item.consumable = true;
			Item.width = 32;
			Item.height = 22;
			Item.maxStack = Item.CommonMaxStack;
			Item.rare = ItemRarityID.Yellow;
			Item.value = Item.sellPrice(0, 10);
		}
		public override bool CanRightClick() => true;
		public override void ModifyItemLoot(ItemLoot itemLoot) {
			itemLoot.Add(ItemDropRule.Common(LootItem));
			itemLoot.Add(ItemDropRule.Common(ChestItem));
		}
		public override void AddRecipes() {
			try {
				CreateRecipe()
				.AddIngredient(ModLoader.GetMod("WaasephisFishingPlus").Find<ModItem>("LockBoxMold"))
				.AddIngredient(KeyItem)
				.AddTile(TileID.MythrilAnvil)
				.Register();
			} catch (Exception e) {
				if (Origins.LogLoadingError("MissingCrossModItem", nameof(WaasephisFishingLockBox), e)) throw;
			}
		}
	}
	public abstract class WaasephisFishingLockBox<TLoot, TChest, TKey> : WaasephisFishingLockBox where TLoot : ModItem where TChest : ModTile where TKey : ModItem {
		public override int LootItem => ModContent.ItemType<TLoot>();
		public override int ChestItem => TileLoader.GetItemDropFromTypeAndStyle(ModContent.TileType<TChest>(), 0);
		public override int KeyItem => ModContent.ItemType<TKey>();
	}
	public abstract class WaasephisFishingLockBox<TBiome> : WaasephisFishingLockBox where TBiome : AltBiome {
		static AltBiome Biome => ModContent.GetInstance<TBiome>();
		public override int LootItem => Biome.BiomeChestItem.Value;
		public override int ChestItem => TileLoader.GetItemDropFromTypeAndStyle(Biome.BiomeChestTile.Value, 0);
		public override int KeyItem => Biome.BiomeKeyItem.Value;
	}
	#endregion
}
