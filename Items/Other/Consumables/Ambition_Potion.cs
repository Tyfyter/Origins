using Origins.Buffs;
using Origins.Items.Other.Fish;
using Origins.Tiles.Ashen;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
    public class Ambition_Potion : ModItem {
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 20;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WrathPotion);
			Item.buffType = Ambition_Buff.ID;
			Item.value = Item.sellPrice(silver: 2);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.BottledWater)
			.AddIngredient(ModContent.ItemType<Polyeel>())
			.AddRecipeGroupWithItem(OriginSystem.DeathweedRecipeGroupID, showItem: ModContent.ItemType<Surveysprout_Item>())
			.AddTile(TileID.Bottles)
			.Register();
		}
	}
}
