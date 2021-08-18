using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using Terraria.DataStructures;
using Origins.Projectiles.Misc;

namespace Origins.Items.Weapons.Other {
    public class Fragarach : ModItem {
        public override bool OnlyShootOnSwing => true;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Fragarach");
            Tooltip.SetDefault("");
        }
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.TerraBlade);
            item.damage = 62;
            item.melee = true;
            item.noUseGraphic = false;
            item.noMelee = false;
            item.width = 58;
            item.height = 58;
            item.useStyle = 1;
            item.useTime = 18;
            item.useAnimation = 18;
            item.knockBack = 9.5f;
            item.value = 500000;
            item.shoot = ProjectileID.None;
            item.rare = ItemRarityID.Purple;
            item.shoot = ModContent.ProjectileType<Fragarach_P>();
            item.shootSpeed = 8f;
            item.autoReuse = true;
            item.scale = 1f;
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips) {
            tooltips[0].overrideColor = new Color(50, 230, 230).MultiplyRGBA(Main.mouseTextColorReal);
        }
    }
    public class Fragarach_P : ModProjectile {
        public static int ID { get; private set; }
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Fragarach");
            ID = projectile.type;
        }
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.TerraBeam);
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = 10;
            projectile.penetrate = 5;
            projectile.extraUpdates = 1;
            projectile.ai[0] = 21;
        }
        public override bool PreAI() {
            if(projectile.ai[1] == 0f) {
                projectile.ai[1] = 1f;
                Main.PlaySound(SoundID.Item, (int)projectile.Center.X, (int)projectile.Center.Y, 109, 1.5f, -0.25f);
                projectile.ai[0] = 21;
            }
            return true;
        }
        public override void AI() {
            if(projectile.ai[0] > 0) {
                projectile.ai[0]--;
            }else if(projectile.timeLeft%7==0) {
                Vector2 pos = projectile.position + new Vector2(Main.rand.Next(projectile.width), Main.rand.Next(projectile.height));
                Projectile proj = Projectile.NewProjectileDirect(pos, Main.rand.NextVector2CircularEdge(3,3), Felnum_Shock_Leader.ID, projectile.damage/6, 0, projectile.owner, pos.X, pos.Y);
                ModProjectile parent = this;
                //Projectile parentProjectile = this.projectile;
                if(proj.modProjectile is Felnum_Shock_Leader shock) {
                    shock.OnStrike += () => {
                        if(parent == projectile.modProjectile) {
                            projectile.ai[0] = 14;
                        }
                    };
                }
            }
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            base.ModifyHitNPC(target, ref damage, ref knockback, ref crit, ref hitDirection);
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            base.OnHitNPC(target, damage, knockback, crit);
        }
        public override void Kill(int timeLeft) {
			Main.PlaySound(SoundID.Item, (int)projectile.Center.X, (int)projectile.Center.Y, 10, volumeScale:2);
            Vector2 shockVelocity = -projectile.oldVelocity;
            shockVelocity.Normalize();
            shockVelocity *= 3;
			for (int i = 4; i < 31; i++) {
                if(i%2==0) {
                    Vector2 pos = projectile.position + new Vector2(Main.rand.Next(projectile.width), Main.rand.Next(projectile.height));
                    Projectile.NewProjectileDirect(pos, shockVelocity.RotatedByRandom(2.5), Felnum_Shock_Leader.ID, projectile.damage/3, 0, projectile.owner, pos.X, pos.Y).usesLocalNPCImmunity = false;
                }
				float offsetX = projectile.oldVelocity.X * (30f / i);
				float offsetY = projectile.oldVelocity.Y * (30f / i);
				int dustIndex = Dust.NewDust(new Vector2(projectile.oldPosition.X - offsetX, projectile.oldPosition.Y - offsetY), 8, 8, 111, projectile.oldVelocity.X, projectile.oldVelocity.Y, 100, default(Color), 1.8f);
				Main.dust[dustIndex].noGravity = true;
				Dust dust1 = Main.dust[dustIndex];
				Dust dust2 = dust1;
				dust2.velocity *= 0.5f;
				dustIndex = Dust.NewDust(new Vector2(projectile.oldPosition.X - offsetX, projectile.oldPosition.Y - offsetY), 8, 8, 111, projectile.oldVelocity.X, projectile.oldVelocity.Y, 100, default(Color), 1.4f);
				dust1 = Main.dust[dustIndex];
				dust2 = dust1;
				dust2.velocity *= 0.05f;
                dust2.noGravity = true;
			}
        }
        public override Color? GetAlpha(Color lightColor) {
            if (projectile.localAI[1] >= 15f){
			    return new Color(255, 255, 255, projectile.alpha);
		    }
		    if (projectile.localAI[1] < 5f){
			    return Color.Transparent;
		    }
		    int num7 = (int)((projectile.localAI[1] - 5f) / 10f * 255f);
		    return new Color(num7, num7, num7, num7);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            return true;
        }
    }
}
