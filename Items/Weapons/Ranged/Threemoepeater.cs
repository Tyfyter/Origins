using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ranged {
    public class Threemoepeater : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Threemoepeater");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.ChlorophyteShotbow);
            Item.damage = 14;
            Item.DamageType = DamageClass.Ranged;
            Item.knockBack = 5;
            Item.noMelee = true;
            Item.useTime = 13;
            Item.width = 50;
            Item.height = 10;
            Item.UseSound = SoundID.Item11;
            Item.value = Item.buyPrice(silver: 42);
            Item.rare = ItemRarityID.Blue;
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ModContent.ItemType<Encrusted_Bar>(), 12);
            recipe.AddIngredient(ModContent.ItemType<Riven_Sample>(), 5);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
}
