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
using Origins.NPCs;
using GNPC = Origins.NPCs.OriginGlobalNPC;

namespace Origins.Items.Weapons.Explosives {
    public class Ace_Shrapnel_Old : ModItem {

        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Ancient Fragthrower");
			Tooltip.SetDefault("");
		}
		public override void SetDefaults() {
            item.CloneDefaults(ItemID.ProximityMineLauncher);
			item.damage = 85;
			item.noMelee = true;
            item.useStyle = 5;
			item.useTime = 28;
			item.useAnimation = 28;
            item.shootSpeed/=2;
			item.value = 5000;
            item.shoot = ModContent.ProjectileType<Ace_Shrapnel_Old_P>();
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
    public class Ace_Shrapnel_Old_P : ModProjectile {
        public override string Texture => "Origins/Projectiles/Pixel";
        internal sbyte[] npcCDs = null;

        public static int maxHits = 3;
        public static int hitCD = 5;

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
            if(npcCDs is null)npcCDs = new sbyte[Main.maxNPCs];
        }
        public override void AI() {
            Dust.NewDustDirect(projectile.Center, 0, 0, 6, Scale:0.4f).noGravity = true;
            if(projectile.ai[0]>0 && projectile.timeLeft%6==0) {
                projectile.ai[0]--;
                if(projectile.velocity.Length()<1) {
                    Vector2 v = Main.rand.NextVector2Unit()*6;
                    Projectile.NewProjectile(projectile.Center+v*8, v.RotatedBy(PiOver2), ModContent.ProjectileType<Ace_Shrapnel_Old_Shard>(), projectile.damage, projectile.knockBack, projectile.owner, projectile.whoAmI, projectile.ai[1]+1);
                    return;
                }
                Projectile.NewProjectile(projectile.Center, projectile.velocity.RotatedByRandom(1)*1.1f, ModContent.ProjectileType<Ace_Shrapnel_Old_Shard>(), projectile.damage, projectile.knockBack, projectile.owner, projectile.whoAmI, projectile.ai[1]+1);
            }
        }
        public override bool? CanHitNPC(NPC target) {
            return false;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            return false;
        }
        internal bool AddHit(int i) {
            if(projectile.localNPCImmunity[i]<=0){
                npcCDs[i] = 0;
            }
            projectile.localNPCImmunity[i] = hitCD;
            if(npcCDs[i]++==0) {
                return false;
            }
            return true;
        }
    }
    public class Ace_Shrapnel_Old_Shard : ModProjectile {

        const float cohesion = 0.5f;

        const double chaos = 0.1f;

        public override string Texture => "Terraria/Projectile_"+ProjectileID.BoneGloveProj;
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.Bullet);
            projectile.aiStyle = 0;
            projectile.penetrate = 3;
            projectile.extraUpdates = 0;
            projectile.width = projectile.height = 10;
            projectile.timeLeft = 120;
            projectile.ignoreWater = true;
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = 10;
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
        public override bool? CanHitNPC(NPC target) {
            Projectile parent = Main.projectile[(int)projectile.ai[0]];
            if(parent.modProjectile is Ace_Shrapnel_Old_P center) {
                return (parent.localNPCImmunity[target.whoAmI]>0&&center.npcCDs[target.whoAmI]>0)?(bool?)false:null;
            }
            return null;
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            target.immune[projectile.owner]/=2;
            if(target.life<=0 && projectile.ai[1]<5) {
                Projectile.NewProjectile(projectile.Center, Vector2.Zero, ModContent.ProjectileType<Ace_Shrapnel_Old_P>(), projectile.damage, projectile.knockBack, projectile.owner, 8-projectile.ai[1], projectile.ai[1]);
            } else {
                if(Main.projectile[(int)projectile.ai[0]]?.modProjectile is Ace_Shrapnel_Old_P center && center.AddHit(target.whoAmI)) {
                    projectile.penetrate++;
                }
            }
        }
    }
}
