using Microsoft.Xna.Framework;
using PegasusLib;
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
				Vector2 direction = projectile.velocity.Normalized(out _);
				bool foundTarget = Main.player[projectile.owner].DoHoming((target) => {
					Vector2 currentDiff = target.Center - projectile.Center;
					float dist = currentDiff.Length();
					currentDiff /= dist;
					float weight = Vector2.Dot(direction, currentDiff) * (300f / (dist + 100)) * 15;
					if (weight > targetWeight && Collision.CanHit(projectile.position, projectile.width, projectile.height, target.position, target.width, target.height)) {
						targetWeight = weight;
						targetDiff = currentDiff;
						return true;
					}
					return false;
				});

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
