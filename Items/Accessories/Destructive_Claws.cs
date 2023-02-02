using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.HandsOn)]
	public class Destructive_Claws : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Destructive Claws");
			Tooltip.SetDefault("30% increased explosive throwing velocity\nIncreases attack speed of thrown explosives\nEnables autouse for all explosive weapons");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaultsKeepSlots(ItemID.YoYoGlove);
			Item.value = Item.sellPrice(gold: 3);
			Item.rare = ItemRarityID.Orange;
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			player.GetAttackSpeed(DamageClasses.Explosive) += 0.1f;
			originPlayer.destructiveClaws = true;
			originPlayer.explosiveThrowSpeed += 0.3f;
		}
	}
}
