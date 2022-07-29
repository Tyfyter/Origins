using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Riven {
    public class Riverang : ModItem {
        static short glowmask;
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Riverang");
			Tooltip.SetDefault("Very hydrodynamic");
            glowmask = Origins.AddGlowMask(this);
            SacrificeTotal = 1;
        }
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.ThornChakram);
            Item.DamageType = DamageClass.MeleeNoSpeed;
			Item.damage = 14;
			Item.width = 20;
			Item.height = 22;
			Item.useTime = 18;
			Item.useAnimation = 18;
			//item.knockBack = 5;
            Item.shoot = ModContent.ProjectileType<Riverang_P>();
            Item.shootSpeed = 10.75f;
            Item.knockBack = 5f;
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
        int lastHitNPC = -1;
        public override string Texture => "Origins/Items/Weapons/Riven/Riverang";
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Riverang");
		}
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.ThornChakram);
            Projectile.penetrate = -1;
			Projectile.width = 22;
			Projectile.height = 22;
            //projectile.scale*=1.25f;
            Projectile.ignoreWater = true;
        }
        public override bool PreAI() {
            Projectile.aiStyle = 3;
            bool wet = false;
            if (Collision.WetCollision(Projectile.position, Projectile.width, Projectile.height) && !Collision.honey) {
                 wet = true;
            }
			Vector2 targetPos = Projectile.Center;
			bool foundTarget = false;
            Vector2 testPos;
            float targetDist = wet ? 80 : 40;
            if (Projectile.localAI[1]>0) {
                Projectile.localAI[1]--;
                if (!wet) goto skip;
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
                    bool inRange = distance < targetDist && Vector2.Dot(difference.SafeNormalize(Vector2.Zero), Projectile.velocity.SafeNormalize(Vector2.Zero))>0.2f;
					if ((!foundTarget || closest) && inRange) {
						targetPos = testPos;
						foundTarget = true;
                        targetDist = distance;
					}
				}
			}
            skip:
            if(foundTarget) {
                Projectile.velocity = (targetPos - Projectile.Center).SafeNormalize(Vector2.UnitX)*Projectile.velocity.Length();
                Projectile.localAI[1] = 10;
            }
            return true;
        }
        public override bool? CanHitNPC(NPC target) {
            if (Collision.WetCollision(Projectile.position, Projectile.width, Projectile.height) && !Collision.honey) {
                Projectile.aiStyle = 0;
            }
            return null;
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            if(Projectile.localAI[1]>0)Projectile.localAI[0] = 20;
            lastHitNPC = target.whoAmI;
        }
        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
            width = 27;
            height = 27;
            return true;
        }
    }
}
