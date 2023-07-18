using Microsoft.Xna.Framework;
using Origins.Items.Pets;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Pets {
	public class Rusty_Cross_Necklace : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Rusty Cross Necklace");
			// Tooltip.SetDefault("Summons a Guardian Angel to watch after you\n'Be not afraid'");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Yellow;
		}

		public override void UseStyle(Player player, Rectangle heldItemFrame) {
			if (player.whoAmI == Main.myPlayer && player.itemTime == 0) {
				//player.AddBuff(Item.buffType, 3600);
			}
		}
	}
}