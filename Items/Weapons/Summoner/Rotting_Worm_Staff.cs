using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Weapons.Summoner;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.OriginExtensions;

namespace Origins.Items.Weapons.Summoner {
	public class Rotting_Worm_Staff : ModItem, ICustomWikiStat {
		internal static int projectileID = 0;
		public override void SetStaticDefaults() {
			ItemID.Sets.StaffMinionSlotsRequired[Item.type] = 1;
			PegasusLib.Sets.ItemSets.InflictsExtraDebuffs[Type] = [BuffID.Poisoned];
		}
		public override void SetDefaults() {
			Item.damage = 4;
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
			Item.buffType = Wormy_Buff.ID;
			Item.shoot = projectileID;
			Item.noMelee = true;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			player.AddBuff(Wormy_Buff.ID, 2);
			player.SpawnMinionOnCursor(source, player.whoAmI, type, Item.damage, knockback);
			return false;
		}
	}
}
namespace Origins.Buffs {
	public class Wormy_Buff : MinionBuff {
		public static int ID { get; private set; }
		public override IEnumerable<int> ProjectileTypes() => [
			Rotting_Worm_Staff.projectileID
		];
	}
}

namespace Origins.Items.Weapons.Summoner.Minions {
	public class Rotting_Worm_Head : Mini_EOW_Base {
		public override void SetStaticDefaults() {
			Rotting_Worm_Staff.projectileID = Projectile.type;
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
			base.SetStaticDefaults();
		}
		public override void SetDefaults() {
			DrawOriginOffsetY = -29;
			base.SetDefaults();
			Projectile.minionSlots = 1f;
			Projectile.localAI[0] = -1;
		}
		public override void OnSpawn(IEntitySource source) {
			Projectile.velocity.Y += 6;
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			if (Main.myPlayer == player.whoAmI && Projectile.localAI[0] != Projectile.identity) {
				Projectile.localAI[0] = Projectile.identity;
				Projectile current;
				Projectile last = Projectile;
				Projectile.netUpdate = true;
				int type = Rotting_Worm_Body.ID;
				//body
				current = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, type, Projectile.damage, Projectile.knockBack, Projectile.owner, ai1: last.identity);
				last.ai[0] = current.identity;

				last = current;
				//tail
				current = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, Rotting_Worm_Tail.ID, Projectile.damage, Projectile.knockBack, Projectile.owner, ai1: last.identity);
				last.ai[0] = current.identity;
			}

