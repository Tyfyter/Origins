using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Origins.Projectiles.Weapons;
using Terraria.GameContent.Creative;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;

namespace Origins.Items.Weapons.Other {
    public class Sleetfire_Alt : ModItem {
		public override string Texture => "Origins/Items/Weapons/Other/Sleetfire";
		public override void SetStaticDefaults() {
            DisplayName.SetDefault("Sleetfire (Alt example)");
            Tooltip.SetDefault("Uses gel as ammo");
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
            Item.shoot = ModContent.ProjectileType<Sleetfire_Alt_P>();
            Item.shootSpeed = 7f;
            Item.rare = ItemRarityID.Orange;
            Item.reuseDelay = 9;
            Item.ArmorPenetration = 5;
        }
		public override Vector2? HoldoutOffset() {
			return new Vector2(4, 0);
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
            position += velocity.SafeNormalize(default) * 36;
		}
	}
    public class Sleetfire_Alt_P : ModProjectile {
        //public override string Texture => "Terraria/Images/Projectile_85";
        public override string Texture => "Origins/Items/Weapons/Other/Projectile_85";
        public static float Lifetime => 30f;
        public static float MinSize => 16f;
        public static float MaxSize => 66f;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Sleetfire");
            Main.projFrames[Type] = 7;
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
        }
		public override void AI() {
            Projectile.ai[0]++;
            Projectile.scale = Utils.Remap(Projectile.ai[0], 0f, Lifetime, MinSize / 96f, MaxSize / 96f);
            Projectile.alpha = (int)(200 * (1 - (Projectile.ai[0] / Lifetime)));
            Projectile.rotation += 0.3f * Projectile.direction;
            if (Projectile.ai[0] > Lifetime) {
                Projectile.Kill();
			}
		}
		public override void ModifyDamageHitbox(ref Rectangle hitbox) {
            int scale = (int)Utils.Remap(Projectile.ai[0], 0f, Lifetime, MinSize - 6, MaxSize - 6);
            hitbox.Inflate(scale, scale);
		}
		public override bool PreDraw(ref Color lightColor) {
            //simplified drawing because vanilla flamethrower drawing is really complicated and not worth replicating unless it's actually going to be used
			Texture2D texture = TextureAssets.Projectile[Type].Value;
            Rectangle frame = texture.Frame(verticalFrames: Main.projFrames[Type], frameY: 3);
            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                frame,
                new Color(128, 200, 255, 128) * (1 - Projectile.alpha / 255f),
                Projectile.rotation,
                frame.Size() * 0.5f,
                Projectile.scale,
                SpriteEffects.None,
            0);
			return false;
		}
	}
}
