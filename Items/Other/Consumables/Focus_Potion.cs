using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Other.Fish;
using Origins.Tiles.Brine;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class Focus_Potion : ModItem {
		public const float bonus_multiplicative = 0.15f;
		public const float bonus_additive = 5f;
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 20;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WrathPotion);
			Item.buffType = Focus_Buff.ID;
			Item.value = Item.sellPrice(silver: 2);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.BottledWater)
			.AddIngredient(ItemID.FallenStar)
			.AddIngredient(ModContent.ItemType<Brineglow_Item>())
			.AddIngredient(ModContent.ItemType<Toadfish>())
			.AddTile(TileID.Bottles)
			.Register();
		}
		public static int GetManaCost(Item item) => (int)(15 * (item.useAnimation / 60f)) + 1;
	}
}
