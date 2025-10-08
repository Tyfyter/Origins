using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Items.Weapons.Ranged;
using PegasusLib;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Crimson {
	public class BlotopusNPC : ModNPC {
		public override string Texture => typeof(Blotopus).GetDefaultTMLName();
		//public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
			CrimsonGlobalNPC.NPCTypes.Add(Type);
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.Shark);
			NPC.lifeMax = 100;
			NPC.damage = 15;
			NPC.aiStyle = 0;
			NPC.width = 64;
			NPC.height = 22;
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (spawnInfo.Sky || spawnInfo.PlayerInTown || spawnInfo.Lihzahrd) {
				return 0f;
			}
			if (spawnInfo.SpawnTileX > Main.maxTilesX / 4 && spawnInfo.SpawnTileX < Main.maxTilesX * 0.75 && !spawnInfo.Player.ZoneCrimson) {
				return 0f;
			}
			if (spawnInfo.SpawnTileY > Main.worldSurface + 20) {
				return 0f;
			}
			return spawnInfo.Water ? 0.05f : 0f;
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
			//target.AddBuff(BuffID.Bleeding, 20);
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText(),
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheCrimson,
				new SearchAliasInfoElement("gun")
			);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Blotopus>()));
		}
		void Shoot() {
			Vector2 vel = new Vector2(12, 0).RotatedBy(Main.rand.NextFloat(NPC.rotation - 0.1f, NPC.rotation + 0.1f));
			SoundEngine.PlaySound(SoundID.Item2, NPC.Center + vel * 2);
			Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + vel.RotatedByRandom(0.1f), vel, ModContent.ProjectileType<Blotopus_P_Hostile>(), NPC.damage, 3);
			NPC.velocity -= vel / 2;
		}
		public override void AI() {
			if (NPC.direction == 0) {
				NPC.TargetClosest(true);
			}
			if (NPC.wet) {
				NPC.TargetClosest(false);
				NPCAimedTarget target = NPC.GetTargetData();
				bool hasTarget = !target.Invalid &&
					((NPC.Distance(target.Center) < (target.Type == Terraria.Enums.NPCTargetType.Player ?
						(Main.player[NPC.target].wet ? 420 : 320) + Math.Max(Main.player[NPC.target].aggro / 2, -240)
						: 320)
					)
					|| NPC.life < NPC.lifeMax);
				if (!hasTarget) {
					if (NPC.collideX) {
						NPC.velocity.X = NPC.velocity.X * -1f;
						NPC.direction *= -1;
						NPC.netUpdate = true;
					}
					if (NPC.collideY) {
						NPC.netUpdate = true;
						if (NPC.velocity.Y > 0f) {
							NPC.velocity.Y = Math.Abs(NPC.velocity.Y) * -1f;
							NPC.directionY = -1;
							NPC.ai[0] = -1f;
						} else if (NPC.velocity.Y < 0f) {
							NPC.velocity.Y = Math.Abs(NPC.velocity.Y);
							NPC.directionY = 1;
							NPC.ai[0] = 1f;
						}
					}
				}
				if (hasTarget) {
					NPC.TargetClosest(true);
					Vector2 targetDiff = target.Hitbox.Center.ToVector2() - NPC.Center;
					Vector2 absDiff = new(Math.Abs(targetDiff.X), Math.Abs(targetDiff.Y));
					Vector2 diffDir = targetDiff / absDiff;
					Vector2 targetVelocity = new(0, 0);
					float targetRot = targetDiff.ToRotation();
					GeometryUtils.AngularSmoothing(ref NPC.rotation, targetRot, 0.15f);
					float distance = (absDiff * new Vector2(0.8f, 1)).Length();
					float range = 400;
					if (target.Type == Terraria.Enums.NPCTargetType.Player) {
						range += Math.Max(Main.player[NPC.target].aggro / 2, -220);
					}
					if (distance < range) {
						NPC.ai[1]++;
						if (NPC.ai[1] > 36) {
							Shoot();
							NPC.ai[1] = 20;
						}
					} else {
						NPC.ai[1] = 0;
					}

					if (absDiff.X > absDiff.Y) {
						if (distance > range * 0.65f) {
							targetVelocity.X += diffDir.X;
						} else if (distance > range * 0.35f) {
							targetVelocity.X -= diffDir.X;
						}
						targetVelocity.Y += diffDir.Y;
					} else {
						if (absDiff.X / absDiff.Y > 0.5f) {
							if (distance < range / 4) {
								targetVelocity.X -= diffDir.X;
							} else if (distance > range * 0.5f) {
								targetVelocity.X += diffDir.X;
							}
						} else {
							if (distance < range / 4) {
								targetVelocity.Y -= diffDir.Y;
							} else if (distance > range * 0.5f) {
								targetVelocity.Y += diffDir.Y;
							}
						}
					}
					int i = (int)(NPC.Center.X / 16);
					int j = (int)(NPC.Center.Y / 16);
					targetVelocity *= 12;
					MathUtils.LinearSmoothing(ref NPC.velocity.X, targetVelocity.X, 0.6f);
					MathUtils.LinearSmoothing(ref NPC.velocity.Y, targetVelocity.Y, 0.6f);
					float minY = (Framing.GetTileSafely(i, j - 1).LiquidAmount < 16) ? 0 : -5;
					if (NPC.velocity.Y > 5) {
						MathUtils.LinearSmoothing(ref NPC.velocity.Y, 5, 0.6f);
					}
					if (NPC.velocity.Y < minY) {
						MathUtils.LinearSmoothing(ref NPC.velocity.Y, minY, 0.6f);
					}
				} else {
					GeometryUtils.AngularSmoothing(ref NPC.rotation, 0, 0.15f);
					NPC.velocity.X = NPC.velocity.X + NPC.direction * 0.1f;
					if (NPC.velocity.X < -1f || NPC.velocity.X > 1f) {
						NPC.velocity.X = NPC.velocity.X * 0.95f;
					}
					if (NPC.ai[0] == -1f) {
						NPC.velocity.Y = NPC.velocity.Y - 0.01f;
						if (NPC.velocity.Y < -0.3) {
							NPC.ai[0] = 1f;
						}
					} else {
						NPC.velocity.Y = NPC.velocity.Y + 0.01f;
						if (NPC.velocity.Y > 0.3) {
							NPC.ai[0] = -1f;
						}
					}
					int i = (int)(NPC.Center.X / 16);
					int j = (int)(NPC.Center.Y / 16);
					if (Framing.GetTileSafely(i, j - 1).LiquidAmount > 128) {
						if (Framing.GetTileSafely(i, j + 1).HasTile) {
							NPC.ai[0] = -1f;
						} else if (Framing.GetTileSafely(i, j + 2).HasTile) {
							NPC.ai[0] = -1f;
						}
					}
				}
				NPC.directionY = Math.Cos(NPC.rotation) < 0 ? -1 : 1;
			} else {
				if (NPC.velocity.Y == 0f) {
					NPC.velocity.X = NPC.velocity.X * 0.94f;
					if (NPC.velocity.X > -0.2 && NPC.velocity.X < 0.2) {
						NPC.velocity.X = 0f;
					}
				}
				if (NPC.collideY) {
					NPC.velocity.Y -= 3f;
					if (NPC.ai[1] >= 40) {
						Shoot();
						NPC.ai[1] = 0;
					}
					NPC.ai[2] = (Main.rand.NextBool() ? -1 : 1) * Main.rand.NextFloat(0.1f, 0.2f);
				}

				NPC.rotation += NPC.ai[2];
				NPC.velocity.Y = NPC.velocity.Y + 0.3f;
				if (NPC.velocity.Y > 10f) {
					NPC.velocity.Y = 10f;
				}
				NPC.ai[0] = 1f;
				if (NPC.ai[1] < 40) NPC.ai[1] += Main.rand.Next(4);
			}
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Texture2D texture = TextureAssets.Npc[Type].Value;
			SpriteEffects effect = NPC.directionY == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;
			if (NPC.IsABestiaryIconDummy) effect ^= SpriteEffects.FlipHorizontally;
			Main.EntitySpriteDraw(texture, NPC.Center - screenPos, null, drawColor, NPC.rotation, texture.Size() * 0.5f, NPC.scale, effect);
			return false;
		}
		public override void FindFrame(int frameHeight) {
			base.FindFrame(frameHeight);
		}
	}
	public class Blotopus_P_Hostile : Blotopus_P {
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.ignoreWater = true;
		}
	}
}
