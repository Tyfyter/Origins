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
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.ThornChakram);
            Item.DamageType = DamageClass.MeleeNoSpeed;
			Item.damage = 28;
			Item.width = 22;
			Item.height = 28;
			Item.useTime = 18;
			Item.useAnimation = 18;
			//item.knockBack = 5;
            Item.shoot = ModContent.ProjectileType<Riverang_P>();
            Item.shootSpeed = 9.75f;
			Item.value = 5000;
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1;
            Item.glowMask = glowmask;
        }
        public override bool CanUseItem(Player player) {
            return player.ownedProjectileCounts[Item.shoot]<=0;
        }
    }
    public class Riverang_P : ModProjectile {
        public override string Texture => "Origins/Items/Weapons/Riven/Riverang";
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Riverang");
		}
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.ThornChakram);
            Projectile.penetrate = -1;
			Projectile.width = 28;
			Projectile.height = 28;
            //projectile.scale*=1.25f;
        }
        public override bool PreAI() {
            if(Projectile.timeLeft % 10 == 0) {
			    Vector2 targetPos = Projectile.Center;
			    bool foundTarget = false;
                Vector2 testPos;
                if(Projectile.localAI[1]>0) {
                    Projectile.localAI[1]--;
                }
                if(Projectile.localAI[0]>0) {
                    Projectile.localAI[0]--;
                    goto skip;
                }
                for (int i = 0; i < Main.maxNPCs; i++) {
				    NPC target = Main.npc[i];
				    if (target.CanBeChasedBy()) {
                        testPos = Projectile.Center.Clamp(target.Hitbox);
					    Vector2 difference = testPos-Projectile.Center;
                        float distance = difference.Length();
					    bool closest = Vector2.Distance(Projectile.Center, targetPos) > distance;
                        bool inRange = distance < 40 && (difference.SafeNormalize(Vector2.Zero)*Projectile.velocity.SafeNormalize(Vector2.Zero)).Length()>0.1f;
					    if ((!foundTarget || closest) && inRange) {
						    targetPos = testPos;
						    foundTarget = true;
					    }
				    }
			    }
                skip:
                if(foundTarget) {
                    Projectile.velocity = (targetPos - Projectile.Center).SafeNormalize(Vector2.UnitX)*Projectile.velocity.Length();
                    Projectile.localAI[1] = 10;
                } else {
                    Projectile.velocity = Projectile.velocity.RotatedByRandom(MathHelper.Min((3600f - Projectile.timeLeft) * 0.025f, MathHelper.PiOver2));
                }
            }
            return true;
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            if(Projectile.localAI[1]>0)Projectile.localAI[0] = 20;
        }
        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
            width = 27;
            height = 27;
            return true;
        }
    }
}
