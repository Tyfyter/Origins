using Origins.Items.Materials;
using Origins.Items.Other.Consumables;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class Potato_Battery : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Potato Battery");
			Tooltip.SetDefault("Magic projectiles slightly home towards targets\n'How are you holding up? BECAUSE I'M A POTATO'");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(30, 28);
			Item.accessory = true;
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(gold: 1);
		}
		public override void UpdateEquip(Player player) {
			//player.GetModPlayer<OriginPlayer>().priorityMail = true;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 2);
			recipe.AddIngredient(ItemID.CopperOre, 3);
			recipe.AddIngredient(ModContent.ItemType<Potato>());
			recipe.AddIngredient(ModContent.ItemType<Power_Core>());
			recipe.AddTile(TileID.MythrilAnvil); //fabricator
			recipe.Register();
		}
	}
}
