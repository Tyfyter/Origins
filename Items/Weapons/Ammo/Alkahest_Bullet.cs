using Origins.Items.Materials;
using Origins.NPCs;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ammo {
    public class Alkahest_Bullet : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Alkahest Bullet");
			Tooltip.SetDefault("Tenderizes the target");
            SacrificeTotal = 99;
        }
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.CursedBullet);
            Item.maxStack = 999;
            Item.damage = 12;
            Item.shoot = ModContent.ProjectileType<Alkahest_Bullet_P>();
			Item.shootSpeed = 5f;
            Item.knockBack = 4f;
			Item.rare = ItemRarityID.Orange;
		}
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type, 50);
            recipe.AddIngredient(ItemID.EmptyBullet, 50);
            recipe.AddIngredient(ModContent.ItemType<Amebic_Gel>());
            recipe.Register();
        }
    }
    public class Alkahest_Bullet_P : ModProjectile {
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.CursedBullet);
            Projectile.penetrate = 1;
            Projectile.width = 20;
            Projectile.height = 2;
        }
        public override void Kill(int timeLeft) {
            SoundEngine.PlaySound(SoundID.Shatter, Projectile.position);
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            OriginGlobalNPC.InflictTorn(target, 180, 180, 0.75f);
        }
    }
}
