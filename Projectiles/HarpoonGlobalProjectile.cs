using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Projectiles {
	//separate global for organization, might also make non-artifact projectiles less laggy than the alternative
	public class HarpoonGlobalProjectile : GlobalProjectile {
		bool isRetracting = false;
		bool slamming = false;
		bool justHit = false;
		public override bool InstancePerEntity => true;
		protected override bool CloneNewInstances => false;
		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) {
			return entity.aiStyle == 13;
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
					if (projectile.aiStyle == 13 && originPlayer.turboReel) {
						projectile.extraUpdates++;
					}
					isRetracting = true;
				}
			} else if (projectile.aiStyle == 13) {
				if (slamming) {
					Vector2 oldDiff = (projectile.oldPosition - projectile.Size * 0.5f) - player.MountedCenter;
					Tyfyter.Utils.PolarVec2 polarDiff = (Tyfyter.Utils.PolarVec2)(projectile.Center - player.MountedCenter);
					polarDiff.R = oldDiff.Length();
					Vector2 diff = (Vector2)polarDiff;
					projectile.Center = player.MountedCenter + diff;
					projectile.velocity = (diff - oldDiff).SafeNormalize(default).RotatedBy(player.direction * 0.05f) * projectile.velocity.Length();
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
		}
		public override void OnHitNPC(Projectile projectile, NPC target, int damage, float knockback, bool crit) {
			justHit = true;
		}
		public override void OnHitPvp(Projectile projectile, Player target, int damage, bool crit) {
			justHit = true;
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
	}
}
