using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class Makeover_Choker : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Makeover Choker");
            Tooltip.SetDefault("Increases life regeneration at low health\nIncreases length of invincibility after taking damage\n'You look like a mess, let's get that taken care of'");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.accessory = true;
            Item.width = 21;
            Item.height = 20;
            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(gold: 10);
        }
        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.longInvince = true;
            Mysterious_Spray.EquippedEffect(player);
			if (!hideVisual) {
                Mysterious_Spray.VanityEffect(player);
            }
        }
		public override void UpdateVanity(Player player) {
            Mysterious_Spray.VanityEffect(player);
        }
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.CrossNecklace);
            recipe.AddIngredient(ModContent.ItemType<Mysterious_Spray>());
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
        }
    }
}
