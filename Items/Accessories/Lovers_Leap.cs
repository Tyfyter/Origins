using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class Lovers_Leap : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Lover's Leap");
            Tooltip.SetDefault("The wearer can run super fast and runs even faster after taking damage\nAllows the player to dash into the enemy\nDouble tap a direction");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.SpectreBoots);
            Item.value = Item.sellPrice(gold: 4);
            Item.rare = ItemRarityID.LightRed;
            Item.accessory = true;
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.SpectreBoots);
            recipe.AddIngredient(ModContent.ItemType<Locket_Necklace>());
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
        }
        public override void UpdateEquip(Player player) {
            //player.GetModPlayer<OriginPlayer>().loversLeap = true;
        }
    }
}
