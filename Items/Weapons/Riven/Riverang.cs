using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Default;

namespace Origins.Items.Weapons.Riven {
    public class Riverang : ModItem {
        static short glowmask;
        public override void SetStaticDefaults() {
			DisplayName.SetDefault(AprilFools.CheckAprilFools() ? "Lobsterang" : "Riverang");
			Tooltip.SetDefault("Very hydrodynamic");
            glowmask = Origins.AddGlowMask(this);
            SacrificeTotal = 1;
        }
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.ThornChakram);
            Item.DamageType = DamageClass.MeleeNoSpeed;
			Item.damage = 23;
			Item.width = 20;
			Item.height = 22;
			Item.useTime = 13;
			Item.useAnimation = 13;
			//item.knockBack = 5;
            Item.shoot = ModContent.ProjectileType<Riverang_P>();
            Item.shootSpeed = 10.75f;
            Item.knockBack = 5f;
            Item.value = Item.buyPrice(silver: 40);
            Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1;
            Item.glowMask = glowmask;
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ModContent.ItemType<Encrusted_Bar>(), 7);
            recipe.AddIngredient(ModContent.ItemType<Riven_Sample>(), 4);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
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
			Projectile.width = 22;
			Projectile.height = 22;
            //projectile.scale*=1.25f;
            Projectile.ignoreWater = true;
            AIType = ProjectileID.FruitcakeChakram;
        }
        public override bool PreAI() {
            Projectile.aiStyle = 3;
            return true;
        }
		public override void PostAI() {
            bool wet = false;
            if (Collision.WetCollision(Projectile.position, Projectile.width, Projectile.height) && !Collision.honey) {
                wet = true;
            }
            Vector2 targetPos = Projectile.Center;
            bool foundTarget = false;
            Vector2 testPos;
            float targetDist = wet ? 80 : 40;
            if (Projectile.localAI[1] > 0) {
                Projectile.localAI[1]--;
                if (!wet) goto skip;
            }
            if (Projectile.localAI[0] > 0) {
                Projectile.localAI[0]--;
                goto skip;
            }
            for (int i = 0; i < Main.maxNPCs; i++) {
                NPC target = Main.npc[i];
                if (target.CanBeChasedBy()) {
                    testPos = Projectile.Center.Clamp(target.Hitbox);
                    Vector2 difference = testPos - Projectile.Center;
                    float distance = difference.Length();
                    bool closest = Vector2.Distance(Projectile.Center, targetPos) > distance;
                    bool inRange = distance < targetDist && Vector2.Dot(difference.SafeNormalize(Vector2.Zero), Projectile.velocity.SafeNormalize(Vector2.Zero)) > 0.2f;
                    if ((!foundTarget || closest) && inRange) {
                        targetPos = testPos;
                        foundTarget = true;
                        targetDist = distance;
                    }
                }
            }
            skip:
            if (foundTarget) {
                Projectile.velocity = (targetPos - Projectile.Center).SafeNormalize(Vector2.UnitX) * Projectile.velocity.Length();
                Projectile.localAI[1] = 10;
            }
        }
		public override bool? CanHitNPC(NPC target) {
            if (Collision.WetCollision(Projectile.position, Projectile.width, Projectile.height) && !Collision.honey) {
                Projectile.aiStyle = 0;
            }
            return null;
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            if(Projectile.localAI[1]>0)Projectile.localAI[0] = 20;
        }
        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
            width = 27;
            height = 27;
            return true;
        }
		public override bool OnTileCollide(Vector2 oldVelocity) {
            if (Collision.WetCollision(Projectile.position, Projectile.width, Projectile.height) && !Collision.honey) {
				if (Projectile.velocity.X != oldVelocity.X) {
                    Projectile.velocity.X = -oldVelocity.X;
                }
                if (Projectile.velocity.Y != oldVelocity.Y) {
                    Projectile.velocity.Y = -oldVelocity.Y;
                }
                Vector2 targetPos = Projectile.Center;
                bool foundTarget = false;
                Vector2 testPos;
                float targetDist = 120;
                for (int i = 0; i < Main.maxNPCs; i++) {
                    NPC target = Main.npc[i];
                    if (target.CanBeChasedBy()) {
                        testPos = Projectile.Center.Clamp(target.Hitbox);
                        Vector2 difference = testPos - Projectile.Center;
                        float distance = difference.Length();
                        bool closest = Vector2.Distance(Projectile.Center, targetPos) > distance;
                        bool inRange = distance < targetDist && Vector2.Dot(difference.SafeNormalize(Vector2.Zero), Projectile.velocity.SafeNormalize(Vector2.Zero)) > -0.1f;
                        if ((!foundTarget || closest) && inRange) {
                            targetPos = testPos;
                            foundTarget = true;
                            targetDist = distance;
                        }
                    }
                }
                if (foundTarget) {
                    Projectile.velocity = (targetPos - Projectile.Center).SafeNormalize(Vector2.UnitX) * Projectile.velocity.Length();
                    Projectile.localAI[1] = 10;
                }
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Dig);
				return false;
            }
            return true;
		}
	}
}
