using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    [AutoloadEquip(EquipType.Back)]
	public class Superjump_Cape : ModItem {
		public static sbyte BackSlot { get; private set; }
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Super-Jump Cape");
			Tooltip.SetDefault("Allows for a super high jump whilst negating all fall damage");
			BackSlot = Item.backSlot;
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 34);
			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.sellPrice(gold: 1);
		}
		public override void UpdateEquip(Player player) {
			player.jumpSpeedBoost += 12;
			player.noFallDmg = true;
		}
	}
}
