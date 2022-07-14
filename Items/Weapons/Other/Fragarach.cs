using Microsoft.Xna.Framework;
using Origins.Projectiles.Misc;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Other {
    public class Fragarach : ModItem {
        static short glowmask;
        //public override bool OnlyShootOnSwing => true;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Fragarach");
            Tooltip.SetDefault("");
            glowmask = Origins.AddGlowMask(this);
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.TerraBlade);
            Item.damage = 62;
            Item.DamageType = DamageClass.Melee;
            Item.noUseGraphic = false;
            Item.noMelee = false;
            Item.width = 58;
            Item.height = 58;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.knockBack = 9.5f;
            Item.value = 500000;
            Item.shoot = ProjectileID.None;
            Item.rare = ItemRarityID.Purple;
            Item.shoot = ModContent.ProjectileType<Fragarach_P>();
            Item.shootSpeed = 8f;
            Item.autoReuse = true;
            Item.scale = 1f;
            Item.glowMask = glowmask;
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips) {
            tooltips[0].OverrideColor = new Color(50, 230, 230).MultiplyRGBA(Main.MouseTextColorReal);
        }
    }
    public class Fragarach_P : ModProjectile {
        public static int ID { get; private set; }
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Fragarach");
            ID = Projectile.type;
        }
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.TerraBeam);
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.penetrate = 5;
            Projectile.extraUpdates = 1;
            Projectile.ai[0] = 21;
        }
        public override bool PreAI() {
            if(Projectile.ai[1] == 0f) {
                Projectile.ai[1] = 1f;
                SoundEngine.PlaySound(SoundID.Item109.WithPitch(-0.25f).WithVolume(1.5f), Projectile.Center);
                Projectile.ai[0] = 21;
            }
            return true;
        }
        public override void AI() {
            if(Projectile.ai[0] > 0) {
                Projectile.ai[0]--;
            }else if(Projectile.timeLeft%7==0) {
                Vector2 pos = Projectile.position + new Vector2(Main.rand.Next(Projectile.width), Main.rand.Next(Projectile.height));
                Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), pos, Main.rand.NextVector2CircularEdge(3,3), Felnum_Shock_Leader.ID, Projectile.damage/6, 0, Projectile.owner, pos.X, pos.Y);
                ModProjectile parent = this;
                //Projectile parentProjectile = this.projectile;
                if(proj.ModProjectile is Felnum_Shock_Leader shock) {
                    shock.OnStrike += () => {
                        if(parent == Projectile.ModProjectile) {
                            Projectile.ai[0] = 14;
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
			SoundEngine.PlaySound(SoundID.Item10.WithVolume(2), Projectile.Center);
            Vector2 shockVelocity = -Projectile.oldVelocity;
            shockVelocity.Normalize();
            shockVelocity *= 3;
			for (int i = 4; i < 31; i++) {
                if(i%2==0) {
                    Vector2 pos = Projectile.position + new Vector2(Main.rand.Next(Projectile.width), Main.rand.Next(Projectile.height));
                    Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), pos, shockVelocity.RotatedByRandom(2.5), Felnum_Shock_Leader.ID, Projectile.damage/3, 0, Projectile.owner, pos.X, pos.Y).usesLocalNPCImmunity = false;
                }
				float offsetX = Projectile.oldVelocity.X * (30f / i);
				float offsetY = Projectile.oldVelocity.Y * (30f / i);
				int dustIndex = Dust.NewDust(new Vector2(Projectile.oldPosition.X - offsetX, Projectile.oldPosition.Y - offsetY), 8, 8, DustID.Clentaminator_Cyan, Projectile.oldVelocity.X, Projectile.oldVelocity.Y, 100, default(Color), 1.8f);
				Main.dust[dustIndex].noGravity = true;
				Dust dust1 = Main.dust[dustIndex];
				Dust dust2 = dust1;
				dust2.velocity *= 0.5f;
				dustIndex = Dust.NewDust(new Vector2(Projectile.oldPosition.X - offsetX, Projectile.oldPosition.Y - offsetY), 8, 8, DustID.Clentaminator_Cyan, Projectile.oldVelocity.X, Projectile.oldVelocity.Y, 100, default(Color), 1.4f);
				dust1 = Main.dust[dustIndex];
				dust2 = dust1;
				dust2.velocity *= 0.05f;
                dust2.noGravity = true;
			}
        }
        public override Color? GetAlpha(Color lightColor) {
            if (Projectile.localAI[1] >= 15f){
			    return new Color(255, 255, 255, Projectile.alpha);
		    }
		    if (Projectile.localAI[1] < 5f){
			    return Color.Transparent;
		    }
		    int num7 = (int)((Projectile.localAI[1] - 5f) / 10f * 255f);
		    return new Color(num7, num7, num7, num7);
        }
        public override bool PreDraw(ref Color lightColor) {
            return true;
        }
    }
}
