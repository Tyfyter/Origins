using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Felnum {
    public class Felnum_Bow : ModItem {
        public const int baseDamage = 19;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Felnum Longbow");
            Tooltip.SetDefault("Receives 50% higher damage bonuses");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.GoldBow);
            Item.damage = baseDamage;
            Item.width = 18;
            Item.height = 58;
            Item.useTime = Item.useAnimation = 32;
            Item.shootSpeed*=2.5f;
            Item.autoReuse = false;
            Item.rare = ItemRarityID.Green;
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ModContent.ItemType<Felnum_Bar>(), 8);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
        public override Vector2? HoldoutOffset() {
            return new Vector2(-8f,0);
        }
        public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
            damage = damage.MultiplyBonuses(1.5f);
        }
    }
}
