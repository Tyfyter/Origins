using Microsoft.Xna.Framework;
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
            Item.value = Item.sellPrice(silver: 9, copper: 1);
            Item.rare = ItemRarityID.Orange;
		}
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type, 50);
            recipe.AddIngredient(ItemID.EmptyBullet, 50);
            recipe.AddIngredient(ModContent.ItemType<Alkahest>());
            recipe.Register();
        }
    }
    public class Alkahest_Bullet_P : ModProjectile {
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.CursedBullet);
            Projectile.aiStyle = 0;
        }
		public override void AI() {
            Projectile.rotation = Projectile.velocity.ToRotation();
            if (Projectile.alpha > 0)
                Projectile.alpha -= 15;
            if (Projectile.alpha < 0)
                Projectile.alpha = 0;
            //TODO: add light
        }
		public override Color? GetAlpha(Color lightColor) {
			if (Projectile.alpha < 200) {
				return new Color(255 - Projectile.alpha, 255 - Projectile.alpha, 255 - Projectile.alpha, 0);
			}
			return Color.Transparent;
		}
		public override void Kill(int timeLeft) {
            Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
            SoundEngine.PlaySound(SoundID.Shatter.WithVolume(0.5f), Projectile.position);
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            OriginGlobalNPC.InflictTorn(target, 180, 180, 0.75f);
        }
    }
}
