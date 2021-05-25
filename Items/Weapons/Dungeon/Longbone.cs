using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Dungeon {
    public class Longbone : ModItem {
        internal static int t = ProjectileID.WoodenArrowFriendly;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Bolterbow");
            Tooltip.SetDefault("Turns most arrows into fragile bone arrows");
        }
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.GoldBow);
            item.damage = 28;
			item.knockBack = 5;
			item.crit = 4;
            item.useTime = item.useAnimation = 16;
			item.shoot = ModContent.ProjectileType<Bone_Bolt>();
            item.shootSpeed = 9;
            item.width = 24;
            item.height = 56;
            item.autoReuse = false;
        }
        public override Vector2? HoldoutOffset() {
            return new Vector2(-8f,0);
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            if(type < ProjectileID.Count) {
                t = type;
                type = item.shoot;
            }
            return true;
        }
    }
    public class Bone_Bolt : ModProjectile {
        public static int ID { get; private set; } = -1;
        public override string Texture => "Terraria/Projectile_117";
        public override void SetStaticDefaults() {
            ID = projectile.type;
            DisplayName.SetDefault("Bone Bolt");
        }
        public override void SetDefaults() {
            projectile.CloneDefaults(Longbone.t);
            projectile.timeLeft = 30;
            projectile.extraUpdates = 1;
            projectile.localAI[0] = Longbone.t;
            projectile.localNPCHitCooldown = 10;
            projectile.usesLocalNPCImmunity = true;
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            projectile.type = (int)projectile.localAI[0];
            projectile.StatusNPC(target.whoAmI);
            projectile.type = ID;
            if((int)projectile.localAI[0] == ProjectileID.HellfireArrow) {
                projectile.Kill();
            }
        }
        public override bool PreKill(int timeLeft) {
            if((int)projectile.localAI[0] == ProjectileID.HellfireArrow) {
                int t = Bone_Shard.ID;
                Longbone.t = (int)projectile.localAI[0];
                Projectile.NewProjectileDirect(projectile.Center, projectile.velocity.RotatedByRandom(0.3f), t, projectile.damage/5, 2, projectile.owner).localNPCImmunity = projectile.localNPCImmunity;
                Projectile.NewProjectileDirect(projectile.Center, projectile.velocity.RotatedByRandom(0.3f), t, projectile.damage/5, 2, projectile.owner).localNPCImmunity = projectile.localNPCImmunity;
                Projectile.NewProjectileDirect(projectile.Center, projectile.velocity.RotatedByRandom(0.3f), t, projectile.damage/5, 2, projectile.owner).localNPCImmunity = projectile.localNPCImmunity;
                SoundEffectInstance sei = Main.PlaySound(SoundID.NPCHit2, projectile.Center);
                if(sei is null) {
                    projectile.SetToType(ProjectileID.HellfireArrow);
                    return false;
                }
                sei.Volume*=0.75f;
                sei.Pitch = Main.rand.NextFloat(0.1f,0.2f);
                projectile.SetToType(ProjectileID.HellfireArrow);
                return true;
            }
            return true;
        }
        public override void Kill(int timeLeft) {
            int t = Bone_Shard.ID;
            Longbone.t = (int)projectile.localAI[0];
            Projectile.NewProjectileDirect(projectile.Center, projectile.velocity.RotatedByRandom(0.3f), t, projectile.damage/5, 2, projectile.owner).localNPCImmunity = projectile.localNPCImmunity;
            Projectile.NewProjectileDirect(projectile.Center, projectile.velocity.RotatedByRandom(0.3f), t, projectile.damage/5, 2, projectile.owner).localNPCImmunity = projectile.localNPCImmunity;
            Projectile.NewProjectileDirect(projectile.Center, projectile.velocity.RotatedByRandom(0.3f), t, projectile.damage/5, 2, projectile.owner).localNPCImmunity = projectile.localNPCImmunity;
            SoundEffectInstance sei = Main.PlaySound(SoundID.NPCHit2, projectile.Center);
            if(sei is null) {
                return;
            }
            sei.Volume*=0.75f;
            sei.Pitch = Main.rand.NextFloat(0.1f,0.2f);
        }
        internal static void Render(Projectile projectile, SpriteBatch spriteBatch, Color lightColor) {
            int d;
            switch((int)projectile.localAI[0]) {
                case ProjectileID.FireArrow:
                lightColor.R+=(byte)Math.Min(80,255-lightColor.R);
                lightColor.G+=(byte)Math.Min(60,255-lightColor.G);
                lightColor.B+=(byte)Math.Min(10,255-lightColor.B);
                Dust.NewDust(projectile.Center, 0, 0, 6);
                break;
                case ProjectileID.FrostburnArrow:
                lightColor.G+=(byte)Math.Min(60,255-lightColor.G);
                lightColor.B+=(byte)Math.Min(80,255-lightColor.B);
                Dust.NewDust(projectile.Center, 0, 0, 135);
                break;
                case ProjectileID.CursedArrow:
                lightColor.R+=(byte)Math.Min(30,255-lightColor.R);
                lightColor.G+=(byte)Math.Min(80,255-lightColor.G);
                d = Dust.NewDust(projectile.Center, 0, 0, 178, Scale:0.75f);
                Main.dust[d].velocity *= 0.5f;
                break;
                case ProjectileID.IchorArrow:
                lightColor.R+=(byte)Math.Min(80,255-lightColor.R);
                lightColor.G+=(byte)Math.Min(80,255-lightColor.G);
                Dust.NewDust(projectile.Center, 0, 0, 228);
                break;
                case ProjectileID.VenomArrow:
                d = Dust.NewDust(projectile.Center, 0, 0, 98);
                Main.dust[d].noGravity = true;
                break;
                case ProjectileID.HellfireArrow:
                lightColor.R+=(byte)Math.Min(80,255-lightColor.R);
                lightColor.G+=(byte)Math.Min(60,255-lightColor.G);
                lightColor.B+=(byte)Math.Min(10,255-lightColor.B);
                Dust.NewDust(projectile.Center, 0, 0, 6);
                break;
            }
            for (int i = 1; i < 5; i++){
				float x = projectile.velocity.X * i;
				float y = projectile.velocity.Y * i;
				Color color = projectile.GetAlpha(lightColor);
				float a = 0f;
				switch(i){
                    case 1:
					a = 0.4f;
                    break;
                    case 2:
					a = 0.3f;
                    break;
                    case 3:
					a = 0.2f;
                    break;
                    case 4:
					a = 0.1f;
                    break;
				}
				color.R = (byte)(color.R * a);
				color.G = (byte)(color.G * a);
				color.B = (byte)(color.B * a);
				color.A = (byte)(color.A * a);
                Texture2D texture = Main.projectileTexture[projectile.type];
				spriteBatch.Draw(texture, new Vector2(projectile.position.X - Main.screenPosition.X - x, projectile.position.Y - Main.screenPosition.Y + (float)(projectile.height / 2) + projectile.gfxOffY - y), new Rectangle(0, 0, texture.Width, texture.Height), color, projectile.rotation, new Vector2(texture.Width/2f,texture.Height/2f), projectile.scale, SpriteEffects.None, 0f);
			}
        }
        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor) {
            Render(projectile, spriteBatch, lightColor);
        }
    }
    public class Bone_Shard : ModProjectile {
        public static int ID { get; private set; } = -1;
        public override string Texture => "Origins/Projectiles/Weapons/BoneS_hard";
        public override void SetStaticDefaults() {
            ID = projectile.type;
            DisplayName.SetDefault("BoneS hard");
        }
        public override void SetDefaults() {
            projectile.CloneDefaults(Longbone.t);
            projectile.extraUpdates = 1;
            projectile.localNPCHitCooldown = 10;
            projectile.usesLocalNPCImmunity = true;
            projectile.localAI[0] = Longbone.t;
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            projectile.type = (int)projectile.localAI[0];
            projectile.StatusNPC(target.whoAmI);
            projectile.type = ID;
            if((int)projectile.localAI[0] == ProjectileID.HellfireArrow) {
                projectile.Kill();
            }
        }
        public override bool PreKill(int timeLeft) {
            projectile.SetToType((int)projectile.localAI[0]);
            SoundEffectInstance sei = Main.PlaySound(SoundID.NPCHit2, projectile.Center);
            if(sei is null) {
                return false;
            }
            sei.Volume*=0.75f;
            sei.Pitch = Main.rand.NextFloat(0.2f,0.3f);
            return true;
        }
        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor) {
            Bone_Bolt.Render(projectile, spriteBatch, lightColor);
        }
    }
}
