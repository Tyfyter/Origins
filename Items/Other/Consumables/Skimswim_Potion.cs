using Origins.Buffs;
using Origins.Items.Other.Fish;
using Origins.Tiles.Brine;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class Skimswim_Potion : ModItem {
        public string[] Categories => [
            "Potion"
        ];
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 20;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WrathPotion);
			Item.buffType = Skimswim_Buff.ID;
			Item.buffTime = 60 * 60 * 6;
			Item.value = Item.sellPrice(silver: 2);
		}
		public override void AddRecipes() {
			/*Recipe.Create(Type)
			.AddIngredient(ItemID.BottledWater)
			.AddIngredient<Brineglow_Item>()
			.AddIngredient<Toadfish>()
			.AddTile(TileID.Bottles)
			.Register();*/
		}
	}
}
