using Microsoft.Xna.Framework;
using Origins.Buffs;
using Origins.Items.Weapons.Summoner;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
using System.Collections.Generic;
using Origins.Projectiles;
namespace Origins.Items.Weapons.Summoner {
	public class Woodsprite_Staff : ModItem, ICustomWikiStat {
		static short glowmask;
		internal static int projectileID = 0;
		internal static int buffID = 0;
        public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
		}
		public override void SetDefaults() {
			Item.damage = 3;
			Item.DamageType = DamageClass.Summon;
			Item.mana = 12;
			Item.width = 32;
			Item.height = 32;
			Item.useTime = 36;
			Item.useAnimation = 36;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.value = Item.sellPrice(silver: 50);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item44;
			buffID = ModContent.BuffType<Woodsprite_Buff>();
			Item.buffType = buffID;
			Item.shoot = projectileID;
			Item.noMelee = true;
			Item.glowMask = glowmask;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			player.AddBuff(Item.buffType, 2);
			player.SpawnMinionOnCursor(source, player.whoAmI, type, Item.damage, knockback);
			return false;
		}
	}
}
namespace Origins.Buffs {
	public class Woodsprite_Buff : MinionBuff {
		public override IEnumerable<int> ProjectileTypes() => [
			Woodsprite_Staff.projectileID
		];
	}
}

namespace Origins.Items.Weapons.Summoner.Minions {
	public class Woodsprite : ModProjectile {
		public override void SetStaticDefaults() {
			Woodsprite_Staff.projectileID = Projectile.type;
			// DisplayName.SetDefault("Woodsprite");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Projectile.type] = 4;
			// This is necessary for right-click targeting
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;

			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[Projectile.type] = true;
			// This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
		}

		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Summon;
			Projectile.width = 18;
			Projectile.height = 28;
			Projectile.tileCollide = true;
			Projectile.friendly = true;
			Projectile.minion = true;
			Projectile.minionSlots = 1f;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 18;
			Projectile.netImportant = true;
		}

		// Here you can decide if your minion breaks things like grass or pots
		public override bool? CanCutTiles() {
			return false;
		}

		// This is mandatory if your minion deals contact damage (further related stuff in AI() in the Movement region)
		public override bool MinionContactDamage() {
			return true;
		}

