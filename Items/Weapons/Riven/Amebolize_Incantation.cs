using Microsoft.Xna.Framework;
using Origins.Buffs;
using Origins.Projectiles;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;
using static Origins.OriginExtensions;

namespace Origins.Items.Weapons.Riven {
    public class Amebolize_Incantation : ModItem {
        static short glowmask;
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Amebolize Incantation");
			Tooltip.SetDefault("Struck enemies will wither away\n5 summon tag damage\n{$CommonItemTooltip.Whips}");
            glowmask = Origins.AddGlowMask(this);
            SacrificeTotal = 1;
        }
		public override void SetDefaults() {
			Item.damage = 20;
            Item.DamageType = DamageClass.Summon;
            Item.width = 20;
			Item.height = 22;
            Item.useStyle = ItemUseStyleID.Shoot;
			Item.useTime = 18;
			Item.useAnimation = 18;
            Item.shoot = ModContent.ProjectileType<Amebolize_Incantation_P>();
            Item.shootSpeed = 9.75f;
            Item.mana = 10;
            Item.knockBack = 0f;
			Item.value = 5000;
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item7;
            Item.glowMask = glowmask;
            Item.channel = true;
        }
    }
    public class Amebolize_Incantation_P : ModProjectile {
        public override string Texture => "Origins/Items/Weapons/Riven/Minions/Amoeba_Bubble";
        public const int frameSpeed = 5;
        public override string GlowTexture => Texture;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Amebolize Incantation");
            Main.projFrames[Projectile.type] = 4;
        }
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.Grenade);
            Projectile.DamageType = DamageClass.Summon;
            Projectile.aiStyle = 0;
            Projectile.penetrate = -1;
			Projectile.width = 22;
			Projectile.height = 22;
            Projectile.ignoreWater = true;
            Projectile.friendly = true;
            Projectile.timeLeft = 90;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void AI() {
            Player player = Main.player[Projectile.owner];

            #region Find target
            // Starting search distance
            float targetDist = 640f;
            float targetAngle = 0;
            Vector2 targetCenter = Projectile.Center;
            int target = -1;
            void targetingAlgorithm(NPC npc, float targetPriorityMultiplier, bool isPriorityTarget, ref bool foundTarget) {
                if (isPriorityTarget) return;
                if (npc.CanBeChasedBy() && Projectile.localNPCImmunity[npc.whoAmI] == 0) {
                    Vector2 diff = npc.Center - Projectile.Center;
                    float dist = diff.Length();
                    if (dist > targetDist) return;
                    float dot = NormDotWithPriorityMult(diff, Projectile.velocity, targetPriorityMultiplier) - (player.DistanceSQ(npc.Center) / (640 * 640));
                    bool lineOfSight = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height);
                    if (dot >= targetAngle && lineOfSight) {
                        targetDist = dist;
                        targetAngle = dot;
                        targetCenter = npc.Center;
                        target = npc.whoAmI;
                        foundTarget = true;
                    }
                }
            }
            bool foundTarget = player.channel;
			if (player.channel && Projectile.ai[0] == 0) {
                if(Main.myPlayer == Projectile.owner) targetCenter = Main.MouseWorld;
                Projectile.timeLeft = 90;
                Projectile.localNPCHitCooldown = 30;
            } else {
                foundTarget = player.GetModPlayer<OriginPlayer>().GetMinionTarget(targetingAlgorithm);
                Projectile.ai[0] = 1;
                Projectile.localNPCHitCooldown = -1;
            }
            #endregion

            #region Movement
            // Default movement parameters (here for attacking)
            float currentSpeed = Projectile.velocity.Length();
            float speed = 18f;
            float turnSpeed = 12f;
            if (foundTarget) {
                if ((int)Math.Ceiling(targetAngle) == -1) {
                    targetCenter.Y -= 16;
                }
            }
            LinearSmoothing(ref currentSpeed, speed, 0.5f - (currentSpeed / 50f));
			if (foundTarget) { 
                Vector2 direction = targetCenter - Projectile.Center;
                float distance = direction.Length();
                direction /= distance;
                Projectile.velocity = (Vector2.Normalize(Projectile.velocity + direction * turnSpeed) * currentSpeed).WithMaxLength(distance);
            }
            #endregion

            #region Animation and visuals

            if(Projectile.velocity != Vector2.Zero)Projectile.rotation = (float)Math.Atan(Projectile.velocity.Y / Projectile.velocity.X);
            Projectile.spriteDirection = Projectile.velocity.X < 0 ? -1 : 1;

            // This is a simple "loop through all frames from top to bottom" animation
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= frameSpeed) {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= Main.projFrames[Projectile.type]) {
                    Projectile.frame = 0;
                }
            }
            #endregion
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            target.AddBuff(Amebolize_Buff.ID, 240);
            if (target.life > 0 && target.CanBeChasedBy()) Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
            if(Projectile.ai[0] != 0) Projectile.damage = (int)(Projectile.damage * 0.90);
        }

        public override bool OnTileCollide(Vector2 oldVelocity) {
            //Projectile.Kill();
            return false;
        }
		public override Color? GetAlpha(Color lightColor) {
            return new Color((lightColor.R + 255) / 510f, (lightColor.G + 255) / 510f, (lightColor.B + 255) / 510f, 0.5f);
		}
	}
}
