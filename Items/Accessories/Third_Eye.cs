using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Face)]
	public class Third_Eye : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Third Eye");
			Tooltip.SetDefault("Your third eye will fire a phantasmal death ray when enemies are nearby\n'I am the cosmos'");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(32, 20);
			Item.value = Item.sellPrice(gold: 6);
			Item.master = true;
			Item.rare = ItemRarityID.Master;
		}
		public override void UpdateEquip(Player player) {
			//player.GetModPlayer<OriginPlayer>().thirdEye = true;
		}
	}
}