		public override void AI() {
			Player player = Main.player[Projectile.owner];

			#region Active check
			// This is the "active check", makes sure the minion is alive while the player is alive, and despawns if not
			if (player.dead || !player.active) {
				player.ClearBuff(Woodsprite_Staff.buffID);
			}
			if (player.HasBuff(Woodsprite_Staff.buffID)) {
				Projectile.timeLeft = 2;
			}
			#endregion

			#region General behavior
			Vector2 idlePosition = player.Top;
			idlePosition.X -= 48f * player.direction;

			// Teleport to player if distance is too big
			Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
			float distanceToIdlePosition = vectorToIdlePosition.Length();
			if (Main.myPlayer == player.whoAmI && distanceToIdlePosition > 2000f) {
				// Whenever you deal with non-regular events that change the behavior or position drastically, make sure to only run the code on the owner of the projectile,
				// and then set netUpdate to true
				Projectile.position = idlePosition;
				Projectile.velocity *= 0.1f;
				Projectile.netUpdate = true;
			}

			// If your minion is flying, you want to do this independently of any conditions
			float overlapVelocity = 0.04f;
			for (int i = 0; i < Main.maxProjectiles; i++) {
				// Fix overlap with other minions
				Projectile other = Main.projectile[i];
				if (i != Projectile.whoAmI && other.active && other.owner == Projectile.owner && Math.Abs(Projectile.position.X - other.position.X) + Math.Abs(Projectile.position.Y - other.position.Y) < Projectile.width) {
					if (Projectile.position.X < other.position.X) Projectile.velocity.X -= overlapVelocity;
					else Projectile.velocity.X += overlapVelocity;

					if (Projectile.position.Y < other.position.Y) Projectile.velocity.Y -= overlapVelocity;
					else Projectile.velocity.Y += overlapVelocity;
				}
			}
			#endregion

			#region Find target
			// Starting search distance
			float distanceFromTarget = 2000f;
			Vector2 targetCenter = default;
			int target = -1;
			void targetingAlgorithm(NPC npc, float targetPriorityMultiplier, bool isPriorityTarget, ref bool foundTarget) {
				if (!isPriorityTarget && distanceFromTarget > 700f) {
					distanceFromTarget = 700f;
				}
				if (npc.CanBeChasedBy()) {
					float between = Vector2.Distance(npc.Center, Projectile.Center);
					bool closest = Vector2.Distance(Projectile.Center, targetCenter) > between;
					bool inRange = between < distanceFromTarget;
					bool lineOfSight = isPriorityTarget || Collision.CanHitLine(Projectile.position + new Vector2(1, 4), Projectile.width - 2, Projectile.height - 8, npc.position, npc.width, npc.height);
					// Additional check for this specific minion behavior, otherwise it will stop attacking once it dashed through an enemy while flying though tiles afterwards
					// The number depends on various parameters seen in the movement code below. Test different ones out until it works alright
					bool closeThroughWall = between < 100f;
					if (((closest && inRange) || !foundTarget) && (lineOfSight || closeThroughWall)) {
						distanceFromTarget = between;
						targetCenter = npc.Center;
						target = npc.whoAmI;
						foundTarget = true;
					}
				}
			}
			bool foundTarget = player.GetModPlayer<OriginPlayer>().GetMinionTarget(targetingAlgorithm);
			#region old targeting code
			/*if (player.HasMinionAttackTargetNPC) {
				NPC npc = Main.npc[player.MinionAttackTargetNPC];
				float between = Vector2.Distance(npc.Center, Projectile.Center);
				if (between < 2000f) {
					distanceFromTarget = between;
					targetCenter = npc.Center;
                    target = player.MinionAttackTargetNPC;
					foundTarget = true;
				}
			}
			if (!foundTarget) {
				for (int i = 0; i < Main.maxNPCs; i++) {
					NPC npc = Main.npc[i];
					if (npc.CanBeChasedBy()) {
						float between = Vector2.Distance(npc.Center, Projectile.Center);
						bool closest = Vector2.Distance(Projectile.Center, targetCenter) > between;
						bool inRange = between < distanceFromTarget;
						bool lineOfSight = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height);
						// Additional check for this specific minion behavior, otherwise it will stop attacking once it dashed through an enemy while flying though tiles afterwards
						// The number depends on various parameters seen in the movement code below. Test different ones out until it works alright
						bool closeThroughWall = between < 100f;
						if (((closest && inRange) || !foundTarget) && (lineOfSight || closeThroughWall)) {
							distanceFromTarget = between;
							targetCenter = npc.Center;
                            target = npc.whoAmI;
							foundTarget = true;
						}
					}
				}
			}*/
			#endregion

			Projectile.friendly = foundTarget;
			#endregion

			#region Movement

			// Default movement parameters (here for attacking)
			float speed = 8f;
			float inertia = 12f;

			if (foundTarget) {
				Projectile.tileCollide = true;
				// Minion has a target: attack (here, fly towards the enemy)
				if (distanceFromTarget > 40f || !Projectile.Hitbox.Intersects(Main.npc[target].Hitbox)) {
					// The immediate range around the target (so it doesn't latch onto it when close)
					Vector2 direction = targetCenter - Projectile.Center;
					direction.Normalize();
					direction *= speed;
					Projectile.velocity = (Projectile.velocity * (inertia - 1) + direction) / inertia;
				}
			} else {
				Projectile.tileCollide = false;
				if (distanceToIdlePosition > 600f) {
					speed = 16f;
					inertia = 36f;
				} else {
					speed = 6f;
					inertia = 48f;
				}
				if (distanceToIdlePosition > 12f) {
					// The immediate range around the player (when it passively floats about)

					// This is a simple movement formula using the two parameters and its desired direction to create a "homing" movement
					vectorToIdlePosition.Normalize();
					vectorToIdlePosition *= speed;
					Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToIdlePosition) / inertia;
				} else if (Projectile.velocity == Vector2.Zero) {
					// If there is a case where it's not moving at all, give it a little "poke"
					Projectile.velocity.X = -0.15f;
					Projectile.velocity.Y = -0.05f;
				}
			}
			#endregion

