using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Tyfyter.Utils;

namespace Origins.Projectiles {
	//separate global for organization
	public class MagicGlobalProjectile : GlobalProjectile {
		bool isHoming = false;
		public override bool InstancePerEntity => true;
		protected override bool CloneNewInstances => false;
		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) {
			return entity.DamageType.CountsAsClass(DamageClass.Magic);
		}
		public override void SetDefaults(Projectile projectile) {
			isHoming = false;
		}
		public override void AI(Projectile projectile) {
			if (isHoming && projectile.friendly) {
				float targetWeight = 4.5f;
				Vector2 targetDiff = default;
				bool foundTarget = false;
				for (int i = 0; i < 200; i++) {
					NPC currentNPC = Main.npc[i];
					if (currentNPC.CanBeChasedBy(this)) {
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
				if (!foundTarget) {
					Player owner = Main.player[projectile.owner];
					if (owner.hostile) {
						foreach (Player player in Main.ActivePlayers) {
							if (!player.dead && player.hostile && player.team != owner.team) {
								Vector2 currentDiff = player.Center - projectile.Center;
								float dist = currentDiff.Length();
								currentDiff /= dist;
								float weight = Vector2.Dot(projectile.velocity, currentDiff) * (300f / (dist + 100));
								if (weight > targetWeight && Collision.CanHit(projectile.position, projectile.width, projectile.height, player.position, player.width, player.height)) {
									targetWeight = weight;
									targetDiff = currentDiff;
									foundTarget = true;
								}
							}
						}
					}
				}

				if (foundTarget) {
					PolarVec2 velocity = (PolarVec2)projectile.velocity;
					OriginExtensions.AngularSmoothing(
						ref velocity.Theta,
						targetDiff.ToRotation(),
						0.001f + velocity.R * 0.0005f * Origins.HomingEffectivenessMultiplier[projectile.type]
					);
					projectile.velocity = (Vector2)velocity;
				}
			}
		}
		public override void OnSpawn(Projectile projectile, IEntitySource source) {
			OriginPlayer originPlayer = Main.player[projectile.owner].GetModPlayer<OriginPlayer>();
			if (Origins.HomingEffectivenessMultiplier[projectile.type] != 0) {
				if (!(originPlayer?.protomindItem?.IsAir??true)) {
					isHoming = true;
				} else if (originPlayer.potatoBattery) {
					isHoming = true;
				}
			}
		}
		public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter) {
			bitWriter.WriteBit(isHoming);
		}
		public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader) {
			isHoming = bitReader.ReadBit();
		}
	}
}
