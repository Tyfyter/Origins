using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class No_U_Card : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("No U Card");
            Tooltip.SetDefault("Attackers recieve 50% of incoming damage and debuffs regardless of immunity\nImmune to most debuffs");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.YoYoGlove);
            Item.value = Item.sellPrice(gold: 4);
            Item.rare = ItemRarityID.LightPurple;
        }
        public override void UpdateEquip(Player player) {
            //player.GetModPlayer<OriginPlayer>().noU = true;
        }
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.AnkhCharm);
            recipe.AddIngredient(ModContent.ItemType<Return_To_Sender>());
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
        }
    }
}
