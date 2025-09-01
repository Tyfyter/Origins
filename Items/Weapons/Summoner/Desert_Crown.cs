using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Weapons.Summoner;
using Origins.Items.Weapons.Summoner.Minions;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Summoner {
	public class Desert_Crown : ModItem, ICustomWikiStat {
		static short glowmask;
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.damage = 6;
			Item.DamageType = DamageClass.Summon;
			Item.knockBack = 4;
			Item.mana = 10;
			Item.width = 32;
			Item.height = 32;
			Item.useTime = 36;
			Item.useAnimation = 36;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.value = Item.sellPrice(silver: 50);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item44;
			Item.buffType = Desert_Crown_Buff.ID;
			Item.shoot = Sand_Elemental.ID;
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
	public class Desert_Crown_Buff : MinionBuff {
		public static int ID { get; private set; }
		public override IEnumerable<int> ProjectileTypes() => [
			Sand_Elemental.ID
		];
	}
}

namespace Origins.Items.Weapons.Summoner.Minions {
	public class Sand_Elemental : ModProjectile {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Projectile.type] = 5;
			// This is necessary for right-click targeting
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;

			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[Projectile.type] = true;
			// This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
			ID = Type;
		}

		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Summon;
			Projectile.width = 54;
			Projectile.height = 58;
			Projectile.tileCollide = true;
			Projectile.friendly = false;
			Projectile.minion = true;
			Projectile.minionSlots = 1f;
			Projectile.penetrate = -1;
			Projectile.netImportant = true;
		}

		// Here you can decide if your minion breaks things like grass or pots
		public override bool? CanCutTiles() {
			return false;
		}

		public override void AI() {
			const float attack_time = 30;
			Player player = Main.player[Projectile.owner];

			#region Active check
			// This is the "active check", makes sure the minion is alive while the player is alive, and despawns if not
			if (player.dead || !player.active) {
				player.ClearBuff(Desert_Crown_Buff.ID);
			}
			if (player.HasBuff(Desert_Crown_Buff.ID)) {
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
			float distanceFromTarget = 25f * 16f;
			Vector2 targetCenter = default;
			int target = -1;
			void targetingAlgorithm(NPC npc, float targetPriorityMultiplier, bool isPriorityTarget, ref bool foundTarget) {
				if (!isPriorityTarget && distanceFromTarget > 15f * 16f) {
					distanceFromTarget = 15f * 16f;
				}
				if (npc.CanBeChasedBy()) {
					float between = Vector2.Distance(npc.Center, player.Center);
					bool closest = Vector2.Distance(player.Center, targetCenter) > between;
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
			#endregion

			Vector2 direction = targetCenter - Projectile.Center;
			float projDistanceFromTarget = direction.Length();
			direction /= projDistanceFromTarget;

			if (foundTarget) {
				if (++Projectile.ai[0] >= attack_time) {
					Projectile.ai[0] -= attack_time;
					Projectile.NewProjectile(
						Projectile.GetSource_FromThis(),
						Projectile.Center,
						direction * 8,
						ModContent.ProjectileType<Sand_Elemental_Sand>(),
						Projectile.damage,
						Projectile.knockBack,
						Projectile.owner
					);
				}
			} else if (Projectile.ai[0] > 0) {
				Projectile.ai[0] -= 0.25f;
			}

			#region Movement

			// Default movement parameters (here for attacking)
			float speed = 8f;
			float inertia = 12f;
			vectorToIdlePosition.Normalize();
			if (foundTarget) {
				float projDistanceFromPlayer = (Projectile.Center - player.Center).Length();
				Projectile.tileCollide = true;
				Vector2 realDirection = direction;
				Projectile.spriteDirection = Math.Sign(direction.X);
				if (projDistanceFromPlayer > 10f * 16f) {
					realDirection = vectorToIdlePosition;
					speed = 2f;
					inertia = 24f;
				} else if (projDistanceFromTarget > 72f) {
					speed = 6f;
					inertia = 32f;
				} else if (projDistanceFromTarget > 48f) {
					speed = 0f;
					inertia = 32f;
				} else {
					speed = -6f;
					inertia = 18f;
				}
				// Minion has a target: attack (here, fly towards the enemy)
				Projectile.velocity = (Projectile.velocity * (inertia - 1) + realDirection * speed) / inertia;
			} else {
				Projectile.tileCollide = false;
				Projectile.spriteDirection = Math.Sign(vectorToIdlePosition.X);
				if (distanceToIdlePosition > 600f) {
					speed = 16f;
					inertia = 36f;
				} else if (distanceToIdlePosition < 16) {
					speed = 0f;
					inertia = 34f;
					Projectile.spriteDirection = player.direction;
				}
				
				Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToIdlePosition * speed) / inertia;
				if (Projectile.velocity.LengthSquared() < 0.01f) {
					// If there is a case where it's not moving at all, give it a little "poke"
					Projectile.velocity += Main.rand.NextVector2Circular(1, 1) * 0.05f;
				}
			}
			#endregion

			#region Animation and visuals
			// So it will lean slightly towards the direction it's moving
			Projectile.rotation = Projectile.velocity.X * 0.05f;

			// This is a simple "loop through all frames from top to bottom" animation
			int frameSpeed = 5;
			if (++Projectile.frameCounter >= frameSpeed) {
				Projectile.frameCounter = 0;
				if (++Projectile.frame >= Main.projFrames[Projectile.type]) Projectile.frame = 0;
			}

			// Some visuals here
			Lighting.AddLight(Projectile.Center, Color.SandyBrown.ToVector3() * 0.18f);
			#endregion
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			width -= 2;
			height -= 8;
			return true;
		}
	}
	public class Sand_Elemental_Sand : ModProjectile {
		public override string Texture => "Origins/Projectiles/Pixel";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.PurificationPowder);
			Projectile.DamageType = DamageClass.Summon;
			Projectile.aiStyle = 0;
			Projectile.width = 38;
			Projectile.height = 38;
			Projectile.timeLeft = 30;
			Projectile.extraUpdates = 0;
			Projectile.penetrate = 1;
			Projectile.friendly = true;
		}
		public override void AI() {
			Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.SandstormInABottle, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f).velocity *= 0.5f;
		}
	}
}
