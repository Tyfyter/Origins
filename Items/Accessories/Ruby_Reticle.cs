using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Ruby_Reticle : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Ruby Reticle");
			Tooltip.SetDefault("Critical strike chance is increased by 15% of weapon damage");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(28, 30);
			Item.rare = ItemRarityID.Green;
			Item.value = Item.sellPrice(gold: 2);
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().rubyReticle = true;
		}
	}
}
