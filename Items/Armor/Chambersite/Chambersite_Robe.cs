using Origins.Tiles.Other;
using System;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Chambersite {
	[AutoloadEquip(EquipType.Body)]
	public class Chambersite_Robe : ModItem {
		int legsID;
		public override void Load() {
			legsID = EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Legs}", EquipType.Legs, name: $"{Name}_{EquipType.Legs}");
		}
		public override void SetDefaults() {
			Item.defense = 9;
			Item.value = Item.sellPrice(gold: 6);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void UpdateEquip(Player player) {
			player.statManaMax2 = Math.Max(player.statManaMax2 - (player.armor[0].type == ItemID.MagicHat ? 20 : 80), 20);
			player.manaCost *= 0.2f;
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return head.type == ItemID.MagicHat || head.type == ItemID.WizardHat;
		}
		public override void UpdateArmorSet(Player player) {
			if (player.armor[0].type == ItemID.MagicHat) {
				player.setBonus = Language.GetTextValue("Mods.Origins.SetBonuses.ChambersiteMagicHat");
			}
			if (player.armor[0].type == ItemID.WizardHat) {
				player.setBonus = Language.GetTextValue("Mods.Origins.SetBonuses.ChambersiteWizardHat");
				player.GetCritChance(DamageClass.Magic) += 20;
			}
		}
		public override void SetMatch(bool male, ref int equipSlot, ref bool robes) {
			robes = true;
			equipSlot = legsID;
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
