using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables.Food {
    public class Brine_Cheese : ModItem {
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 5;
			ItemID.Sets.FoodParticleColors[Type] = [
				new Color(170, 204, 164)
			];
			ItemID.Sets.IsFood[Type] = true;
			Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3));
		}
		public override void SetDefaults() {
			Item.DefaultToFood(
				24, 24,
				BuffID.WellFed3,
				60 * 60 * 12
			);
			Item.scale = 0.6f;
			Item.value = Item.sellPrice(copper: 1);
			Item.rare = ItemRarityID.Gray;
		}
		public override bool ConsumeItem(Player player) {
			player.AddBuff(BuffID.Rabies, Item.buffTime + 120);
			player.AddBuff(BuffID.Tipsy, Item.buffTime);
			return true;
		}
	}
}
