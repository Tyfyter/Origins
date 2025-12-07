using System;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Mana_Drive : ModItem {
		static short glowmask;
        public string[] Categories => [
            "Vitality"
        ];
        public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(30, 28);
			Item.value = Item.sellPrice(gold: 10);
			Item.rare = ButterscotchRarity.ID;
			Item.glowMask = glowmask;
		}
		public override void UpdateEquip(Player player) {
			float factor = Math.Min(player.velocity.Length() / 12f, 1);
			player.manaRegenDelayBonus += 5f * factor;
			player.manaRegenBonus += (int)(125 * factor);
		}
	}
}
