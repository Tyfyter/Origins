using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class Last_Descendent : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Last Descendent");
            Tooltip.SetDefault("Increases damage, movement speed, length of invincibility after taking damage\nAdditionally causes stars to fall after taking damage");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.StarVeil);
            Item.value = Item.sellPrice(gold: 4);
            Item.rare = ItemRarityID.LightPurple;
            Item.accessory = true;
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.StarVeil);
            recipe.AddIngredient(ModContent.ItemType<Locket_Necklace>());
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
        }
        public override void UpdateEquip(Player player) {
            //player.GetModPlayer<OriginPlayer>().lastDescendent = true;
        }
    }
}
