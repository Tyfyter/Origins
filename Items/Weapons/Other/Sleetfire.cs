using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Origins.Projectiles.Weapons;
using Terraria.GameContent.Creative;

namespace Origins.Items.Weapons.Other {
    public class Sleetfire : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Sleetfire");
            Tooltip.SetDefault("Uses gel as ammo");
            ItemID.Sets.SkipsInitialUseSound[Item.type] = true;
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.EldMelter);
            Item.damage = 12;
            Item.useAnimation = 20;
            Item.useTime = 4;
            Item.width = 36;
            Item.height = 16;
            Item.useAmmo = ItemID.Gel;
            Item.shoot = ModContent.ProjectileType<Sleetfire_P>();
            Item.shootSpeed = 7f;
            Item.rare = ItemRarityID.Orange;
            Item.reuseDelay = 9;
            Item.ArmorPenetration = 5;
        }
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
            position += velocity.SafeNormalize(default) * 36;
		}
	}
    public class Sleetfire_P : ModProjectile {
        //public override string Texture => "Terraria/Images/Projectile_85";
        public override string Texture => "Terraria/Images/Projectile_465";//temp texture
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Sleetfire");
            //Main.projFrames[Type] = 7;
            Main.projFrames[Type] = 4;//temp value for temp texture
        }
        public override void SetDefaults() {
            Projectile.width = Projectile.height = 6;
            Projectile.penetrate = 2;
            Projectile.friendly = true;
            Projectile.alpha = 255;
            Projectile.extraUpdates = 1;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            DrawOriginOffsetX = -45;
            DrawOriginOffsetY = -45;
        }
		public override void AI() {
            Projectile.ai[0]++;
            Projectile.scale = Utils.Remap(Projectile.ai[0], 0f, 45f, 16f / 96f, 66f / 96f);
            Projectile.alpha = (int)(255 * (1 - (Projectile.ai[0] / 45f)));
			if (Projectile.ai[0] > 45) {
                Projectile.Kill();
			}
		}
		public override void ModifyDamageHitbox(ref Rectangle hitbox) {
            int scale = (int)Utils.Remap(Projectile.ai[0], 0f, 45f, 10f, 60f);
            hitbox.Inflate(scale, scale);
		}
		public override Color? GetAlpha(Color lightColor) {
			return new Color(0, 200, 255, 255) * (1 - Projectile.alpha / 255f);
		}
	}
}
