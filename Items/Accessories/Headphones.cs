using Origins.Items.Materials;
using Origins.Tiles.Other;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Face)]
	public class Headphones : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Headphones");
			Tooltip.SetDefault("Tagging an enemy increases the chance of a critical strike by 25% on next hit\nApplies to teammates\n'Callouts guys!'");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(32, 20);
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Pink;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().summonTagForceCrit = true;
		}
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<Busted_Servo>(), 8);
			recipe.AddIngredient(ModContent.ItemType<Fiberglass_Item>(), 6);
			recipe.AddIngredient(ModContent.ItemType<Power_Core>());
			recipe.AddIngredient(ModContent.ItemType<Rubber>(), 5);
			recipe.AddTile(TileID.MythrilAnvil); //fabricator
			recipe.Register();
		}
	}
}
