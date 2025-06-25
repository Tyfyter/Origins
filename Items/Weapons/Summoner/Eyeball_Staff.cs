using Microsoft.Xna.Framework;
using Origins.Buffs;
using Origins.Items.Weapons.Summoner;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.OriginExtensions;

using Origins.Dev;
using System.Collections.Generic;
namespace Origins.Items.Weapons.Summoner {
	public class Eyeball_Staff : ModItem, ICustomWikiStat {
		internal static int projectileID = 0;
		public override void SetStaticDefaults() {
			ItemID.Sets.StaffMinionSlotsRequired[Item.type] = 1;
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.damage = 8;
			Item.DamageType = DamageClass.Summon;
			Item.mana = 10;
			Item.width = 32;
			Item.height = 32;
			Item.useTime = 36;
			Item.useAnimation = 36;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item44;
			Item.buffType = Mini_EOC_Buff.ID;
			Item.shoot = projectileID;
			Item.noMelee = true;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			player.AddBuff(Mini_EOC_Buff.ID, 2);
			player.SpawnMinionOnCursor(source, player.whoAmI, type, Item.damage, knockback);
			return false;
		}
	}
}
namespace Origins.Buffs {
	public class Mini_EOC_Buff : MinionBuff {
		public override IEnumerable<int> ProjectileTypes() => [
			Eyeball_Staff.projectileID
		];
		public static int ID { get; private set; }
	}
}

namespace Origins.Items.Weapons.Summoner.Minions {
	public class Mini_EOC : ModProjectile {
		public const int frameSpeed = 5;
		public override void SetStaticDefaults() {
			Eyeball_Staff.projectileID = Projectile.type;
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
			Projectile.width = 28;
			Projectile.height = 28;
			Projectile.tileCollide = true;
			Projectile.friendly = true;
			Projectile.minion = true;
			Projectile.minionSlots = 0f;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 12;
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
				player.ClearBuff(Mini_EOC_Buff.ID);
			}
			if (player.HasBuff(Mini_EOC_Buff.ID)) {
				Projectile.timeLeft = 2;
			}
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.minionSubSlots[0] += 0.5f;
			int eyeCount = player.ownedProjectileCounts[Eyeball_Staff.projectileID] / 2;
			if (originPlayer.minionSubSlots[0] <= eyeCount) {
				Projectile.minionSlots = 0.5f;
			} else {
				Projectile.minionSlots = 0;
			}
			#endregion

			#region General behavior
			Vector2 idlePosition = player.Top + new Vector2(player.direction * -player.width / 2, 0);
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
			float targetDist = 2000f;
			float targetAngle = -2;
			Vector2 targetCenter = Projectile.Center;
			int target = -1;
			void targetingAlgorithm(NPC npc, float targetPriorityMultiplier, bool isPriorityTarget, ref bool foundTarget) {
				if (!isPriorityTarget && targetDist > 700f) {
					targetDist = 700f;
				}
				if (isPriorityTarget && Projectile.ai[1] < 0) foundTarget = true;
				if (npc.CanBeChasedBy()) {
					Vector2 diff = npc.Hitbox.ClosestPointInRect(Projectile.Center) - Projectile.Center;
					float dist = diff.Length();
					if (dist > targetDist) return;
					float dot = NormDotWithPriorityMult(diff, Projectile.velocity, targetPriorityMultiplier);
					bool inRange = dist <= targetDist;
					bool lineOfSight = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height);
					if (((dot >= targetAngle && inRange) || !foundTarget) && lineOfSight) {
						targetDist = dist;
						targetAngle = dot;
						targetCenter = npc.Center;
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
			float speed = 6f + Projectile.localAI[0] / 15;
			float turnSpeed = 1f + Math.Max((Projectile.localAI[0] - 15) / 30, 0);
			float currentSpeed = Projectile.velocity.Length();
			Projectile.tileCollide = true;
			if (foundTarget) {
				Projectile.tileCollide = true;
				if (Projectile.ai[0] != target) {
					Projectile.ai[0] = target;
					Projectile.ai[1] = 0;
				} else {
					if (++Projectile.ai[1] > 180) {
						Projectile.ai[1] = -30;
					}
				}
				if ((int)Math.Ceiling(targetAngle) == -1) {
					targetCenter.Y -= 16;
				}
			} else {
				if (distanceToIdlePosition > 640f) {
					Projectile.tileCollide = false;
					speed = 16f;
				} else if (distanceToIdlePosition < 64f) {
					speed = 4f;
					turnSpeed = 0;
				} else {
					speed = 6f;
				}
				if (!Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, idlePosition, 1, 1)) {
					Projectile.tileCollide = false;
				}
			}
			LinearSmoothing(ref currentSpeed, speed, currentSpeed < 1 ? 1 : 0.1f + Projectile.localAI[0] / 60f);
			Vector2 direction = foundTarget ? targetCenter - Projectile.Center : vectorToIdlePosition;
			Projectile.velocity = (Projectile.velocity + direction.SafeNormalize(default) * turnSpeed).SafeNormalize(default) * currentSpeed;
			#endregion

			#region Animation and visuals
			// So it will lean slightly towards the direction it's moving
			Projectile.rotation = Projectile.velocity.ToRotation();
			Projectile.spriteDirection = 1;// Math.Sign(Projectile.velocity.X);

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
			if (Projectile.localAI[0] > 0) Projectile.localAI[0]--;
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.SourceDamage.Base += (int)(Projectile.localAI[0] / 6);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Vector2 intersect = Rectangle.Intersect(Projectile.Hitbox, target.Hitbox).Center.ToVector2() - Projectile.Hitbox.Center.ToVector2();
			if (intersect.X != 0 && (Math.Sign(intersect.X) == Math.Sign(Projectile.velocity.X))) {
				Projectile.velocity.X = -Projectile.velocity.X;
			}
			if (intersect.Y != 0 && (Math.Sign(intersect.Y) == Math.Sign(Projectile.velocity.Y))) {
				Projectile.velocity.Y = -Projectile.velocity.Y;
			}
			Projectile.localAI[0] += 20 - Projectile.localAI[0] / 6;
			Projectile.ai[1] = 0;
			Projectile.netUpdate = true;
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			fallThrough = true;
			return true;
		}
	}
}
