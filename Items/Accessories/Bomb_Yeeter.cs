using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.HandsOn)]
	public class Bomb_Yeeter : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Bomb Handling Device");
			Tooltip.SetDefault("25% increased explosive throwing velocity\nAlso commonly referred to as the 'Bomb Yeeter'");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(38, 20);
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Green;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().bombHandlingDevice = true;
			player.GetModPlayer<OriginPlayer>().explosiveThrowSpeed += 0.25f;
		}
	}
}
