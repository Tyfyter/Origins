using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Fiberglass_Dagger : ModItem {
		
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 20);
			Item.rare = ItemRarityID.Expert;
			Item.value = Item.sellPrice(gold: 2);
		}
		public override void UpdateEquip(Player player) {
			player.statDefense -= 4;
			player.GetDamage(DamageClass.Default).Flat += 8;
			player.GetDamage(DamageClass.Generic).Flat += 8;
			//player.GetModPlayer<OriginPlayer>().fiberglassDagger = true;
		}
	}
}