			#region Animation and visuals
			// So it will lean slightly towards the direction it's moving
			Projectile.rotation = Projectile.velocity.X * 0.05f;

			// This is a simple "loop through all frames from top to bottom" animation
			int frameSpeed = 5;
			Projectile.frameCounter++;
			if (Projectile.frameCounter >= frameSpeed) {
				Projectile.frameCounter = 0;
				Projectile.frame++;
				if (Projectile.frame >= Main.projFrames[Projectile.type]) {
					Projectile.frame = 0;
				}
			}

			// Some visuals here
			Lighting.AddLight(Projectile.Center, Color.LawnGreen.ToVector3() * 0.18f);
			#endregion
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			int healing = Main.rand.RandomRound(1 + ((damageDone-3) / 5f));
			if (healing > 0) Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<Woodsprite_Lifesteal>(), healing, 0, Projectile.owner);
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			width -= 2;
			height -= 8;
			fallThrough = true;
			return true;
		}
	}
	public class Woodsprite_Lifesteal : ModProjectile {
		public override string Texture => "Origins/Projectiles/Pixel";
		public override void SetDefaults() {
			Projectile.timeLeft = 300;
			Projectile.friendly = false;
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			if (player.dead || !player.active) {
				Projectile.Kill();
				return;
			}
			if (player.statLife >= player.statLifeMax2 && Projectile.ai[0] == 0) {
				float lowestHealth = 1;
				foreach (Projectile otherProj in Main.ActiveProjectiles) {
					if (otherProj.owner == Projectile.owner && otherProj.ModProjectile is IArtifactMinion artifactMinion) {
						float healthPercent = artifactMinion.Life / artifactMinion.MaxLife;
						if (healthPercent < lowestHealth) {
							lowestHealth = healthPercent;
							Projectile.ai[0] = otherProj.identity + 1;
						}
					}
				}
			}
			Vector2 unit;
			if (Projectile.ai[0] != 0) {
				IArtifactMinion targetProj = null;
				Rectangle targetProjHitbox = default;
				foreach (Projectile otherProj in Main.ActiveProjectiles) {
					if (otherProj.owner == Projectile.owner && otherProj.ModProjectile is IArtifactMinion artifactMinion && otherProj.identity + 1 == Projectile.ai[0]) {
						targetProj = artifactMinion;
						targetProjHitbox = otherProj.Hitbox;
						break;
					}
				}
				if (targetProj is null) {
					Projectile.ai[0] = 0;
					return;
				}
				if (targetProjHitbox.Contains(Projectile.Center.ToPoint())) {
					float oldHealth = targetProj.Life;
					targetProj.Life += Projectile.damage;
					if (targetProj.Life > targetProj.MaxLife) targetProj.Life = targetProj.MaxLife;
					CombatText.NewText(targetProjHitbox, CombatText.HealLife, (int)Math.Round(targetProj.Life - oldHealth), true, dot: true);
					Projectile.Kill();
					return;
				}
				unit = (targetProjHitbox.Center() - Projectile.Center).SafeNormalize(Projectile.velocity);
				Projectile.velocity = Vector2.Lerp(Projectile.velocity, unit * 8, 0.1f);
				Dust.NewDustPerfect(Projectile.Center, 110, Vector2.Zero);
				return;
			}
			if (player.Hitbox.Contains(Projectile.Center.ToPoint())) {
				player.Heal(Projectile.damage);
				Projectile.Kill();
				return;
			}
			unit = (player.Center - Projectile.Center).SafeNormalize(Projectile.velocity);
			Projectile.velocity = Vector2.Lerp(Projectile.velocity, unit * 8, 0.1f);
			Dust.NewDustPerfect(Projectile.Center, 110, Vector2.Zero);
		}
	}
}
