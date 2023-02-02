using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class The_Button : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("The Button");
			Tooltip.SetDefault("Summons I.C.A.R.U.S");

			SacrificeTotal = 3;
			//ItemID.Sets.SortingPriorityBossSpawns[Type] = 3;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WormFood);
			Item.rare = CrimsonRarity.ID;
		}
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient<Fibron_Plating>();
			recipe.AddIngredient<Qube>();
			recipe.AddTile(TileID.MythrilAnvil); //Omni-printer
			recipe.Register();
		}
	}
}
