using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
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
        bool felnumEffect = false;
        bool viperEffect = false;
        public static bool felnumEffectNext = false;
        public static bool viperEffectNext = false;
        public static bool hostileNext = false;
        public override void SetDefaults(Projectile projectile) {
            if(hostileNext) {
                projectile.hostile = true;
                hostileNext = false;
            }
            felnumEffect = felnumEffectNext;
            felnumEffectNext = false;
            if(viperEffectNext) {
                viperEffect = true;
                projectile.extraUpdates+=2;
                viperEffectNext = false;
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
                }else Dust.NewDustPerfect(projectile.Center, 226, projectile.velocity.RotatedByRandom(0.1)*0.5f, Scale:0.5f);
            }
            if(viperEffect) {
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
            if(Origins.ExplosiveProjectiles[projectile.type] && Main.rand.Next(1, 101) <= originPlayer.explosiveCrit){
				crit = true;
			}
            if(viperEffect) {
                for(int i = 0; i < target.buffType.Length; i++) {
                    if(Main.debuff[target.buffType[i]]) {
                        crit = true;
                        break;
                    }
                }
            }
        }
        public override bool PreKill(Projectile projectile, int timeLeft) {
            if(felnumEffect&&projectile.aiStyle==60) {
                OriginPlayer originPlayer = Main.player[projectile.owner].GetModPlayer<OriginPlayer>();
                Projectile.NewProjectileDirect(projectile.Center, Vector2.Zero, ModContent.ProjectileType<Shock_Grenade_Shock>(), (int)(originPlayer.felnumShock / 2.5f), projectile.knockBack, projectile.owner).timeLeft = 1;
                originPlayer.felnumShock = 0;
                Main.PlaySound(SoundID.Item, (int)projectile.Center.X, (int)projectile.Center.Y, 122, 2f, 1f);
            }
            return true;
        }
        public override void OnHitNPC(Projectile projectile, NPC target, int damage, float knockback, bool crit) {
            if(Origins.ExplosiveProjectiles[projectile.type]) {
                OriginPlayer originPlayer = Main.player[projectile.owner].GetModPlayer<OriginPlayer>();
                if(originPlayer.madHand) {
                    target.AddBuff(BuffID.Oiled, 600);
                    target.AddBuff(BuffID.OnFire, 600);
                }
            }
        }
        public override void ModifyDamageHitbox(Projectile projectile, ref Rectangle hitbox) {
            if(Origins.ExplosiveProjectiles[projectile.type]) {
                OriginPlayer originPlayer = Main.player[projectile.owner].GetModPlayer<OriginPlayer>();
                if(originPlayer.madHand&&(projectile.timeLeft<=3||projectile.penetrate==0)){
                    hitbox.Inflate(hitbox.Width/4,hitbox.Height/4);
                }
            }
        }
    }
}
