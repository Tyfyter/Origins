using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Back)]
	public class Danger_Barrel : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Danger Barrel");
			// Tooltip.SetDefault("All explosives inflict 'On Fire!' and have a reduced fuse time");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(28, 34);
			Item.value = Item.sellPrice(gold: 2, silver: 20);
			Item.rare = ItemRarityID.Orange;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().dangerBarrel = true;
			player.GetModPlayer<OriginPlayer>().explosiveFuseTime *= 0.666f;
		}
	}
}