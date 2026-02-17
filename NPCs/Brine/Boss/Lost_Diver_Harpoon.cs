using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Weapons.Ammo;
using Origins.Items.Weapons.Ranged;
using Origins.Projectiles;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Brine.Boss {
	public class Lost_Diver_Harpoon : ModProjectile {
		public override string Texture => typeof(Harpoon_P).GetDefaultTMLName();
		public NPC Owner => Main.npc[(int)Projectile.ai[2]];
		HarpoonGlobalProjectile global;
		public override void SetDefaults() {
			Projectile.width = 4;
			Projectile.height = 4;
			Projectile.hostile = true;
			Projectile.hide = true;
			Projectile.ignoreWater = true;
			Projectile.penetrate = -1;
			Projectile.alpha = 255;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 12;
			ProjectileID.Sets.DontAttachHideToAlpha[Type] = true;
			global = new();
		}
		Vector2[] path = [];
		int pathIndex = 0;
		public override void AI() {
			NPC owner = Owner;
			if (!owner.active) {
				Projectile.Kill();
				return;
			}
			if (global.chainFrameSeed == -1) {
				global.chainFrameSeed = Main.rand.Next(0, ushort.MaxValue);
			}

			Vector2 diff = (owner.Center - Vector2.UnitY * 2) - Projectile.Center;
			if (Projectile.alpha == 0) {
				if (diff.X > 0) owner.direction = -1;
				else owner.direction = 1;
			}
			float distance = diff.Length();
			if (Projectile.ai[0] == 0f) {
				if (distance > 700f && path.Length > pathIndex)
					Projectile.ai[0] = 1f;

				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
				Projectile.localAI[1] += 1f;
				if (Projectile.localAI[1] > 5f)
					Projectile.alpha = 0;
				if (Projectile.localAI[1] >= 10f) {
					if (owner.ModNPC is Lost_Diver lostDiver && lostDiver.enragedAttackCount >= 0) {
						Projectile.localAI[1] = 5f;
						Vector2 searchSize = new Vector2(48) * 16;
						Vector2 searchStart = Projectile.Center - searchSize;
						searchStart = (searchStart / 16).Floor() * 16;
						Point topLeft = searchStart.ToTileCoordinates();
						Point bottomRight = (Projectile.Center + searchSize).ToTileCoordinates();
						HashSet<Point> validEnds = [];
						Rectangle targetHibox = owner.GetTargetData().Hitbox;
						foreach (Point point in Collision.GetTilesIn(targetHibox.TopLeft(), targetHibox.BottomRight())) {
							validEnds.Add(point);
						}
						path = CollisionExtensions.GridBasedPathfinding(
							CollisionExtensions.GeneratePathfindingGrid(topLeft, bottomRight, 1, 1),
							searchSize.ToTileCoordinates(),
							(targetHibox.Center() - searchStart).ToTileCoordinates(),
							validEnds
						).Select(p => p.ToWorldCoordinates() + searchStart).ToArray();
					} else {
						Projectile.localAI[1] = 15f;
						Projectile.velocity.Y += 0.3f;
					}
				}
				if (pathIndex < path.Length) {
					Vector2 pos = Projectile.Center;
				}
				if (pathIndex < path.Length) {
					Vector2 targetPos = path[pathIndex];
					Vector2 diffFromTarget = targetPos - Projectile.Center;
					float dist = diffFromTarget.Length();
					if (dist < 32) {
						Vector2 pos = Projectile.Center;
						pathIndex++;
						if (pathIndex + 1 < path.Length && CollisionExt.CanHitRay(pos, path[pathIndex + 1])) pathIndex++;
					}
					Projectile.velocity = diffFromTarget * 12 / dist;
				}
				Projectile.ai[1] = -1;
			} else if (Projectile.ai[0] == 1f) {
				if (distance < 50f) {
					Projectile.Kill();
					return;
				}
				Projectile.tileCollide = false;
				Projectile.rotation = diff.ToRotation() - MathHelper.PiOver2;

				diff *= 20f / distance;
				Projectile.velocity = diff;
			}
			if (Main.player.IndexInRange((int)Projectile.ai[1])) {
				Player player = Main.player[(int)Projectile.ai[1]];
				if (!player.active || player.dead) {
					Projectile.ai[1] = -1;
					return;
				}
				float multiplier = ContentExtensions.DifficultyDamageMultiplier - 1;
				if (owner.wet && !player.wet) {
					multiplier += 2;
				} else Projectile.hostile = false;
				MathUtils.LinearSmoothing(ref player.velocity, Projectile.velocity * 0.2f * multiplier, 0.6f * multiplier);
				player.OriginPlayer().forceFallthrough = true;
				Projectile.Center = player.MountedCenter - Projectile.velocity - Projectile.velocity.SafeNormalize(default) * 8;
				if (distance < 16 * (10 - multiplier)) {
					Projectile.ai[1] = -1;
				}
				if (path.Length > 0) {
					if (Projectile.timeLeft < 120) Projectile.timeLeft = 120;
				} else {
					if (Projectile.timeLeft >= 120) Projectile.timeLeft = 120;
				}
			}
		}
		public override bool CanHitPlayer(Player target) => Projectile.ai[1] == -1;
		public override bool? CanHitNPC(NPC target) {
			if (Mildew_Creeper.FriendlyNPCTypes.Contains(target.type)) return false;
			return Projectile.ai[1] == -1 ? null : false;
		}
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			modifiers.ScalingArmorPenetration += Brine_Pool_NPC.ScalingArmorPenetrationToCompensateForTSNerf;
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			if ((Main.expertMode || (Owner.wet && !target.wet)) && Projectile.ai[0] == 0) Projectile.ai[1] = target.whoAmI;
			Projectile.ai[0] = 1f;
			Projectile.netUpdate = true;
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			behindNPCs.Add(index);
		}
		public override bool PreDrawExtras() {
			Boat_Rocker harpoonGun = ModContent.GetInstance<Boat_Rocker>();
			float num129 = Projectile.position.X + 8f;
			float num130 = Projectile.position.Y + 2f;
			float velocityX = Projectile.velocity.X;
			float velocityY = Projectile.velocity.Y;
			if (velocityX == 0f && velocityY == 0f) {
				velocityY = 0.0001f;
			}

			float dist = MathF.Sqrt(velocityX * velocityX + velocityY * velocityY);
			dist = 20f / dist;
			if (Projectile.ai[0] == 0f) {
				num129 -= Projectile.velocity.X * dist;
				num130 -= Projectile.velocity.Y * dist;
			} else {
				num129 += Projectile.velocity.X * dist;
				num130 += Projectile.velocity.Y * dist;
			}

			NPC owner = Owner;
			Vector2 pos = new(num129, num130);
			velocityX = owner.Center.X - pos.X;
			velocityY = owner.Center.Y - pos.Y;
			float rotation = MathF.Atan2(velocityY, velocityX) - 1.57f;
			if (Projectile.alpha == 0 && owner.ModNPC is Lost_Diver lostDiver) {
				int direction = Projectile.Center.X < owner.Center.X ? 1 : -1;

				lostDiver.itemRotation = MathF.Atan2(velocityY * direction, velocityX * direction);
			}
			Texture2D chain = harpoonGun.ChainTexture;
			int i = 0;
			do {
				dist = MathF.Sqrt(velocityX * velocityX + velocityY * velocityY);
				if (float.IsNaN(dist) || dist < 25f) break;

				dist = chain.Height / dist;
				velocityX *= dist;
				velocityY *= dist;
				pos.X += velocityX;
				pos.Y += velocityY;
				velocityX = owner.Center.X - pos.X;
				velocityY = owner.Center.Y - pos.Y;
				Rectangle frame = chain.Frame(harpoonGun.ChainFrames, frameX: harpoonGun.GetChainFrame(i, global, Projectile));
				Main.EntitySpriteDraw(
				chain,
					pos - Main.screenPosition,
					frame,
					Lighting.GetColor((int)pos.X / 16, (int)(pos.Y / 16f)),
					rotation,
					frame.Size() * 0.5f,
					1f,
					SpriteEffects.None
				);
			} while (++i < 400);
			return false;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (path.Length > pathIndex) return false;
			Projectile.ai[0] = 1f;
			return false;
		}
	}
}
