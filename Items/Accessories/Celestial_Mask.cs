using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Face)]
	public class Celestial_Mask : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Celestial Stone Mask");
			Tooltip.SetDefault("Increase to damage, melee speed, critical strike chance,\nlife regeneration, defense, mining speed, and minion knockback,\nbut your movement is hindered");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			sbyte slot = Item.faceSlot;
			Item.CloneDefaults(ItemID.Aglet);
			Item.neckSlot = -1;
			Item.faceSlot = slot;
			Item.value = Item.sellPrice(gold: 18);
			Item.rare = ItemRarityID.Yellow;
		}
		public override void UpdateEquip(Player player) {
			player.GetAttackSpeed(DamageClass.Melee) += 0.15f;
			player.GetDamage(DamageClass.Generic) += 0.1f;
			player.GetCritChance(DamageClass.Generic) += 0.04f;
			player.lifeRegen += 4;
			player.statDefense += 8;
			player.pickSpeed += 0.25f;
			player.GetKnockback(DamageClass.Summon) += 1;

			player.moveSpeed *= 0.9f;
			player.jumpSpeedBoost -= 1.8f;
		}
	}
}
