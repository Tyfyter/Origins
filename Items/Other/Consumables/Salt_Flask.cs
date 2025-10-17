using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class Salt_Flask : ModItem, ITornSource {
		public static float TornSeverity => 0.2f;
		float ITornSource.Severity => TornSeverity;
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 20;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.FlaskofIchor);
			Item.buffType = Weapon_Imbue_Salt.ID;
			Item.buffTime = 60 * 60 * 20;
			Item.value = Item.sellPrice(silver: 5);
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.BottledWater)
			.AddIngredient(ModContent.ItemType<Alkahest>(), 2)
			.AddTile(TileID.ImbuingStation)
			.Register();
		}
	}
}
