using Origins.Items.Materials;
using Origins.Tiles.Other;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Omnidirectional_Claymore : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Omni-Directional Claymore");
			Tooltip.SetDefault("Explodes when enemies cross its laser\nCan aim in different directions");
			SacrificeTotal = 5;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.LandMine);
			Item.damage = 165;
			Item.createTile = ModContent.TileType<Omnidirectional_Claymore_Tile>();
			Item.value = Item.sellPrice(gold: 1, silver: 75);
			Item.rare = ItemRarityID.Pink;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 25);
			recipe.AddIngredient(ItemID.ExplosivePowder, 50);
			recipe.AddIngredient(ModContent.ItemType<Busted_Servo>(), 25);
			recipe.AddIngredient(ModContent.ItemType<Power_Core>());
			recipe.AddIngredient(ModContent.ItemType<Rotor>(), 100);
			recipe.AddTile(TileID.MythrilAnvil); //Fabricator
			recipe.Register();
		}
	}
}
