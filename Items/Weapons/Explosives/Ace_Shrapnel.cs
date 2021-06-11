using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.OriginExtensions;
using static Microsoft.Xna.Framework.MathHelper;

namespace Origins.Items.Weapons.Explosives {
    public class Ace_Shrapnel : ModItem {

        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Ace Shrapnel");
			Tooltip.SetDefault("Needs shard sprites?");
		}
		public override void SetDefaults() {
            item.CloneDefaults(ItemID.ProximityMineLauncher);
			item.damage = 150;
			item.noMelee = true;
            item.useStyle = 5;
			item.useTime = 20;
			item.useAnimation = 28;
            item.shootSpeed/=1;
			item.value = 5000;
            item.shoot = ModContent.ProjectileType<Ace_Shrapnel_P>();
			item.rare = ItemRarityID.Lime;
		}
        public override void AddRecipes() {
            Origins.AddExplosive(item);
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            type-=ModContent.ProjectileType<Ace_Shrapnel_P>();
            type/=3;
            Projectile.NewProjectile(position, new Vector2(speedX, speedY), item.shoot, damage, knockBack, player.whoAmI, 6+type, 0-type);
            return false;
        }
    }
    public class Ace_Shrapnel_P : ModProjectile {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Ace Shrapnel");
            Origins.ExplosiveProjectiles[projectile.type] = true;
		}
        public override string Texture => "Origins/Projectiles/Pixel";
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.Bullet);
            projectile.aiStyle = 0;
            projectile.penetrate = -1;
            projectile.extraUpdates = 0;
            projectile.width = projectile.height = 10;
            projectile.light = 0;
            projectile.timeLeft = 168;
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = 20;
            projectile.ignoreWater = true;
        }
        public override void AI() {
            Dust.NewDustDirect(projectile.Center, 0, 0, 6, Scale:0.4f).noGravity = true;
            if(projectile.ai[0]>0 && projectile.timeLeft%6==0) {
                projectile.ai[0]--;
                if(projectile.velocity.Length()<1) {
                    Vector2 v = Main.rand.NextVector2Unit()*6;
                    Projectile.NewProjectile(projectile.Center+v*8, v, ModContent.ProjectileType<Ace_Shrapnel_Shard>(), projectile.damage, projectile.knockBack, projectile.owner, projectile.whoAmI, projectile.ai[1]+1);
                    return;
                }
                Projectile.NewProjectile(projectile.Center, projectile.velocity.RotatedByRandom(1)*1.1f, ModContent.ProjectileType<Ace_Shrapnel_Shard>(), projectile.damage, projectile.knockBack, projectile.owner, projectile.whoAmI, projectile.ai[1]+1);
            }
        }
        public override bool? CanHitNPC(NPC target) {
            return false;//((int)projectile.ai[0]<=0)?null:((bool?)false);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            return false;
        }
    }
    public class Ace_Shrapnel_Shard : ModProjectile {

        const float cohesion = 0.1f;

        const double chaos = Math.PI;

        public override string Texture => "Terraria/Projectile_"+ProjectileID.BoneGloveProj;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Shrapnel");
            Origins.ExplosiveProjectiles[projectile.type] = true;
		}
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.Bullet);
            projectile.aiStyle = 0;
            projectile.penetrate = 3;
            projectile.extraUpdates = 0;
            projectile.width = projectile.height = 10;
            projectile.timeLeft = 240;
            projectile.ignoreWater = true;
        }
        public override void AI() {
            Dust.NewDustPerfect(projectile.Center, 1, Vector2.Zero).noGravity = true;
            if(projectile.ai[0]>=0) {
                Projectile center = Main.projectile[(int)projectile.ai[0]];
                if(!center.active) {
                    projectile.ai[0] = -1;
                    return;
                }
                projectile.velocity = projectile.velocity.RotatedByRandom(chaos);
                //float angle = projectile.velocity.ToRotation();
                float targetAngle = (center.Center - projectile.Center).ToRotation();
                projectile.velocity = (projectile.velocity+new Vector2(cohesion*(projectile.ai[1]>1?2:1),0).RotatedBy(targetAngle)).SafeNormalize(Vector2.Zero)*projectile.velocity.Length();
                //projectile.velocity = projectile.velocity.RotatedBy(Clamp((float)AngleDif(targetAngle,angle), -0.05f, 0.05f));
                //Dust.NewDustDirect(projectile.Center+new Vector2(16,0).RotatedBy(targetAngle), 0, 0, 6, Scale:2).noGravity = true;
            }
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            target.immune[projectile.owner]/=2;
            if(target.life<=0 && projectile.ai[1]<5) {
                Projectile.NewProjectile(projectile.Center, Vector2.Zero, ModContent.ProjectileType<Ace_Shrapnel_P>(), projectile.damage, projectile.knockBack, projectile.owner, 8-projectile.ai[1], projectile.ai[1]);
            }
        }
    }
}
