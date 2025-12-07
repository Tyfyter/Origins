using Origins.Buffs;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class Bile_Flask : ModItem {
        public string[] Categories => [
            "Potion"
        ];
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 20;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.FlaskofCursedFlames);
			Item.buffType = Weapon_Imbue_Bile.ID;
			Item.buffTime = 60 * 60 * 20;
			Item.value = Item.sellPrice(silver: 5);
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.BottledWater)
			.AddIngredient(ModContent.ItemType<Black_Bile>(), 2)
			.AddTile(TileID.ImbuingStation)
			.Register();
		}
	}
}
