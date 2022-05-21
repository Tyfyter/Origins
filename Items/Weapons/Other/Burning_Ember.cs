using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Origins.Projectiles.Weapons;
using Microsoft.Xna.Framework.Graphics;
using Tyfyter.Utils;

namespace Origins.Items.Weapons.Other {
    public class Burning_Ember : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Burning Ember");
            Tooltip.SetDefault("");
            Item.staff[item.type] = true;
        }
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.Flamelash);
            item.damage = 25;
            item.useStyle = 5;
            item.useAnimation = 11;
            item.useTime = 11;
            item.width = 78;
            item.height = 78;
            item.mana = 3;
            item.shoot = ModContent.ProjectileType<Burning_Ember_P>();
            item.shootSpeed = 8f;
            item.autoReuse = true;
            //item.scale = 0.8f;
        }
    }
    public class Burning_Ember_P : ModProjectile {
        public override string Texture => "Origins/Projectiles/Weapons/Fire_Wave_P";

        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Burning Ember");
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 19;
        }
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.DD2SquireSonicBoom);
            projectile.extraUpdates = 1;
            projectile.aiStyle = 0;
            projectile.timeLeft = 200;
        }
        public override void AI() {
            if(projectile.timeLeft >= 80)projectile.rotation = projectile.velocity.ToRotation();
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            if(Main.rand.NextBool())target.AddBuff(BuffID.OnFire, 300);
            if(projectile.penetrate == 1) {
                projectile.penetrate = 2;
                projectile.timeLeft = 80;
                projectile.velocity = Vector2.Zero;
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
            if(projectile.timeLeft < 80) {
                return false;
            }
            Rectangle hitbox;
            Vector2 offset = Vector2.Zero;
            Vector2 velocity = projectile.velocity.RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.UnitY)*8;
            for(int n = 0; n < 4; n++) {
                offset += velocity;
                hitbox = projHitbox;
                hitbox.Offset(offset.ToPoint());
                if(hitbox.Intersects(targetHitbox))return true;
                hitbox = projHitbox;
                hitbox.Offset((-offset).ToPoint());
                if(hitbox.Intersects(targetHitbox))return true;
            }
            return null;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            Texture2D texture = Main.projectileTexture[projectile.type];
            Vector2[] positions = new Vector2[projectile.oldPos.Length+1];
            projectile.oldPos.CopyTo(positions, 1);
            positions[0] = projectile.position;
            Vector2 halfSize = new Vector2(projectile.width, projectile.height) * 0.5f;
            int alpha = 0;
            int min = Math.Max(80-projectile.timeLeft, 0)/2;
            for(int i = positions.Length - 1; i >= min; i--) {
                if((i&3)!=0) {
                    continue;
                }
                alpha = (20 - i) * 10;
                spriteBatch.Draw(
                    texture,
                    (positions[i] + halfSize) - Main.screenPosition,
                    null,
                    new Color(alpha, alpha, alpha, alpha),
                    projectile.rotation,
                    texture.Size()*0.5f,
                    projectile.scale,
                    projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                    0);
            }
            Vector2 offset = Vector2.Zero;
            Vector2 velocity = (Vector2)new PolarVec2(8, projectile.rotation+MathHelper.PiOver2);
            const int backset = 2;
            if(min+backset<positions.Length)for(int n = 0; n < 4; n++) {
                offset += velocity;
                if(Main.rand.NextBool(3))Dust.NewDust(positions[min+backset] + offset, projectile.width, projectile.height, DustID.Fire);
                if(Main.rand.NextBool(3))Dust.NewDust(positions[min+backset] - offset, projectile.width, projectile.height, DustID.Fire);
            }
            //spriteBatch.Draw(mod.GetTexture("Projectiles/ClawFeather"), oldPositions[oldPositions.Count-1], null, lightColor, projectile.rotation, projectile.Center, 1, SpriteEffects.None, 0);
            return false;
        }
    }
}
