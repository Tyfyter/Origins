using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Weapons.Ranged;
using PegasusLib;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;

namespace Origins.Projectiles {
	//separate global for organization
	public class HarpoonGlobalProjectile : GlobalProjectile {
		bool isRetracting = false;
		bool slamming = false;
		int slamTime = -1;
		bool justHit = false;
		public bool bloodletter = false;
		public Vector2 extraGravity = default;
		bool boatRockerCanEmbed = false;
		int boatRockerEmbedTime = 0;
		Entity boatRockerEmbed = null;
		public int chainFrameSeed = -1;
		public FastRandom chainRandom;
		float oldWeakpointAnalyzerDist = float.PositiveInfinity;
		public override bool InstancePerEntity => true;
		protected override bool CloneNewInstances => false;
		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) {
			return entity.aiStyle == ProjAIStyleID.Harpoon;
		}
		public override void SetDefaults(Projectile projectile) {
			isRetracting = false;
			slamming = false;
		}
		public override void OnSpawn(Projectile projectile, IEntitySource source) {
			if (source is EntitySource_ItemUse itemUse) {
				if (itemUse.Item.ModItem is Boat_Rocker and not Boat_Rocker_Alt) {
					boatRockerCanEmbed = true;
				}
			}
		}
		public override bool PreAI(Projectile projectile) {
			if (slamming && justHit && projectile.penetrate > 2) {
				projectile.ai[0] = 0;
			}
			justHit = false;
			if (chainFrameSeed == -1) {
				chainFrameSeed = Main.rand.Next(0, ushort.MaxValue);
			}
			OriginGlobalProj globalProj = projectile.GetGlobalProjectile<OriginGlobalProj>();
			if (projectile.aiStyle == ProjAIStyleID.Harpoon && globalProj.weakpointAnalyzerTarget.HasValue) {
				if (projectile.TryGetOwner(out Player owner) && globalProj.weakpointAnalyzerFake) {
					projectile.aiStyle = ProjAIStyleID.Arrow;
					if (!owner.active) {
						projectile.aiStyle = ProjAIStyleID.Arrow;
						return false;
					}

					Vector2 diff = (owner.Center - Vector2.UnitY * 2) - projectile.Center;
					float distance = diff.Length();
					if (projectile.ai[0] == 0f) {
						if (distance > 700f)
							projectile.aiStyle = ProjAIStyleID.Arrow;

						projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
						projectile.localAI[1] += 1f;
						if (projectile.localAI[1] > 5f)
							projectile.alpha = 0;
					}
					projectile.velocity.Y += 0.12f;
					return false;
				}
				float distSQ = projectile.DistanceSQ(globalProj.weakpointAnalyzerTarget.Value);
				const float range = 128;
				const float rangeSQ = range * range;
				if (oldWeakpointAnalyzerDist < distSQ && MathHelper.Min(1f / (((distSQ * distSQ) / (rangeSQ * rangeSQ)) + 1), 1) < 0.01f) {
					projectile.aiStyle = ProjAIStyleID.Arrow;
				}
				oldWeakpointAnalyzerDist = distSQ;
			}
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
					boatRockerCanEmbed = false;
					if (projectile.numUpdates == -1) {
						Vector2 oldDiff = (projectile.oldPosition - projectile.Size * 0.5f) - player.MountedCenter;
						PolarVec2 polarDiff = (PolarVec2)(projectile.Center - player.MountedCenter);
						polarDiff.R = oldDiff.Length();
						if (slamTime == 0) {
							polarDiff.R = polarDiff.R * 1.02f - 16f;
							if (polarDiff.R < 24) {
								projectile.Kill();
							}
						}
						Vector2 diff = (Vector2)polarDiff;
						projectile.Center = player.MountedCenter + diff;
						Vector2 oldVel = projectile.velocity;
						projectile.velocity = (diff - oldDiff).SafeNormalize(default).RotatedBy(player.direction * 0.05f) * projectile.velocity.Length() * 0.9995f;
						if (Math.Sign(oldVel.X) != Math.Sign(projectile.velocity.X)) {
							SoundEngine.PlaySound(SoundID.Item1.WithPitch(-0.66f), projectile.Center);
						}
					}
					if (slamTime > 0) slamTime--;
				} else if (originPlayer.boatRockerAltUse || originPlayer.boatRockerAltUse2) {
					Vector2 diff = projectile.Center - player.MountedCenter;
					float dist = diff.Length();
					diff = diff.RotatedBy(MathHelper.PiOver2 * player.direction).SafeNormalize(default);
					float speed = dist * 0.075f + 12;
					int speedFactor = (int)(speed / 16 + 1);
					projectile.MaxUpdates *= speedFactor;
					projectile.velocity = diff * speed / speedFactor;
					projectile.penetrate = 10;
					slamming = true;
					projectile.netUpdate = true;
					if (originPlayer.boatRockerAltUse2) slamTime = 20;
				}
			}
			if (bloodletter) {
				float targetWeight = 4.5f;
				Vector2 targetDiff = default;
				bool foundTarget = Main.player[projectile.owner].DoHoming((target) => {
					if (target is NPC targetNPC) {
						if (!targetNPC.HasBuff(BuffID.Bleeding)) return false;
					} else if (target is Player targetPlayer) {
						if (!targetPlayer.HasBuff(BuffID.Bleeding)) return false;
					} else return false;
					Vector2 currentDiff = target.Center - projectile.Center;
					float dist = currentDiff.Length();
					currentDiff /= dist;
					float weight = Vector2.Dot(projectile.velocity, currentDiff) * (300f / (dist + 100));
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
						0.003f + velocity.R * 0.0015f * Origins.HomingEffectivenessMultiplier[projectile.type]
					);
					projectile.velocity = (Vector2)velocity;
				}
			}
			if (boatRockerEmbed is not null) {
				if (!boatRockerEmbed.active || boatRockerEmbed is Player { dead: true } || ++boatRockerEmbedTime > 90 || projectile.WithinRange(player.MountedCenter, 16 * 10)) {
					boatRockerEmbed = null;
				} else {
					float knockbackResist = boatRockerEmbed is NPC npc ? npc.knockBackResist : 1;
					MathUtils.LinearSmoothing(ref boatRockerEmbed.velocity, projectile.velocity * 0.4f, (projectile.knockBack / 2.5f) * knockbackResist);
					projectile.Center = boatRockerEmbed.Center - projectile.velocity - projectile.velocity.SafeNormalize(default) * 8;
				}
			}
			switch (projectile.aiStyle) {
				case ProjAIStyleID.Harpoon:
				if (projectile.ai[1] >= 10f) projectile.velocity += extraGravity;
				break;
				case ProjAIStyleID.Arrow:
				if (projectile.ai[1] >= 15f) projectile.velocity += (extraGravity + 0.2f * Vector2.UnitY);
				break;
			}
		}
		public override bool CanHitPlayer(Projectile projectile, Player target) {
			if (boatRockerEmbed is not null) return false;
			return base.CanHitPlayer(projectile, target);
		}
		public override bool? CanHitNPC(Projectile projectile, NPC target) {
			if (boatRockerEmbed is not null) return false;
			return base.CanHitNPC(projectile, target);
		}
		public override void PostAI(Projectile projectile) {
			if (projectile.aiStyle == ProjAIStyleID.Harpoon && Main.player.IndexInRange(projectile.owner)) {
				Player owner = Main.player[projectile.owner];
				if (owner.OriginPlayer() is OriginPlayer originPlayer) {
					originPlayer.nextActiveHarpoons++;
					originPlayer.nextActiveHarpoonAveragePosition += projectile.Center;
				}
			}
		}
		public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) {
			justHit = true;
			if (boatRockerCanEmbed && target.knockBackResist != 0) boatRockerEmbed = target;
			if (bloodletter) target.AddBuff(BuffID.Bleeding, 300);
		}
		public override void OnHitPlayer(Projectile projectile, Player target, Player.HurtInfo info) {
			justHit = true;
			if (boatRockerCanEmbed) boatRockerEmbed = target;
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
			binaryWriter.Write((short)slamTime);
			bitWriter.WriteBit(justHit);
			bitWriter.WriteBit(bloodletter);
			if (boatRockerEmbed is not null) {
				bitWriter.WriteBit(true);
				binaryWriter.Write(boatRockerEmbed.whoAmI + ((boatRockerEmbed is NPC) ? 300 : 0));
			} else {
				bitWriter.WriteBit(false);
			}
			bitWriter.WriteBit(extraGravity != default);
			if (extraGravity != default) binaryWriter.WriteVector2(extraGravity);
		}
		public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader) {
			isRetracting = bitReader.ReadBit();
			slamming = bitReader.ReadBit();
			slamTime = binaryReader.ReadInt16();
			justHit = bitReader.ReadBit();
			bloodletter = bitReader.ReadBit();
			if (bitReader.ReadBit()) {
				int target = binaryReader.ReadInt32();
				if (target >= 300) {
					boatRockerEmbed = Main.npc[target - 300];
				} else {
					boatRockerEmbed = Main.player[target];
				}
			} else {
				boatRockerEmbed = null;
			}
			if (bitReader.ReadBit()) extraGravity = binaryReader.ReadVector2();
		}
		public override bool PreDrawExtras(Projectile projectile) {
			Player player = Main.player[projectile.owner];
			if (player.HeldItem?.ModItem is Harpoon_Gun harpoonGun && harpoonGun.ChainTexture.Exists) {
				float num129 = projectile.position.X + 8f;
				float num130 = projectile.position.Y + 2f;
				float velocityX = projectile.velocity.X;
				float velocityY = projectile.velocity.Y;
				if (velocityX == 0f && velocityY == 0f) {
					velocityY = 0.0001f;
				}

				float dist = MathF.Sqrt(velocityX * velocityX + velocityY * velocityY);
				dist = 20f / dist;
				if (projectile.ai[0] == 0f) {
					num129 -= projectile.velocity.X * dist;
					num130 -= projectile.velocity.Y * dist;
				} else {
					num129 += projectile.velocity.X * dist;
					num130 += projectile.velocity.Y * dist;
				}

				Vector2 pos = new(num129, num130);
				velocityX = player.MountedCenter.X - pos.X;
				velocityY = player.MountedCenter.Y - pos.Y;
				float rotation = MathF.Atan2(velocityY, velocityX) - 1.57f;
				if (projectile.alpha == 0) {
					int direction = projectile.Center.X < player.MountedCenter.X ? 1 : -1;

					player.itemRotation = MathF.Atan2(velocityY * direction, velocityX * direction);
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
					velocityX = player.MountedCenter.X - pos.X;
					velocityY = player.MountedCenter.Y - pos.Y;
					Rectangle frame = chain.Frame(harpoonGun.ChainFrames, frameX: harpoonGun.GetChainFrame(i, this, projectile));
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
			return true;
		}
	}
}
