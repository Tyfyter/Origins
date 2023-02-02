using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ammo {
    public class Acid_Harpoon : ModItem {
        public override string Texture => "Origins/Items/Weapons/Ammo/Acid_Harpoon";
        public static int ID { get; private set; } = -1;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Acid Harpoon");
            SacrificeTotal = 99;
            ID = Type;
        }
        public override void SetDefaults() {
            Item.damage = 16;
            Item.DamageType = DamageClass.Ranged;
            Item.consumable = true;
            Item.maxStack = 99;
            Item.shoot = Acid_Harpoon_P.ID;
            Item.ammo = Harpoon.ID;
            Item.value = Item.sellPrice(silver: 28);
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type, 8);
            recipe.AddRecipeGroup(RecipeGroupID.IronBar, 8);
            recipe.AddIngredient(ModContent.ItemType<Bottled_Brine>());
            recipe.AddTile(TileID.Anvils);
            recipe.Register();

            recipe = Recipe.Create(Type, 8);
            recipe.AddIngredient(ModContent.ItemType<Harpoon>(), 8);
            recipe.AddIngredient(ModContent.ItemType<Bottled_Brine>());
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
    public class Acid_Harpoon_P : Harpoon_P {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Harpoon;
		public static new int ID { get; private set; } = -1;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Acid Harpoon");
            ID = Type;
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            target.AddBuff(BuffID.Venom, Main.rand.Next(270, 360));
        }
    }
}
