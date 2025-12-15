using Origins.Items.Materials;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables.Food {
	public class Crockin_Sprocks : ModItem {
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 5;
			ItemID.Sets.FoodParticleColors[Type] = [
				FromHexRGB(0xD3A189),
				FromHexRGB(0x9A380B),
				FromHexRGB(0x753F1A)
			];
			ItemID.Sets.IsFood[Type] = true;
			Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3));
		}
		public override void SetDefaults() {
			Item.DefaultToFood(
				32, 24,
				BuffID.WellFed2,
				60 * 60 * 5
			);
			Item.value = Item.sellPrice(silver: 20);
			Item.rare = ItemRarityID.Orange;
		}
		public override bool ConsumeItem(Player player) {
			player.AddBuff(BuffID.Regeneration, Item.buffTime);
			return true;
		}
		public override void AddRecipes() => CreateRecipe()
			.AddIngredient(ModContent.ItemType<Biocomponent10>(), 10)
			.AddTile(TileID.CookingPots)
			.Register();
	}
}
