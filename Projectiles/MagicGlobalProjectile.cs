using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
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
			if (originPlayer.potatoBattery && Origins.HomingEffectivenessMultiplier[projectile.type] != 0) {
				isHoming = true;
			}
		}
	}
}
