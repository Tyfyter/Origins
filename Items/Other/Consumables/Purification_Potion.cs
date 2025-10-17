using Origins.Buffs;
using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class Purification_Potion : ModItem {
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 20;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WrathPotion);
			Item.buffType = Purification_Buff.ID;
			Item.buffTime = 60 * 60 * 8;
			Item.value = Item.sellPrice(silver: 2);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.Ectoplasm)
			.AddIngredient(ItemID.Moonglow)
			.AddIngredient(ModContent.ItemType<Mojo_Flask>())
			.AddTile(TileID.Bottles)
			.Register();
		}
	}
}
