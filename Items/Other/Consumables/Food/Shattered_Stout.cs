using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables.Food {
    public class Shattered_Stout : ModItem {
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 5;
			ItemID.Sets.FoodParticleColors[Type] = [
				new Color(81, 52, 161),
				new Color(22, 18, 33)
			];
			ItemID.Sets.IsFood[Type] = true;
		}
		public override void SetDefaults() {
			Item.DefaultToFood(
				32, 24,
				BuffID.WellFed,
				60 * 60 * 20,
				true
			);
			Item.holdStyle = ItemHoldStyleID.None;
		}
	}
}
