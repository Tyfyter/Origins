using Origins.Buffs;
using PegasusLib;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Defiled.Boss {
	public class DA_Guided_Missile : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.MagicMissile;
		static public int ID { get; private set; }
		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailingMode[Type] = ProjectileID.Sets.TrailingMode[ProjectileID.MagicMissile];
			ProjectileID.Sets.TrailCacheLength[Type] = ProjectileID.Sets.TrailCacheLength[ProjectileID.MagicMissile];
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.hostile = true;
			Projectile.timeLeft = 600;
			Projectile.width = Projectile.height = 8;
		}
		public override void AI() {
			NPC owner = Main.npc[(int)Projectile.ai[0]];
			if (!owner.active || owner.ModNPC is not Defiled_Amalgamation { AIState: Defiled_Amalgamation.state_magic_missile } || Projectile.velocity.LengthSquared() < 0.01f) {
				Projectile.Kill();
				return;
			}
			NPCAimedTarget target = owner.GetTargetData();
			void MoveTowardsTarget(Vector2 target, float inertia = 25) {
				float damageMult = ContentExtensions.DifficultyDamageMultiplier;
				inertia -= damageMult * 2;
				float speed = 10 + damageMult * 0.5f;
				Projectile.velocity = (((target - Projectile.Center).SafeNormalize(default) * speed + Projectile.velocity * (inertia - 1)) / inertia).WithMaxLength(speed);
			}
			if (CollisionExt.CanHitRay(Projectile.Center, target.Center)) {
				MoveTowardsTarget(target.Center);
			} else if (Projectile.ai[1] != 0) {
				Vector2 targetPos = new(Projectile.ai[1], Projectile.ai[2]);
				MoveTowardsTarget(targetPos);
				if (Projectile.DistanceSQ(targetPos) < 16 * 16) {
					Projectile.ai[1] = 0;
				}
			} else {
				Pathfind(target);
			}
			Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.AncientLight, Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 100, Scale: 1f).noGravity = true;
		}
		public void Pathfind(NPCAimedTarget target) {
			int bestMatch = -1;
			float bestMatchDist = float.PositiveInfinity;
			Vector2 diff = target.Center - Projectile.Center;
			for (int i = 0; i < dirs.Length; i++) {
				float dist = dirs[i].ToVector2().DistanceSQ(diff);
				if (dist < bestMatchDist) {
					bestMatchDist = dist;
					bestMatch = i;
				}
			}
			diff.Normalize();
			Vector2 position = Projectile.position;
			position += diff * CollisionExt.Raymarch(position, diff, 16 * 50);
			Vector2 clockwiseTarget = default;
			Vector2 counterclockwiseTarget = default;
			if (Crawl(target, position.ToTileCoordinates(), bestMatch, false) is Point pos1) {
				clockwiseTarget = pos1.ToWorldCoordinates();
				/*Rectangle rect = new(0, 0, 16, 16);
				rect.X = pos1.X * 16;
				rect.Y = pos1.Y * 16;
				OriginExtensions.DrawDebugOutline(rect);*/
			}
			if (Crawl(target, position.ToTileCoordinates(), bestMatch, true) is Point pos2) {
				counterclockwiseTarget = pos2.ToWorldCoordinates();
			}
			if (clockwiseTarget == default && counterclockwiseTarget == default) return;
			if (target.Center.DistanceSQ(clockwiseTarget) > target.Center.DistanceSQ(counterclockwiseTarget)) {
				Projectile.ai[1] = counterclockwiseTarget.X;
				Projectile.ai[2] = counterclockwiseTarget.Y;
			} else {
				Projectile.ai[1] = clockwiseTarget.X;
				Projectile.ai[2] = clockwiseTarget.Y;
			}
		}
		static Point[] dirs = [
			new(0, -1),
			new(1, -1),
			new(1, 0),
			new(1, 1),
			new(0, 1),
			new(-1, 1),
			new(-1, 0),
			new(-1, -1)
		];
		public Point? Crawl(NPCAimedTarget target, Point pos, int dir, bool counterclockwise) {
			const float max_dist = 16 * 50;
			static bool IsValidPosition(Point pos) => !Framing.GetTileSafely(pos).HasFullSolidTile();
			static bool IsOnes(Point p) => p.X is -1 or 1 && p.Y is -1 or 1;
			List<Point> path = [pos];
			//Rectangle rect = new(0, 0, 16, 16);
			test:
			if (CollisionExt.CanHitRay(pos.ToWorldCoordinates(), target.Position)) {
				if (CollisionExt.CanHitRay(Projectile.Center, path[^1].ToWorldCoordinates())) return path[^1];
				if (path.Count <= 1) return null;
				Point? nextTarget = path[1];
				for (int i = 1; i < path.Count; i++) {
					/*rect.X = path[i].X * 16;
					rect.Y = path[i].Y * 16;
					OriginExtensions.DrawDebugOutline(rect);*/
					if (CollisionExt.CanHitRay(Projectile.Center, path[i].ToWorldCoordinates())) {
						nextTarget = path[i];
					} else {
						Point step = path[i] - nextTarget.Value;
						if (IsOnes(step)) {
							for (int j = 0; j < dirs.Length; j++) {
								if (step == dirs[j]) {
									nextTarget += dirs[(j + (counterclockwise ? -1 : 1) + dirs.Length) % dirs.Length];
									break;
								}
							}
						}
					}
				}
				return nextTarget;
			}
			//rect.X = pos.X * 16;
			//rect.Y = pos.Y * 16;
			//OriginExtensions.DrawDebugOutline(rect);
			if (counterclockwise) {
				for (int i = 0; i < dirs.Length; i++) {
					int nextDir = (dir - i + dirs.Length) % dirs.Length;
					if (IsValidPosition(pos + dirs[nextDir])) {
						pos += dirs[nextDir];
						dir = (nextDir + 2) % dirs.Length;
						/*rect.X = pos.X * 16;
						rect.Y = pos.Y * 16;
						OriginExtensions.DrawDebugOutline(rect);*/
						//Dust.NewDustPerfect(pos.ToWorldCoordinates(), 6, Vector2.Zero).noGravity = true;
						break;
					}
					/*Point miss = pos + dirs[nextDir];
					rect.X = miss.X * 16;
					rect.Y = miss.Y * 16;
					OriginExtensions.DrawDebugOutline(rect, dustType: DustID.DungeonWater);*/
				}
			} else {
				for (int i = 0; i < dirs.Length; i++) {
					int nextDir = (dir + i) % dirs.Length;
					if (IsValidPosition(pos + dirs[nextDir])) {
						pos += dirs[nextDir];
						dir = (nextDir - 2 + dirs.Length) % dirs.Length;
						//Dust.NewDustPerfect(pos.ToWorldCoordinates(), 6, Vector2.Zero).noGravity = true;
						/*rect.X = pos.X * 16;
						rect.Y = pos.Y * 16;
						OriginExtensions.DrawDebugOutline(rect);*/
						break;
					}
					/*Point miss = pos + dirs[nextDir];
					rect.X = miss.X * 16;
					rect.Y = miss.Y * 16;
					OriginExtensions.DrawDebugOutline(rect, dustType: DustID.DungeonWater);*/
				}
			}
			if (path.Contains(pos) || pos.ToWorldCoordinates().DistanceSQ(target.Center) > max_dist * max_dist) return null;
			path.Add(pos);
			goto test;
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			target.AddBuff(ModContent.BuffType<Rasterized_Debuff>(), 160);
			Projectile.Kill();
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(ModContent.BuffType<Rasterized_Debuff>(), 160);
			Projectile.Kill();
		}
		public override void OnKill(int timeLeft) {
			NPC owner = Main.npc[(int)Projectile.ai[0]];
			if (owner.active && owner.ModNPC is Defiled_Amalgamation { AIState: Defiled_Amalgamation.state_magic_missile } parent) {
				parent.AIState = -Defiled_Amalgamation.state_magic_missile;
				owner.ai[1] = 0;
			}
			float scale = 1f;
			for (int i = 0; i < 6; i++) {
				Dust.NewDustDirect(
					Projectile.position,
					Projectile.width,
					Projectile.height,
					DustID.AncientLight,
					Projectile.velocity.X * 0.5f,
					Projectile.velocity.Y * 0.5f,
					100,
					Scale: scale
				).noGravity = true;
			}
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Projectile.ai[1] = 0;
			return Projectile.velocity.LengthSquared() == 0;
		}
		public override bool PreDraw(ref Color lightColor) {
			default(MagicMissileDrawer).Draw(Projectile);
			return false;
		}
	}
}
