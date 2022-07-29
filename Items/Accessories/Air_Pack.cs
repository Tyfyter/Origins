using Terraria.GameContent.Creative;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.Items.Materials;

namespace Origins.Items.Accessories {
	public class Air_Pack : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Air Pack");
            Tooltip.SetDefault("Greatly extends underwater breathing\nImmunity to ‘Suffocation’");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.YoYoGlove);
            Item.handOffSlot = -1;
            Item.handOnSlot = -1;
            Item.rare = ItemRarityID.LightRed;
        }
        public override void UpdateEquip(Player player) {
            player.buffImmune[BuffID.Suffocation] = true;
            player.breathMax+=514;
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ModContent.ItemType<Adhesive_Wrap>(), 30);
            recipe.AddIngredient(ModContent.ItemType<Air_Tank>(), 2);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
        }
    }
}
