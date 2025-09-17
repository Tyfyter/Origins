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
	public class Brainy_Staff : ModItem, ICustomWikiStat {
		internal static int projectileID = 0;
		public override void SetStaticDefaults() {
			ItemID.Sets.StaffMinionSlotsRequired[Type] = 1;
			PegasusLib.Sets.ItemSets.InflictsExtraDebuffs[Type] = [BuffID.Confused];
		}
		public override void SetDefaults() {
			Item.damage = 10;
			Item.DamageType = DamageClass.Summon;
			Item.mana = 10;
			Item.width = 32;
			Item.height = 32;
			Item.useTime = 36;
			Item.useAnimation = 36;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.value = Item.sellPrice(gold: 1, silver: 50);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item44;
			Item.buffType = Brainy_Buff.ID;
			Item.shoot = projectileID;
			Item.noMelee = true;
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			position = Main.MouseWorld;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			player.AddBuff(Brainy_Buff.ID, 2);
			player.SpawnMinionOnCursor(source, player.whoAmI, type, Item.damage, knockback);
			return false;
		}
	}
}
namespace Origins.Buffs {
	public class Brainy_Buff : MinionBuff, ICustomWikiStat {
		public static int ID { get; private set; }
		public override IEnumerable<int> ProjectileTypes() => [
			Brainy_Staff.projectileID
		];
		public string CustomStatPath => nameof(Brainy_Buff);
	}
}

namespace Origins.Items.Weapons.Summoner.Minions {
	public class Mini_BOC : ModProjectile {
		public const int frameSpeed = 4;
		public override void SetStaticDefaults() {
			Brainy_Staff.projectileID = Projectile.type;
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
			Projectile.width = 34;
			Projectile.height = 26;
			Projectile.tileCollide = true;
			Projectile.friendly = true;
			Projectile.minion = true;
			Projectile.minionSlots = 1f;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 14;
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
				player.ClearBuff(Brainy_Buff.ID);
			}
			if (player.HasBuff(Brainy_Buff.ID)) {
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
			Vector2 targetCenter = Projectile.position;
			int target = -1;
			void targetingAlgorithm(NPC npc, float targetPriorityMultiplier, bool isPriorityTarget, ref bool foundTarget) {
				if (!isPriorityTarget && distanceFromTarget > 700f) {
					distanceFromTarget = 700f;
				}
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
						targetCenter = npc.height / (float)npc.width > 1 ? npc.Top + new Vector2(0, 8) : npc.Center;
						target = npc.whoAmI;
						foundTarget = true;
					}
				}
			}
			bool foundTarget = player.GetModPlayer<OriginPlayer>().GetMinionTarget(targetingAlgorithm);

			Projectile.friendly = foundTarget;
			#endregion

			#region Movement

			// Default movement parameters (here for attacking)
			float speed = 12f;
			float inertia = 16f;

			if (foundTarget) {
				Projectile.tileCollide = true;
				// Minion has a target: attack (here, fly towards the enemy)
				//if (distanceFromTarget > 40f || !projectile.Hitbox.Intersects(Main.npc[target].Hitbox)) {
				// The immediate range around the target (so it doesn't latch onto it when close)
				Vector2 direction = targetCenter - Projectile.Center;
				direction.Normalize();
				direction *= speed;
				Projectile.velocity = (Projectile.velocity * (inertia - 1) + direction) / inertia;
				//}
			} else {
				Projectile.tileCollide = false;
				if (distanceToIdlePosition > 600f) {
					speed = 24f;
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
			Projectile.spriteDirection = Projectile.direction;
			#endregion
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			fallThrough = true;
			return true;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (Main.rand.Next(10) < 3 && (target.Center - Main.player[Projectile.owner].Center).Length() < 480) {
				target.AddBuff(BuffID.Confused, 60);
			}
		}
	}
}