			#region Active check
			// This is the "active check", makes sure the minion is alive while the player is alive, and despawns if not
			if (player.dead || !player.active) {
				player.ClearBuff(Wormy_Buff.ID);
			}
			if (player.HasBuff(Wormy_Buff.ID)) {
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
			float targetDist = 2000f;
			float targetAngle = -2;
			Vector2 targetCenter = Projectile.position;
			int target = -1;
			void targetingAlgorithm(NPC npc, float targetPriorityMultiplier, bool isPriorityTarget, ref bool foundTarget) {
				if (!isPriorityTarget && targetDist > 700f) {
					targetDist = 700f;
				}
				if (npc.CanBeChasedBy()) {
					Vector2 diff = npc.Hitbox.ClosestPointInRect(Projectile.Center) - Projectile.Center;
					float dist = diff.Length();
					if (dist > targetDist) return;
					float dot = NormDotWithPriorityMult(diff, Projectile.velocity, targetPriorityMultiplier);
					bool inRange = dist < targetDist;
					//bool jumpOfHight = (npc.Bottom.Y-projectile.Top.Y)<160;
					if ((dot > targetAngle && inRange) || !foundTarget) {
						targetDist = dist;
						targetAngle = dot;
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
			bool leap = false;
			if (foundTarget || distanceToIdlePosition <= 600f) {
				if (Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, Projectile.position + Projectile.velocity * 4, Projectile.width, Projectile.height)) {
					if (Projectile.ai[2] <= 0) leap = true;
					Projectile.ai[2] = 5;
				}
			}
			if (distanceToIdlePosition > 900f) Projectile.ai[2] = 0;
			// Default movement parameters (here for attacking)
			float speed = 8f + (targetCenter.Y < Projectile.Center.Y ? (Projectile.Center.Y - targetCenter.Y) / 32f : 0);
			float turnSpeed = 2f;
			float currentSpeed = Projectile.velocity.Length();
			if (foundTarget) {
				if ((int)Math.Ceiling(targetAngle) == -1) {
					targetCenter.Y -= 16;
				}
			} else {
				if (distanceToIdlePosition > 600f) {
					speed = 16f;
				} else if (distanceToIdlePosition <= 120f) {
					speed = 4f;
				}
			}
			if (Projectile.ai[2] > 0) {
				Projectile.velocity.Y += 0.3f;
				turnSpeed = 0.1f;
				Projectile.ai[2]--;
				if (leap) {
					turnSpeed = 10f;
					targetCenter.Y -= 64 * NormDot(Projectile.velocity, foundTarget ? targetCenter - Projectile.Center : vectorToIdlePosition);
				}
			} else LinearSmoothing(ref currentSpeed, speed, currentSpeed < 1 ? 1 : 0.1f);
			Vector2 direction = foundTarget ? targetCenter - Projectile.Center : vectorToIdlePosition;
			direction.Normalize();
			Projectile.velocity = Vector2.Normalize(Projectile.velocity + direction * turnSpeed) * currentSpeed;
			if (Projectile.ai[2] <= 0 && (++Projectile.frameCounter) * currentSpeed > 60) {
				SoundEngine.PlaySound(SoundID.WormDig.WithPitch(1), Projectile.Center);
				Projectile.frameCounter = 0;
			}
			#endregion

			#region Worminess
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			#endregion
		}
		public override void OnKill(int timeLeft) {
			int currentIndex = Projectile.GetByUUID(Projectile.owner, Projectile.ai[0]);
			while (Main.projectile.IndexInRange(currentIndex)) {
				Projectile currentProjectile = Main.projectile[currentIndex];
				currentProjectile.Kill();
				currentIndex = Projectile.GetByUUID(Projectile.owner, currentProjectile.ai[0]);
			}
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.SourceDamage.Base += (int)Projectile.velocity.Length();
		}
	}
	public class Rotting_Worm_Body : Mini_EOW_Base {
		internal static int ID = 0;
		public override void SetStaticDefaults() {
			ID = Projectile.type;
			base.SetStaticDefaults();
		}
		public override void SetDefaults() {
			DrawOriginOffsetY = -23;
			base.SetDefaults();
		}
	}
	public class Rotting_Worm_Tail : Mini_EOW_Base {
		internal static int ID = 0;
		public override void SetStaticDefaults() {
			ID = Projectile.type;
			base.SetStaticDefaults();
		}
		public override void SetDefaults() {
			DrawOriginOffsetY = -32;
			base.SetDefaults();
		}
	}
	public abstract class Mini_EOW_Base : ModProjectile {
		public const int frameSpeed = 5;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Wormy");
			// This is necessary for right-click targeting
			ProjectileID.Sets.MinionTargettingFeature[Type] = true;

			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[Type] = true;
			// This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned
			ProjectileID.Sets.CultistIsResistantTo[Type] = true;
			ProjectileID.Sets.TrailCacheLength[Type] = 3;
			ProjectileID.Sets.TrailingMode[Type] = 2;
			ProjectileID.Sets.NeedsUUID[Type] = true;
		}

		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Summon;
			Projectile.width = 21;
			Projectile.height = 21;
			Projectile.tileCollide = false;
			Projectile.friendly = true;
			Projectile.minion = true;
			Projectile.minionSlots = 0f;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 12;
			Projectile.scale = 0.5f;
			Projectile.timeLeft = 2;
			Projectile.netImportant = true;
			DrawOriginOffsetX = 0.5f;
		}

		public override void AI() {
			#region Worminess
			if (Main.myPlayer != Projectile.owner) Projectile.timeLeft = 2;
			Player player = Main.player[Projectile.owner];
			int lastIndex = Projectile.GetByUUID(Projectile.owner, Projectile.ai[1]);
			if (!Main.projectile.IndexInRange(lastIndex)) {
				if (Main.myPlayer != player.whoAmI) return;
				Projectile.Kill();
			}
			Projectile last = Main.projectile[lastIndex];
			if (!last.active || !(last.type == Rotting_Worm_Staff.projectileID || last.type == Rotting_Worm_Body.ID)) {
				if (Main.myPlayer == player.whoAmI) Projectile.Kill();
				return;
			}
			if (player.HasBuff(Wormy_Buff.ID)) {
				Projectile.timeLeft = 2;
			}
			float dX = last.Center.X - Projectile.Center.X;
			float dY = last.Center.Y - Projectile.Center.Y;
			Projectile.rotation = (float)Math.Atan2(dY, dX) + MathHelper.PiOver2;
			float dist = (float)Math.Sqrt(dY * dY + dX * dX);
			if (dist != 0f) {
				dist = (dist - 21) / dist;
				dX *= dist;
				dY *= dist;
				Projectile.position.X += dX;
				Projectile.position.Y += dY;
			}
			#endregion
		}

		// Here you can decide if your minion breaks things like grass or pots
		public override bool? CanCutTiles() {
			return false;
		}

		// This is mandatory if your minion deals contact damage (further related stuff in AI() in the Movement region)
		public override bool MinionContactDamage() {
			return true;
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.SourceDamage.Base += (int)(Projectile.velocity.Length() / 2);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (Main.rand.NextBool(10)) {
				target.AddBuff(BuffID.Poisoned, 180);
			}
		}
	}
}
