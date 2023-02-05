using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Sonic_Radar : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Sonic Radar");
			Tooltip.SetDefault("Allows the detection of traps, enemies, and valuables\nEmits a small aura of light");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(30, 26);
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Pink;
		}
		public override void UpdateEquip(Player player) {
			//player.GetModPlayer<OriginPlayer>().sonicRadar = true;
			player.dangerSense = true;
		}
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.MetalDetector);
			recipe.AddIngredient(ItemID.Radar);
			recipe.AddIngredient(ItemID.SpelunkerPotion, 5);
			recipe.AddIngredient(ItemID.TrapsightPotion, 5);
			recipe.AddTile(TileID.MythrilAnvil); //Fabricator
			recipe.Register();
		}
	}
}
