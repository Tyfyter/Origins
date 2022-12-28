using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Other {
    public class Sleetfire : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Sleetfire");
            Tooltip.SetDefault("Uses gel as ammo");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.EldMelter);
            Item.damage = 9;
            Item.useAnimation = 20;
            Item.useTime = 4;
            Item.width = 36;
            Item.height = 16;
            Item.useAmmo = ItemID.Gel;
            Item.shoot = ModContent.ProjectileType<Sleetfire_P>();
            Item.shootSpeed = 7f;
            Item.reuseDelay = 9;
            Item.rare = ItemRarityID.White;
            Item.ArmorPenetration = 5;
            Item.UseSound = SoundID.Item34;
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.IceBlock, 30);
            recipe.AddIngredient(ItemID.Shiverthorn, 5);
            recipe.AddIngredient(ItemID.IceTorch);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
            position += velocity.SafeNormalize(default) * 36;
		}
	}
    public class Sleetfire_P : ModProjectile {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Sleetfire");
        }
        public override void SetDefaults() {
            Projectile.width = Projectile.height = 6;
            Projectile.penetrate = 2;
            Projectile.friendly = true;
            Projectile.alpha = 245;
            Projectile.extraUpdates = 1;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            DrawOriginOffsetX = -45;
            DrawOriginOffsetY = -45;
        }
		public override void AI() {
            Lighting.AddLight(Projectile.Center, 0f, 0.2f, 0.85f);
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.FrostStaff);
            Projectile.ai[0]++;
            Projectile.scale = Utils.Remap(Projectile.ai[0], 0f, 25f, 16f / 96f, 66f / 96f);
            Projectile.alpha = (int)(255 * (1 - (Projectile.ai[0] / 25f)));
			if (Projectile.ai[0] > 25) {
                Projectile.Kill();
			}
		}
		public override void ModifyDamageHitbox(ref Rectangle hitbox) {
            int scale = (int)Utils.Remap(Projectile.ai[0], 0f, 25f, 10f, 60f);
            hitbox.Inflate(scale, scale);
		}
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            target.AddBuff(BuffID.Frostburn, crit ? 600 : 300);
        }
        public override Color? GetAlpha(Color lightColor) {
			return new Color(0, 200, 255, 255) * (1 - Projectile.alpha / 255f);
		}
	}
}
