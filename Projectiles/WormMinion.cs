using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Projectiles {
	public abstract class MinionBase : ModProjectile {
		public override void SetStaticDefaults() {
			// Sets the amount of frames this minion has on its spritesheet
			// This is necessary for right-click targeting
			ProjectileID.Sets.MinionTargettingFeature[Type] = true;

			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[Type] = true;
			// This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned
			ProjectileID.Sets.MinionSacrificable[Type] = true;
		}
		public abstract ref bool HasBuff(Player player);
		public TargetingData targetingData;
		public struct TargetingData {
			public Rectangle targetHitbox;
			/// <summary>
			/// the distance from the target squared, or the maximum distance squared if no target is found
			/// </summary>
			public float distanceFromTarget;
			public int targetID;
			public int lastTargetID;
			public readonly bool IsLastTarget(NPC npc) => npc.whoAmI == lastTargetID;
			public readonly float KeepTargetOrDistance(Projectile projectile, NPC npc) {
				if (IsLastTarget(npc)) return 0;
				return projectile.DistanceSQ(npc.Center);
			}
			public void KeepOrPickNearest(Projectile projectile, NPC npc, ref bool foundTarget) {
				if (Minimize(ref distanceFromTarget, KeepTargetOrDistance(projectile, npc))) {
					targetHitbox = npc.Hitbox;
					targetID = npc.whoAmI;
					foundTarget = true;
				}
			}
			public void PickNearest(Projectile projectile, NPC npc, ref bool foundTarget) {
				if (Minimize(ref distanceFromTarget, projectile.DistanceSQ(npc.Center))) {
					targetHitbox = npc.Hitbox;
					targetID = npc.whoAmI;
					foundTarget = true;
				}
			}
			public readonly void Send(BinaryWriter writer) {
				writer.Write((short)targetID);
			}
			public void Receive(BinaryReader reader) {
				targetID = reader.ReadInt16();
				if (Main.npc.GetIfInRange(targetID) is NPC npc) {
					targetHitbox = npc.Hitbox;
				}
			}
		}
		public virtual Rectangle RestRegion {
			get {
				Player player = Main.player[Projectile.owner];
				return player.Hitbox.Add(Vector2.UnitX * player.direction * -48);
			}
		}
		public virtual float MaxPriorityRange => 1000;
		public virtual float MaxNonPriorityRange => 700;
		public virtual void ResetTargetingData() {
			targetingData.targetHitbox = Projectile.Hitbox;
			targetingData.distanceFromTarget = MaxPriorityRange * MaxPriorityRange;
			targetingData.lastTargetID = targetingData.targetID;
			targetingData.targetID = -1;
		}
		public virtual void TargetingAlgorithm(NPC npc, float targetPriorityMultiplier, bool isPriorityTarget, ref bool foundTarget) {
			if (!isPriorityTarget) Min(ref targetingData.distanceFromTarget, MaxNonPriorityRange * MaxNonPriorityRange);

			targetingData.KeepOrPickNearest(Projectile, npc, ref foundTarget);
		}
		public virtual void MoveTowardsTarget() {
			bool foundTarget = targetingData.targetID != -1;
			Rectangle targetHitbox = foundTarget ? targetingData.targetHitbox : RestRegion;
			if (foundTarget) targetHitbox.Inflate(targetHitbox.Width / 8, targetHitbox.Height / 8);

			Vector2 targetPos = Projectile.Center.Clamp(targetHitbox);
			Vector2 direction = (targetPos - Projectile.Center).Normalized(out float distance);
			if (foundTarget) {
				float speed = distance switch {
					< 300f => 0.6f,
					< 600f => 0.9f,
					_ => 1.2f
				};
				Projectile.velocity += direction * speed;
				if (Vector2.Dot(Projectile.velocity.Normalized(out _), direction) < 0.25f)
					Projectile.velocity *= 0.8f;
				Projectile.velocity = Projectile.velocity.Normalized(out speed) * Math.Min(speed, 30);
			} else {
				float speed = distance switch {
					< 300f => 0.1f,
					< 600f => 0.2f,
					_ => 0.3f
				};
				Projectile.velocity += direction * speed;
				if (Vector2.Dot(Projectile.velocity.Normalized(out _), direction) < 0.25f)
					Projectile.velocity *= 0.8f;

				Projectile.velocity = Projectile.velocity.Normalized(out speed);
				if (speed > 2) speed *= 0.96f;
				Projectile.velocity *= Math.Min(speed, 15);
			}
		}
		public override void AI() => BasicAI();
		protected virtual void BasicAI() {
			Player player = Main.player[Projectile.owner];
			if ((int)Main.timeForVisualEffects % 120 == 0)
				Projectile.netUpdate = true;
			DoActiveCheck();
			if (!Projectile.active) return;

			Vector2 center = player.Center;
			if (Projectile.Distance(center) > 2000f) {
				Projectile.Center = center;
				Projectile.netUpdate = true;
			}

			ResetTargetingData();
			player.OriginPlayer().GetMinionTarget(TargetingAlgorithm);

			MoveTowardsTarget();

			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			int oldDirection = Projectile.direction;
			Projectile.direction = (Projectile.spriteDirection = ((Projectile.velocity.X > 0f) ? 1 : (-1)));
			if (oldDirection != Projectile.direction)
				Projectile.netUpdate = true;

			Projectile.position.X = MathHelper.Clamp(Projectile.position.X, 160f, Main.maxTilesX * 16 - 160);
			Projectile.position.Y = MathHelper.Clamp(Projectile.position.Y, 160f, Main.maxTilesY * 16 - 160);
		}

		public void DoActiveCheck() {
			Player player = Main.player[Projectile.owner];
			if (!player.active) {
				Projectile.active = false;
				return;
			}

			ref bool hasBuff = ref HasBuff(player);
			if (player.dead) hasBuff = false;

			if (hasBuff) Projectile.timeLeft = 2;
			else if (Projectile.IsLocallyOwned()) Min(ref Projectile.timeLeft, 2);
		}

		public override void SendExtraAI(BinaryWriter writer) {
			targetingData.Send(writer);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			targetingData.Receive(reader);
		}
	}
	public abstract class WormMinion : MinionBase {
		public enum BodyPart {
			Invalid,
			Head,
			Body,
			Tail
		}
		public struct WormData {
			int parent;
			int child;
			public int Parent {
				readonly get => parent - 1;
				set => parent = value + 1;
			}
			public int Child {
				readonly get => child - 1;
				set => child = value + 1;
			}
			public readonly void Send(BinaryWriter writer) {
				writer.Write((short)Parent);
				writer.Write((short)Child);
			}
			public void Receive(BinaryReader reader) {
				Parent = reader.ReadInt16();
				Child = reader.ReadInt16();
			}
			public override readonly string ToString() => $"{Parent}-->this-->{Child}";
		}
		public WormData wormData;
		public abstract BodyPart Part { get; }
		public abstract bool IsValidParent(Projectile segment);
		public abstract bool CanInsert(Projectile parent, Projectile child);
		// Remember, farther is for physical distance, further is for metaphorical distance, and father is for emotional distance!
		public virtual float ChildDistance => 16;
		public bool TryInsertAfter(Projectile parent) {
			if (parent.whoAmI == Projectile.whoAmI) return false;
			if (parent.owner != Projectile.owner || parent.ModProjectile is not WormMinion parentSegment) return false;
			if (!parentSegment.Part.CanHaveChild() || !IsValidParent(parent)) return false;
			Projectile existingChild = OriginExtensions.GetProjectile(Projectile.owner, parentSegment.wormData.Child);
			if (CanInsert(parent, existingChild)) {
				parentSegment.wormData.Child = Projectile.identity;
				wormData.Parent = parent.identity;

				if (existingChild?.ModProjectile is WormMinion childSegment) {
					wormData.Child = existingChild.identity;
					childSegment.wormData.Parent = Projectile.identity;
					existingChild.netUpdate = true;
				} else {
					wormData.Child = -1;
				}

				parent.netUpdate = true;
				Projectile.netUpdate = true;
				return true;
			}
			return false;
		}
		protected override void BasicAI() {
			if (Part == BodyPart.Head) {
				base.BasicAI();
				Projectile.localAI[1] = 0;
			} else {
				if (Part == BodyPart.Tail) Projectile.localAI[0] = 0;
				Player player = Main.player[Projectile.owner];
				if ((int)Main.timeForVisualEffects % 120 == 0)
					Projectile.netUpdate = true;

				if (!player.active) {
					Projectile.active = false;
					return;
				}

				ref bool hasBuff = ref HasBuff(player);
				if (player.dead) hasBuff = false;

				if (hasBuff) Projectile.timeLeft = 2;
				else if (Projectile.IsLocallyOwned()) Min(ref Projectile.timeLeft, 2);

				bool hasValidParent = false;
				Vector2 parentCenter = Vector2.Zero;
				float parentRotation = 0f;
				float parentDistance = 0f;

				if (OriginExtensions.GetProjectile(Projectile.owner, wormData.Parent) is Projectile parent) {
					if (parent.active && parent.ModProjectile is WormMinion parentSegment && parentSegment.Part.CanHaveChild() && IsValidParent(parent)) {
						hasValidParent = true;
						parentCenter = parent.Center;
						parentRotation = parent.rotation;
						parentDistance = parentSegment.ChildDistance;
						parent.localAI[0] = Projectile.localAI[0] + 1f;
						Projectile.localAI[1] = parent.localAI[1] + 1f;
						parentSegment.wormData.Child = Projectile.identity;

						if (Projectile.owner == Main.myPlayer && Projectile.type == ProjectileID.StardustDragon4 && parent.type == ProjectileID.StardustDragon1) {
							parent.Kill();
							Projectile.Kill();
							return;
						}
					}
				}

				if (!hasValidParent) {
					foreach (Projectile possibleParent in Main.ActiveProjectiles) {
						if (TryInsertAfter(possibleParent)) break;
					}
					return;
				}

				Projectile.velocity = Vector2.Zero;
				Vector2 offset = parentCenter - Projectile.Center;
				if (parentRotation != Projectile.rotation) {
					float angleDiff = MathHelper.WrapAngle(parentRotation - Projectile.rotation);
					offset = offset.RotatedBy(angleDiff * 0.1f);
				}

				Projectile.rotation = offset.ToRotation() + MathHelper.PiOver2;

				if (offset != Vector2.Zero)
					Projectile.Center = parentCenter - Vector2.Normalize(offset) * parentDistance;

				Projectile.spriteDirection = (offset.X > 0f).ToDirectionInt();
			}
		}
		public override void SendExtraAI(BinaryWriter writer) {
			base.SendExtraAI(writer);
			wormData.Send(writer);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			base.ReceiveExtraAI(reader);
			wormData.Receive(reader);
		}
		public Projectile GetChild() => OriginExtensions.GetProjectile(Projectile.owner, wormData.Child);
		public Projectile GetParent() => OriginExtensions.GetProjectile(Projectile.owner, wormData.Parent);
	}
	public static class WormExtensions {
		public static bool CanHaveChild(this WormMinion.BodyPart part) => part switch {
			WormMinion.BodyPart.Head => true,
			WormMinion.BodyPart.Body => true,
			_ => false
		};
		public static bool CanHaveParent(this WormMinion.BodyPart part) => part switch {
			WormMinion.BodyPart.Body => true,
			WormMinion.BodyPart.Tail => true,
			_ => false
		};
	}
}
