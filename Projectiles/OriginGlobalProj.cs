using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Origins.Buffs;
using Origins.Items.Weapons.Felnum;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Projectiles {
    public class OriginGlobalProj : GlobalProjectile {
        public override bool InstancePerEntity => true;
        public override bool CloneNewInstances => true;
        //bool init = true;
        public bool felnumEffect = false;
        public bool viperEffect = false;
        public bool ownerSafe = false;
        bool? explosiveOverride = null;
        public int killLink = -1;
        public float godHunterEffect = 0f;
        //ModProjectile.SetDefaults is run before GlobalProjectiles' SetDefaults, so these can be used from SetDefaults
        public static bool felnumEffectNext = false;
        public static bool viperEffectNext = false;
        public static bool? explosiveOverrideNext = null;
        public static bool hostileNext = false;
        public static int killLinkNext = -1;
        public static int extraUpdatesNext = -1;
        public static float godHunterEffectNext = 0f;
        public override void SetDefaults(Projectile projectile) {
            if(hostileNext) {
                projectile.hostile = true;
                hostileNext = false;
            }
            felnumEffect = felnumEffectNext;
            felnumEffectNext = false;
            explosiveOverride = explosiveOverrideNext;
            explosiveOverrideNext = null;
            if(killLinkNext!=-1) {
                killLink = killLinkNext;
                Main.projectile[killLink].GetGlobalProjectile<OriginGlobalProj>().killLink = projectile.whoAmI;
                killLinkNext = -1;
            }
            if(viperEffectNext) {
                viperEffect = true;
                projectile.extraUpdates+=2;
                viperEffectNext = false;
            }
            if(extraUpdatesNext!=0) {
                projectile.extraUpdates+=extraUpdatesNext;
                extraUpdatesNext = 0;
            }
            if(godHunterEffectNext!=0) {
                godHunterEffect = godHunterEffectNext;
                godHunterEffectNext = 0;
            }
        }
        public override void AI(Projectile projectile) {
            switch(projectile.aiStyle) {
                case -1:
                projectile.rotation = projectile.velocity.ToRotation();
                break;
            }
            if(felnumEffect) {
                if(projectile.melee) {
                    if(Main.player[projectile.owner].GetModPlayer<OriginPlayer>().felnumShock>19)Dust.NewDustPerfect(projectile.Center, 226, projectile.velocity.RotatedByRandom(0.1)*0.5f, Scale:0.5f);
                } else Dust.NewDustPerfect(projectile.Center, 226, projectile.velocity.RotatedByRandom(0.1)*0.5f, Scale:0.5f);
            }
            if(viperEffect&&projectile.extraUpdates != 19) {
                Lighting.AddLight(projectile.Center, 0, 0.75f*projectile.scale, 0.3f*projectile.scale);
                Dust dust = Dust.NewDustPerfect(projectile.Center, 226, projectile.velocity.RotatedByRandom(0.1f)*-0.25f, 100, new Color(0, 255, 0), projectile.scale/2);
                dust.shader = GameShaders.Armor.GetSecondaryShader(18, Main.LocalPlayer);
                dust.noGravity = true;
                dust.noLight = true;
            }
        }
        public override void ModifyHitNPC(Projectile projectile, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            //this is actually how vanilla does projectile crits, which might explain why there are no vanilla multiclass weapons, since a 4% crit chance with a 4-class weapon would crit ~15% of the time
            OriginPlayer originPlayer = Main.player[projectile.owner].GetModPlayer<OriginPlayer>();
            if(IsExplosive(projectile) && Main.rand.Next(1, 101) <= originPlayer.explosiveCrit) {
				crit = true;
			}
            if(viperEffect) {
                bool crt = crit;
                for(int i = 0; i < target.buffType.Length; i++) {
                    if(Main.debuff[target.buffType[i]]&&target.buffType[i]!=SolventDebuff.ID) {
                        crit = true;
                        break;
                    }
                }
                if(crt || Main.rand.Next(0, 9)==0) {
                    target.AddBuff(SolventDebuff.ID, 450);
                }
            }
            if(target.boss && godHunterEffect != 0f) {
                damage += (int)(damage*godHunterEffect);
            }
        }
        public override bool CanHitPlayer(Projectile projectile, Player target) {
            return ownerSafe?target.whoAmI!=projectile.owner:true;
        }
        public override bool PreKill(Projectile projectile, int timeLeft) {
            if(felnumEffect&&projectile.type==ProjectileID.WaterGun) {//projectile.aiStyle==60
                OriginPlayer originPlayer = Main.player[projectile.owner].GetModPlayer<OriginPlayer>();
                Projectile.NewProjectileDirect(projectile.Center, Vector2.Zero, ModContent.ProjectileType<Shock_Grenade_Shock>(), (int)(originPlayer.felnumShock / 2.5f), projectile.knockBack, projectile.owner).timeLeft = 1;
                originPlayer.felnumShock = 0;
                Main.PlaySound(SoundID.Item, (int)projectile.Center.X, (int)projectile.Center.Y, 122, 2f, 1f);
            }
            return true;
        }
        public override void Kill(Projectile projectile, int timeLeft) {
            if(killLink != -1&&projectile.penetrate == 0) {
                Main.projectile[killLink].active = false;
                killLink = -1;
            }
        }
        public override void OnHitNPC(Projectile projectile, NPC target, int damage, float knockback, bool crit) {
            if(IsExplosive(projectile)) {
                OriginPlayer originPlayer = Main.player[projectile.owner].GetModPlayer<OriginPlayer>();
                if(originPlayer.madHand) {
                    target.AddBuff(BuffID.Oiled, 600);
                    target.AddBuff(BuffID.OnFire, 600);
                }
            }
        }
        public override void ModifyDamageHitbox(Projectile projectile, ref Rectangle hitbox) {
            if(IsExplosive(projectile)) {
                OriginPlayer originPlayer = Main.player[projectile.owner].GetModPlayer<OriginPlayer>();
                if(originPlayer.madHand&&(projectile.timeLeft<=3||projectile.penetrate==0)) {
                    hitbox.Inflate(hitbox.Width/4,hitbox.Height/4);
                }
            }
        }
        public bool IsExplosive(Projectile projectile) {
            return explosiveOverride??Origins.ExplosiveProjectiles[projectile.type];
        }
        public static bool IsExplosiveProjectile(Projectile projectile) {
            return projectile.GetGlobalProjectile<OriginGlobalProj>().explosiveOverride??Origins.ExplosiveProjectiles[projectile.type];
        }
        public static void ClentaminatorAI(Projectile projectile, int conversionType, int dustType, Color color) {
	        if (projectile.owner == Main.myPlayer) {
		        WorldGen.Convert((int)(projectile.Center.X) / 16, (int)(projectile.Center.Y) / 16, conversionType, 2);
	        }
	        if (projectile.timeLeft > 133) {
		        projectile.timeLeft = 133;
	        }
	        if (projectile.ai[0] > 7f) {
		        float scale = 1f;
                switch(projectile.ai[0]) {
                    case 8f:
			        scale = 0.2f;
                    break;
                    case 9f:
			        scale = 0.4f;
                    break;
                    case 10f:
			        scale = 0.6f;
                    break;
                    case 11f:
			        scale = 0.8f;
                    break;
                }
		        projectile.ai[0]++;
		        for (int num354 = 0; num354 < 1; num354++) {
			        int d = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, dustType, projectile.velocity.X * 0.2f, projectile.velocity.Y * 0.2f, 100, color);
			        Main.dust[d].noGravity = true;
			        Dust dust1 = Main.dust[d];
			        Dust dust2 = dust1;
			        dust2.scale *= 1.75f;
			        Main.dust[d].velocity.X *= 2f;
			        Main.dust[d].velocity.Y *= 2f;
			        dust1 = Main.dust[d];
			        dust2 = dust1;
			        dust2.scale *= scale;
		        }
	        } else {
		        projectile.ai[0]++;
	        }
	        projectile.rotation+=0.3f * projectile.direction;
        }
    }
}
