using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables.Food {
	public class Bread : ModItem {
        public override void SetStaticDefaults() {
            ItemID.Sets.ShimmerTransformToItem[ItemID.ArtisanLoaf] = ModContent.ItemType<Bread>();
            ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Bread>()] = ItemID.ArtisanLoaf;
            Item.ResearchUnlockCount = 5;
			ItemID.Sets.FoodParticleColors[Type] = [
				new Color(186, 179, 106),
				new Color(175, 157, 75),
				new Color(136, 116, 62)
			];
			ItemID.Sets.IsFood[Type] = true;
		}
		public override void SetDefaults() {
			Item.DefaultToFood(
				20, 18,
				BuffID.WellFed,
				60 * 60 * 8
			);
			Item.value = Item.sellPrice(silver: 1);
			Item.rare = ItemRarityID.White;
			Item.holdStyle = ItemHoldStyleID.None;
		}
	}
}
