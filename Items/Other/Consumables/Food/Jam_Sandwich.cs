using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables.Food {
	public class Jam_Sandwich : ModItem {
		static short glowmask;
        public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 5;
			ItemID.Sets.FoodParticleColors[Type] = [
				new Color(42, 59, 112),
				new Color(211, 239, 255),
				new Color(88, 129, 255),
				new Color(88, 255, 192)
			];
			ItemID.Sets.IsFood[Type] = true;
			Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3));
		}
		public override void SetDefaults() {
			Item.DefaultToFood(
				32, 24,
				BuffID.WellFed2,
				60 * 60 * 10
			);
			Item.value = Item.sellPrice(silver: 10);
			Item.rare = ItemRarityID.Orange;
			Item.glowMask = glowmask;
		}
	}
}
