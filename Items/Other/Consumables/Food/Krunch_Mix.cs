using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables.Food {
	public class Krunch_Mix : ModItem {
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 5;
			ItemID.Sets.FoodParticleColors[Type] = [
				new Color(180, 180, 180),
				new Color(129, 129, 129),
				new Color(88, 88, 88)
			];
			ItemID.Sets.IsFood[Type] = true;
		}
		public override void SetDefaults() {
			Item.DefaultToFood(
				32, 24,
				BuffID.WellFed,
				60 * 60 * 6
			);
			Item.holdStyle = ItemHoldStyleID.None;
			Item.scale = 0.4f;
			Item.value = Item.sellPrice(silver: 20);
			Item.rare = ItemRarityID.Orange;
		}
		public override bool ConsumeItem(Player player) {
			player.AddBuff(BuffID.Regeneration, Item.buffTime);
			return true;
		}
	}
}
