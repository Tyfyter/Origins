using CalamityMod.Buffs.Summon;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Other.Critters;
using Origins.Reflection;
using Origins.Tiles.Limestone;
using Origins.World;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Critters {
	public class Hyrax : ModNPC, IWikiNPC {
		public Rectangle DrawRect => new(0, 0, 20, 13);
		public int AnimationFrames => 2;
		public int FrameDuration => 8;
		public NPCExportType ImageExportType => NPCExportType.SpriteSheet;
		public override void SetStaticDefaults() {
			Main.npcCatchable[Type] = true;
			Main.npcFrameCount[Type] = 6;
			NPCID.Sets.ShimmerTransformToNPC[Type] = NPCID.Shimmerfly;
			NPCID.Sets.TownCritter[Type] = true;
			NPCID.Sets.CountsAsCritter[Type] = true;
			NPCID.Sets.TakesDamageFromHostilesWithoutBeingFriendly[Type] = true;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.Bunny);
			NPC.aiStyle = -1;
			NPC.width = 26;
			NPC.height = 26;
			NPC.DeathSound = Origins.Sounds.HyraxKill;
			NPC.catchItem = ModContent.ItemType<Hyrax_Item>();
			SpawnModBiomes = [
				ModContent.GetInstance<Limestone_Cave>().Type
			];
		}
		public ref float MovementTimer => ref NPC.ai[1];
		public override void AI() {
			if (Main.rand.NextBool(650)) SoundEngine.PlaySound(Origins.Sounds.HyraxIdle, NPC.Center);
			else if (Main.rand.NextBool(900)) SoundEngine.PlaySound(Origins.Sounds.HyraxWawa, NPC.Center);
			else if (Main.rand.NextBool(1200)) SoundEngine.PlaySound(Origins.Sounds.HyraxCall, NPC.Center);

			NPC.defense = NPC.defDefense;
			if (NPC.dryadWard) {
				if (Main.masterMode)
					NPC.defense += 14;
				else if (Main.expertMode)
					NPC.defense += 10;
				else
					NPC.defense += 6;
			}

			if (NPC.target == 255) {
				NPC.TargetClosest();
				if (NPC.position.X < Main.player[NPC.target].position.X) {
					NPC.direction = 1;
					NPC.spriteDirection = NPC.direction;
				}

				if (NPC.position.X > Main.player[NPC.target].position.X) {
					NPC.direction = -1;
					NPC.spriteDirection = NPC.direction;
				}

				if (NPC.homeTileX == -1)
					NPC.UpdateHomeTileState(NPC.homeless, (int)((NPC.position.X + NPC.width / 2) / 16f), NPC.homeTileY);
			}

			int standingTileX = (int)(NPC.position.X + NPC.width / 2) / 16;
			int standingTileY = (int)(NPC.position.Y + NPC.height + 1f) / 16;

			NPC.directionY = -1;
			if (NPC.direction == 0)
				NPC.direction = 1;

			if (!WorldGen.InWorld(standingTileX, standingTileY) || NetmodeActive.MultiplayerClient && !Main.sectionManager.TileLoaded(standingTileX, standingTileY))
				return;
			const float max_danger_distance = 200f;

			bool inDanger = false;
			float leftDangerDist = float.PositiveInfinity;
			float rightDangerDist = float.PositiveInfinity;
			int saferDirection;
			NPC.rotation = 0;
			if (!NetmodeActive.MultiplayerClient) {
				foreach (NPC other in Main.ActiveNPCs) {
					if (other.friendly || other.damage <= 0 || !other.WithinRange(NPC.Center, max_danger_distance) || (!other.noTileCollide && !Collision.CanHit(NPC.Center, 0, 0, other.Center, 0, 0)))
						continue;

					if (!NPCLoader.CanHitNPC(other, NPC))
						continue;

					inDanger = true;
					float xDiff = other.Center.X - NPC.Center.X;

					if (xDiff < 0f) Min(ref leftDangerDist, -xDiff);
					else if (xDiff > 0f) Min(ref rightDangerDist, xDiff);
				}

				if (inDanger) {
					switch ((float.IsFinite(leftDangerDist), float.IsFinite(rightDangerDist))) {
						case (true, true):
						saferDirection = (rightDangerDist < leftDangerDist).ToDirectionInt();
						break;
						case (false, false):
						case (true, false):
						saferDirection = 1;
						break;
						case (false, true):
						saferDirection = -1;
						break;
					}

					if (NPC.ai[0] != 1) {
						int tileX = (int)((NPC.position.X + NPC.width / 2 + 15 * NPC.direction) / 16f);
						int tileY = (int)((NPC.position.Y + NPC.height - 16f) / 16f);
						bool currentlyDrowning = NPC.wet;
						NPCMethods.AI_007_TownEntities_GetWalkPrediction(NPC, standingTileX, NPC.homeTileX, false, currentlyDrowning, tileX, tileY, out _, out bool avoidFalling);
						if (!avoidFalling) {
							NPC.ai[0] = 1;
							MovementTimer = 120 + Main.rand.Next(120);
							NPC.localAI[3] = 0f;
							NPC.direction = saferDirection;
							NPC.netUpdate = true;
						}
					} else if (NPC.direction != saferDirection) {
						NPC.direction = saferDirection;
						NPC.netUpdate = true;
					}
				}
			}

			if (NPC.ai[0] == 0) {
				NPC.localAI[3].Cooldown();

				MathUtils.LinearSmoothing(ref NPC.velocity.X, 0, 0.1f);

				if (!NetmodeActive.MultiplayerClient) {

					if (MovementTimer > 0f)
						MovementTimer -= 1f;

					int tileX2 = (int)((NPC.position.X + NPC.width / 2 + 15 * NPC.direction) / 16f);
					int tileY2 = (int)((NPC.position.Y + NPC.height - 16f) / 16f);
					NPCMethods.AI_007_TownEntities_GetWalkPrediction(NPC, standingTileX, NPC.homeTileX, false, NPC.wet, tileX2, tileY2, out _, out bool avoidFalling);
					if (NPC.wet) {
						if (Collision.DrownCollision(NPC.position, NPC.width, NPC.height, 1f, includeSlopes: true)) {
							NPC.ai[0] = 1;
							MovementTimer = 200 + Main.rand.Next(300);
							MovementTimer += Main.rand.Next(200, 400);

							NPC.localAI[3] = 0f;
							NPC.netUpdate = true;
						}
					}

					if (MovementTimer <= 0f) {
						if (!avoidFalling) {
							NPC.ai[0] = 1;
							MovementTimer = 200 + Main.rand.Next(300);
							MovementTimer += Main.rand.Next(200, 400);

							NPC.localAI[3] = 0f;
							NPC.netUpdate = true;
						} else {
							NPC.direction *= -1;
							MovementTimer = 60 + Main.rand.Next(120);
							NPC.netUpdate = true;
						}
					}
				}

				if (!NetmodeActive.MultiplayerClient) {
					if (standingTileX < NPC.homeTileX - 25 || standingTileX > NPC.homeTileX + 25) {
						if (NPC.localAI[3] == 0f) {
							if (standingTileX < NPC.homeTileX - 50 && NPC.direction == -1) {
								NPC.direction = 1;
								NPC.netUpdate = true;
							} else if (standingTileX > NPC.homeTileX + 50 && NPC.direction == 1) {
								NPC.direction = -1;
								NPC.netUpdate = true;
							}
						}
					} else if (Main.rand.NextBool(80) && NPC.localAI[3] == 0f) {
						NPC.localAI[3] = 200f;
						NPC.direction *= -1;
						NPC.netUpdate = true;
					}
				}
			} else if (NPC.ai[0] == 1) {
				if (!Collision.DrownCollision(NPC.position, NPC.width, NPC.height, 1f, includeSlopes: true)) {
					MovementTimer -= 1f;
				}

				if (MovementTimer <= 0f) {
					NPC.ai[0] = 0;
					MovementTimer = 300 + Main.rand.Next(300);
					MovementTimer -= Main.rand.Next(100);

					NPC.localAI[3] = 60f;
					NPC.netUpdate = true;
				}

				const float speed = 1f;
				const float acceleration = 0.07f;

				if (NPC.velocity.X < -speed || NPC.velocity.X > speed) {
					if (NPC.velocity.Y == 0f)
						NPC.velocity *= 0.8f;
				} else {
					NPC.velocity.X += acceleration * NPC.direction;
					if (NPC.direction == 1) Min(ref NPC.velocity.X, speed);
					else Max(ref NPC.velocity.X, -speed);
				}
				if (NPC.velocity.Y < 0 && NPC.Hitbox.Modified(0, -2, 0, 0).OverlapsAnyTiles()) {
					NPC.ai[0] = 0;
					MovementTimer = 60 + Main.rand.Next(100);
					NPC.netUpdate = true;
				} else if(NPC.collideX) {
					NPC.velocity.Y -= acceleration;
					Max(ref NPC.velocity.Y, -speed);
					Min(ref NPC.velocity.Y, speed);
					NPC.GravityMultiplier *= 0;
					NPC.rotation = NPC.direction * -MathHelper.PiOver2;
					Max(ref MovementTimer, 10);
				} else {
					bool dontStepDown = true;
					if (NPC.homeTileY * 16 - 32 > NPC.position.Y)
						dontStepDown = false;

					if (!dontStepDown && NPC.velocity.Y == 0f)
						Collision.StepDown(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);

					Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY, holdsMatching: dontStepDown);
				}
			}
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			int limestoneTile = ModContent.TileType<Limestone>();
			for (int i = 0; i < 10; i++) {
				if (Framing.GetTileSafely(spawnInfo.SpawnTileX, spawnInfo.SpawnTileY + i).TileIsType(limestoneTile)) {
					return spawnInfo.SpawnTileY > Main.worldSurface ? 1 : 0;
				}
			}
			return 0f;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ItemID.Leather, 4));
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				Gore.NewGore(
					NPC.GetSource_Death(),
					NPC.Center + new Vector2(-8 * NPC.direction, 2).RotatedBy(NPC.rotation),
					NPC.velocity,
					Mod.GetGoreSlot("Gores/NPCs/Hyrax_Gore1")
				);
				Gore.NewGore(
					NPC.GetSource_Death(),
					NPC.Center + new Vector2(2 * NPC.direction, 5).RotatedBy(NPC.rotation),
					NPC.velocity,
					Mod.GetGoreSlot("Gores/NPCs/Hyrax_Gore2")
				);
				Gore.NewGore(
					NPC.GetSource_Death(),
					NPC.Center + new Vector2(9 * NPC.direction, 6).RotatedBy(NPC.rotation),
					NPC.velocity,
					Mod.GetGoreSlot("Gores/NPCs/Hyrax_Gore3")
				);
			}
		}
		public override void OnKill() {
			if (!NetmodeActive.MultiplayerClient && NPC.AnyInteractions() && OriginsModIntegrations.CheckAprilFools()) {
				Player player = Main.player[NPC.lastInteraction];
				Projectile.NewProjectile(NPC.GetSource_Death(), player.Bottom, Vector2.Zero, ModContent.ProjectileType<Hyrax_Laser>(), 500, 0, ai0: player.whoAmI);
				SoundEngine.PlaySound(SoundID.Zombie104, player.Bottom);
			}
		}
		public override void FindFrame(int frameHeight) {
			if (Main.rand.NextBool(350)) {
				SoundEngine.PlaySound(SoundID.Zombie15, NPC.Center); // replace with a "AWAWA" sound
			}
			if (NPC.ai[0] == 1) {
				NPC.DoFrames(6, 1..6);
			} else {
				NPC.frame.Y = 0;
				NPC.frameCounter = 0;
			}
			if (NPC.velocity.X != 0) NPC.spriteDirection = Math.Sign(NPC.velocity.X);
		}
	}
	public class Hyrax_Laser : ModProjectile {
		public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.PhantasmalDeathray}";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.PhantasmalDeathray);
			Projectile.aiStyle = 0;
			Projectile.damage = 500;
		}
		public override void AI() {
			if ((int)Projectile.ai[0] < 0) Projectile.Kill();
			Player player = Main.player[(int)Projectile.ai[0]];
			if (!player.active) Projectile.Kill();
			if (!Framing.GetTileSafely((Projectile.Bottom + Vector2.UnitY).ToTileCoordinates()).HasSolidTile()) {
				Projectile.position.Y += CollisionExt.Raymarch(Projectile.Bottom, Vector2.UnitY);
				Projectile.netUpdate = true;
			}
		}
		public override void OnSpawn(IEntitySource source) {
		}
		public override void ModifyDamageHitbox(ref Rectangle hitbox) {
			hitbox.Height = Projectile.Hitbox.Bottom + 100;
			hitbox.Y = -100;
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			overPlayers.Add(index);
		}
		public override bool PreDraw(ref Color lightColor) {
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Main.EntitySpriteDraw(
				texture,
				Projectile.Center - Main.screenPosition,
				null,
				Color.White,
				Projectile.rotation,
				texture.Size() * 0.5f,
				Projectile.scale,
				SpriteEffects.None
			);
			return false;
		}
	}
}
