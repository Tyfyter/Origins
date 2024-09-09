using Microsoft.Xna.Framework;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Tyfyter.Utils;

namespace Origins.Projectiles {
	//separate global for organization
	public class HarpoonGlobalProjectile : GlobalProjectile {
		bool isRetracting = false;
		bool slamming = false;
		bool justHit = false;
		public bool bloodletter = false;
		public override bool InstancePerEntity => true;
		protected override bool CloneNewInstances => false;
		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) {
			return entity.aiStyle == ProjAIStyleID.Harpoon;
		}
		public override void SetDefaults(Projectile projectile) {
			isRetracting = false;
			slamming = false;
		}
		public override bool PreAI(Projectile projectile) {
			if (slamming && justHit && projectile.penetrate > 2) {
				projectile.ai[0] = 0;
			}
			justHit = false;
			return true;
		}
		public override void AI(Projectile projectile) {
			Player player = Main.player[projectile.owner];
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			if (projectile.ai[0] == 1) {
				if (!isRetracting) {
					if (projectile.aiStyle == ProjAIStyleID.Harpoon) {
						if (originPlayer.turboReel2) {
							projectile.extraUpdates += 2;
						} else if(originPlayer.turboReel) {
							projectile.extraUpdates++;
						}
					}
					isRetracting = true;
				}
			} else if (projectile.aiStyle == ProjAIStyleID.Harpoon) {
				if (slamming) {
					Vector2 oldDiff = (projectile.oldPosition - projectile.Size * 0.5f) - player.MountedCenter;
					PolarVec2 polarDiff = (PolarVec2)(projectile.Center - player.MountedCenter);
					polarDiff.R = oldDiff.Length();
					Vector2 diff = (Vector2)polarDiff;
					projectile.Center = player.MountedCenter + diff;
					Vector2 oldVel = projectile.velocity;
					projectile.velocity = (diff - oldDiff).SafeNormalize(default).RotatedBy(player.direction * 0.05f) * projectile.velocity.Length() * 0.9995f;
					if (Math.Sign(oldVel.X) != Math.Sign(projectile.velocity.X)) {
						SoundEngine.PlaySound(SoundID.Item1, projectile.Center);
					}
				} else if (originPlayer.boatRockerAltUse) {
					Vector2 diff = projectile.Center - player.MountedCenter;
					float dist = diff.Length();
					diff = diff.RotatedBy(MathHelper.PiOver2 * player.direction).SafeNormalize(default);
					float speed = dist * 0.075f + 12;
					int speedFactor = (int)(speed / 16 + 1);
					projectile.MaxUpdates *= speedFactor;
					projectile.velocity = diff * speed / speedFactor;
					projectile.penetrate = 10;
					slamming = true;
				}
			}
			if (bloodletter) {
				float targetWeight = 4.5f;
				Vector2 targetDiff = default;
				bool foundTarget = false;
				for (int i = 0; i < 200; i++) {
					NPC currentNPC = Main.npc[i];
					if (currentNPC.CanBeChasedBy(projectile) && currentNPC.HasBuff(BuffID.Bleeding)) {
						Vector2 currentDiff = currentNPC.Center - projectile.Center;
						float dist = currentDiff.Length();
						currentDiff /= dist;
						float weight = Vector2.Dot(projectile.velocity, currentDiff) * (300f / (dist + 100));
						if (weight > targetWeight && Collision.CanHit(projectile.position, projectile.width, projectile.height, currentNPC.position, currentNPC.width, currentNPC.height)) {
							targetWeight = weight;
							targetDiff = currentDiff;
							foundTarget = true;
						}
					}
				}

				if (foundTarget) {
					PolarVec2 velocity = (PolarVec2)projectile.velocity;
					OriginExtensions.AngularSmoothing(
						ref velocity.Theta,
						targetDiff.ToRotation(),
						0.003f + velocity.R * 0.0015f * Origins.HomingEffectivenessMultiplier[projectile.type]
					);
					projectile.velocity = (Vector2)velocity;
				}
			}
		}
		public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) {
			justHit = true;
			if (bloodletter) target.AddBuff(BuffID.Bleeding, 300);
		}
		public override void OnHitPlayer(Projectile projectile, Player target, Player.HurtInfo info) {
			justHit = true;
			if (bloodletter) target.AddBuff(BuffID.Bleeding, 300);
		}
		public override bool? Colliding(Projectile projectile, Rectangle projHitbox, Rectangle targetHitbox) {
			if (slamming) {
				float collisionPoint = 0;
				Vector2 startpoint = Main.player[projectile.owner].MountedCenter;
				if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), startpoint, projectile.Center, 6, ref collisionPoint)) {
					return true;
				}
			}
			return null;
		}
		public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter) {
			bitWriter.WriteBit(isRetracting);
			bitWriter.WriteBit(slamming);
			bitWriter.WriteBit(justHit);
			bitWriter.WriteBit(bloodletter);
		}
		public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader) {
			isRetracting = bitReader.ReadBit();
			slamming = bitReader.ReadBit();
			justHit = bitReader.ReadBit();
			bloodletter = bitReader.ReadBit();
		}
	}
}
