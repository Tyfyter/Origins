using Origins.Dev;
using Origins.Tiles.Other;
using System;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Chambersite {
	[AutoloadEquip(EquipType.Body)]
	public class Chambersite_Robe : ModItem, INoSeperateWikiPage {
		private int ManaToRemove = 80;
		public string[] Categories => [
			"ArmorSet",
			"MagicBoostGear"
		];
		public override void SetStaticDefaults() {
			ArmorIDs.Body.Sets.HidesBottomSkin[Item.bodySlot] = true;
		}
		public override void SetDefaults() {
			Item.defense = 9;
			Item.value = Item.sellPrice(gold: 6);
			Item.rare = ItemRarityID.Lime;
		}
		public override void UpdateEquip(Player player) {
			player.statManaMax2 = Math.Max(player.statManaMax2 - ManaToRemove, 20);
			ManaToRemove = 80;
			player.manaCost = 0.2f;
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return head.type == ItemID.MagicHat || head.type == ItemID.WizardHat;
		}
		public override void UpdateArmorSet(Player player) {
			if (player.armor[0].type == ItemID.MagicHat) {
				player.setBonus = Language.GetTextValue("Mods.Origins.SetBonuses.ChambersiteMagicHat");
				ManaToRemove = 20;
			}
			if (player.armor[0].type == ItemID.WizardHat) {
				player.setBonus = Language.GetTextValue("Mods.Origins.SetBonuses.ChambersiteWizardHat");
				player.GetCritChance(DamageClass.Magic) += 20;
			}
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.Robe)
			.AddIngredient<Chambersite_Item>(10)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
}
