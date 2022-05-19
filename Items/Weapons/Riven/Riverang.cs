using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Riven {
	public class Riverang : ModItem {
        static short glowmask;
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Riverang");
			Tooltip.SetDefault("Not very aerodynamic");
            glowmask = Origins.AddGlowMask(this);
        }
		public override void SetDefaults() {
            item.CloneDefaults(ItemID.ThornChakram);
			item.damage = 28;
			item.width = 22;
			item.height = 28;
			item.useTime = 18;
			item.useAnimation = 18;
			//item.knockBack = 5;
            item.shoot = ModContent.ProjectileType<Riverang_P>();
            item.shootSpeed = 9.75f;
			item.value = 5000;
			item.rare = ItemRarityID.Blue;
			item.UseSound = SoundID.Item1;
            item.glowMask = glowmask;
        }
        public override bool CanUseItem(Player player) {
            return player.ownedProjectileCounts[item.shoot]<=0;
        }
    }
    public class Riverang_P : ModProjectile {
        public override string Texture => "Origins/Items/Weapons/Riven/Riverang";
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Riverang");
		}
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.ThornChakram);
            projectile.penetrate = -1;
			projectile.width = 28;
			projectile.height = 28;
            //projectile.scale*=1.25f;
        }
        public override bool PreAI() {
            if(projectile.timeLeft % 10 == 0) {
			    Vector2 targetPos = projectile.Center;
			    bool foundTarget = false;
                Vector2 testPos;
                if(projectile.localAI[1]>0) {
                    projectile.localAI[1]--;
                }
                if(projectile.localAI[0]>0) {
                    projectile.localAI[0]--;
                    goto skip;
                }
                for (int i = 0; i < Main.maxNPCs; i++) {
				    NPC target = Main.npc[i];
				    if (target.CanBeChasedBy()) {
                        testPos = projectile.Center.Clamp(target.Hitbox);
					    Vector2 difference = testPos-projectile.Center;
                        float distance = difference.Length();
					    bool closest = Vector2.Distance(projectile.Center, targetPos) > distance;
                        bool inRange = distance < 40 && (difference.SafeNormalize(Vector2.Zero)*projectile.velocity.SafeNormalize(Vector2.Zero)).Length()>0.1f;
					    if ((!foundTarget || closest) && inRange) {
						    targetPos = testPos;
						    foundTarget = true;
					    }
				    }
			    }
                skip:
                if(foundTarget) {
                    projectile.velocity = (targetPos - projectile.Center).SafeNormalize(Vector2.UnitX)*projectile.velocity.Length();
                    projectile.localAI[1] = 10;
                } else {
                    projectile.velocity = projectile.velocity.RotatedByRandom(MathHelper.Min((3600f - projectile.timeLeft) * 0.025f, MathHelper.PiOver2));
                }
            }
            return true;
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            if(projectile.localAI[1]>0)projectile.localAI[0] = 20;
        }
        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough) {
            width = 27;
            height = 27;
            return true;
        }
    }
}
