using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables.Food {
	public class Spearmint_Tea : ModItem {
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 5;
			ItemID.Sets.DrinkParticleColors[Type] = [
				FromHexRGB(0xFFBD5B),
				FromHexRGB(0xE68900),
				FromHexRGB(0xC94F0B)
			];
			ItemID.Sets.IsFood[Type] = true;
			Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3));
		}
		public override void SetDefaults() {
			Item.DefaultToFood(
				32, 24,
				BuffID.WellFed2,
				60 * 60 * 6,
				true
			);
			Item.value = Item.sellPrice(silver: 20);
			Item.rare = ItemRarityID.Orange;
		}
		public override bool ConsumeItem(Player player) {
			player.AddBuff(BuffID.Calm, Item.buffTime);
			return true;
		}
	}
}
