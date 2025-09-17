using Origins.Buffs;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class Voidsight_Potion : ModItem {
        public string[] Categories => [
            "Potion"
        ];
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 20;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WrathPotion);
			Item.buffType = Voidsight_Buff.ID;
			Item.buffTime = 60 * 60 * 6;
			Item.value = Item.sellPrice(silver: 2);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.Fireblossom)
			.AddIngredient(ItemID.NightOwlPotion)
			.AddIngredient(ModContent.ItemType<Bleeding_Obsidian_Shard>())
			.AddTile(TileID.Bottles)
			.Register();
		}
	}
}
