using Microsoft.Xna.Framework;
using Origins.Buffs;
using Origins.Items.Accessories;
using Origins.Items.Weapons.Summoner;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Lazy_Cloak : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Lazy Cloak");
			Tooltip.SetDefault("Chases after marked enemies\n'It just doesn't want to do all the work'");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(32, 36);
			Item.damage = 30;
			Item.DamageType = DamageClass.Summon;
			Item.useTime = 36;
			Item.useAnimation = 36;
			Item.shoot = ModContent.ProjectileType<Lazy_Cloak_P>();
			Item.value = Item.sellPrice(gold: 6);
			Item.rare = ItemRarityID.Master;
			Item.backSlot = 5;
			Item.frontSlot = 3;
			Item.canBePlacedInVanityRegardlessOfConditions = true;
			Item.master = true;
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			Item.backSlot = -1;
			Item.frontSlot = -1;
			if (!hideVisual) {
				player.GetModPlayer<OriginPlayer>().lazyCloakVisible = true;
			}
			if (player.ownedProjectileCounts[Item.shoot] < 1) {
				player.SpawnMinionOnCursor(player.GetSource_Accessory(Item), player.whoAmI, Item.shoot, Item.damage, Item.knockBack, player.MountedCenter - Main.MouseWorld);
			}
			player.AddBuff(Lazy_Cloak_Buff.ID, 5);
		}
		public override void UpdateVanity(Player player) {
			Item.backSlot = 5;
			Item.frontSlot = 3;
		}
	}
	public class Lazy_Cloak_P : ModProjectile {
		public const int frameSpeed = 5;
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			Eyeball_Staff.projectileID = Projectile.type;
			DisplayName.SetDefault("Lazy Cloak");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Projectile.type] = 2;

			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[Projectile.type] = true;
			// This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
			ID = Type;
		}

		public sealed override void SetDefaults() {
			Projectile.DamageType = DamageClass.Summon;
			Projectile.width = 40;
			Projectile.height = 28;
			Projectile.tileCollide = true;
			Projectile.friendly = true;
			Projectile.minion = true;
			Projectile.minionSlots = 0f;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 12;
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
				player.ClearBuff(Lazy_Cloak_Buff.ID);
			}
			if (player.HasBuff(Lazy_Cloak_Buff.ID)) {
				Projectile.timeLeft = 2;
			}
			#endregion

			#region General behavior
			Vector2 idlePosition = player.MountedCenter;

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
			float distanceFromTarget = 2000f * 2000f;
			Vector2 targetCenter = Projectile.position;
			int target = -1;
			void targetingAlgorithm(NPC npc, float targetPriorityMultiplier, bool isPriorityTarget, ref bool foundTarget) {
				if (!isPriorityTarget) return;
				float between = Vector2.DistanceSquared(npc.Center, Projectile.Center);
				if (between < distanceFromTarget) {
					distanceFromTarget = between;
					targetCenter = npc.Center;
					target = player.MinionAttackTargetNPC;
					foundTarget = true;
				}
			}
			bool foundTarget = player.GetModPlayer<OriginPlayer>().GetMinionTarget(targetingAlgorithm);
			/*if (!foundTarget) {
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
							targetCenter = npc.height / (float)npc.width > 1 ? npc.Top + new Vector2(0, 8) : npc.Center;
							target = npc.whoAmI;
							foundTarget = true;
						}
					}
				}
			}*/

			Projectile.friendly = foundTarget;
			#endregion

			#region Movement

			// Default movement parameters (here for attacking)
			float speed = 12f;
			float inertia = 16f;

			if (foundTarget) {
				Projectile.hide = false;
				Projectile.ai[0] = 1;
				//Projectile.tileCollide = true;
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
					inertia = 12f;
				} else {
					speed = 12f;
					inertia = 12f;
				}
				if (distanceToIdlePosition > 12f) {
					// The immediate range around the player (when it passively floats about)

					// This is a simple movement formula using the two parameters and its desired direction to create a "homing" movement
					vectorToIdlePosition.Normalize();
					vectorToIdlePosition *= speed;
					Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToIdlePosition) / inertia;
				} else {
					Projectile.ai[0] = 0;
				}
				if (Projectile.ai[0] == 0) {
					Projectile.hide = true;
					Projectile.position = idlePosition;
					if (player.GetModPlayer<OriginPlayer>().lazyCloakVisible) {
						player.back = 5;
						player.front = 3;
					}
				}
			}
			#endregion

			#region Animation and visuals
			// So it will lean slightly towards the direction it's moving
			Projectile.rotation = Projectile.velocity.X * 0.05f;
			Projectile.spriteDirection = Math.Sign(Projectile.velocity.X);

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
			#endregion
		}
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			target.AddBuff(BuffID.Ichor, 60);
			if (crit && target.life < damage * 3) {
				target.life = 0;
				//target.checkDead();
			}
		}
	}
}
namespace Origins.Buffs {
	public class Lazy_Cloak_Buff : ModBuff {
		public override string Texture => "Origins/Buffs/Lazy_Cloak_Buff";
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Lazy Cloak");
			Description.SetDefault("Your cloak will fight for you");
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			ID = Type;
		}

		public override void Update(Player player, ref int buffIndex) {
			if (player.ownedProjectileCounts[Lazy_Cloak_P.ID] <= 0) {
				player.DelBuff(buffIndex);
				buffIndex--;
			}
		}
	}
}