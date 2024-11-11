using Microsoft.Xna.Framework;
using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Pets {
	public class Rusty_Cross_Necklace : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Pet"
		];
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