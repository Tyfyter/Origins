using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Items.Weapons.Demolitionist;
using Origins.Tiles.Brine;
using Origins.World.BiomeData;
using PegasusLib;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Brine {
	public class Shotgunfish : Brine_Pool_NPC, ICustomWikiStat {
		[field: CloneByReference]
		public HashSet<int> PreyNPCTypes { get; private set; } = [];
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.npcFrameCount[NPC.type] = 8;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Velocity = 1f
			};
			TargetNPCTypes.Add(ModContent.NPCType<Nasty_Crawdad>());
			TargetNPCTypes.Add(ModContent.NPCType<Mildew_Creeper>());
			PreyNPCTypes.Add(ModContent.NPCType<Nasty_Crawdad>());
			AprilFoolsTextures.AddNPC(this);
		}
		public bool? Hardmode => true;
		public override void SetDefaults() {
			NPC.aiStyle = NPCAIStyleID.ActuallyNone;
			NPC.lifeMax = 140;
			NPC.defense = 14;
			NPC.damage = 26;
			NPC.width = 28;
			NPC.height = 28;
			NPC.friendly = false;
			NPC.HitSound = SoundID.NPCHit19;
			NPC.DeathSound = SoundID.NPCDeath22;
			NPC.knockBackResist = 0.95f;
			NPC.value = 500;
			NPC.noGravity = true;
			SpawnModBiomes = [
				ModContent.GetInstance<Brine_Pool>().Type
			];
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			return Brine_Pool.SpawnRates.EnemyRate(spawnInfo, Brine_Pool.SpawnRates.A_GUN, true);
		}
		public override bool CanSpawnInPosition(int tileX, int tileY) => base.CanSpawnInPosition(tileX, tileY) && AnyMossNearSpawn(tileX, tileY);
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags([
				this.GetBestiaryFlavorText()
			]);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(new LeadingConditionRule(DropConditions.PlayerInteraction).WithOnSuccess(
				ItemDropRule.ByCondition(new Conditions.IsHardmode(), ModContent.ItemType<Alkaliphiliac_Tissue>(), 1, 1, 2)
			));
		}
		public override bool CanTargetNPC(NPC other) {
			return base.CanTargetNPC(other) && other.WithinRange(NPC.Center, 16 * 12 + NPC.width + Math.Max(other.width, other.height));
		}
		public override bool CanTargetPlayer(Player player) {
			return base.CanTargetPlayer(player) && player.WithinRange(NPC.Center, 16 * 12 + NPC.width + Math.Max(player.width, player.height));
		}
		public override float RippleTargetWeight(float magnitude, float distance) {
			return base.RippleTargetWeight(magnitude, distance) * 2;
		}
		public override void AI() {
			DoTargeting();
			Vector2 direction;
			if (NPC.wet) {
				bool flipTargetRotation = false;
				NPC.noGravity = true;
				bool targetIsPrey = TargetPos != default && !TargetIsRipple && NPC.HasNPCTarget && PreyNPCTypes.Contains(Main.npc[NPC.TranslatedTargetIndex].type);
				if (TargetPos != default) {
					if ((!targetIsPrey && !NPC.WithinRange(TargetPos, 16 * 12)) || (TargetIsRipple && NPC.ai[1] > 0)) {
						TargetPos = default;
					} else if (NPC.HasPlayerTarget) {
						Player target = Main.player[NPC.target];
						if (!target.active || target.dead || !CanTargetPlayer(target)) TargetPos = default;
					} else if (NPC.HasNPCTarget) {
						NPC target = Main.npc[NPC.TranslatedTargetIndex];
						if (!target.active || !CanTargetNPC(target)) TargetPos = default;
					}
				}
				if (TargetPos != default) {
					direction = NPC.DirectionTo(TargetPos);
					if (NPC.ai[1] <= 0) {
						direction *= -1;
					}
					if (NPC.ai[1] > 0 && CanSeeTarget) {
						flipTargetRotation = true;
						if (GeometryUtils.AngleDif(NPC.rotation, direction.ToRotation(), out _) < 1f && ++NPC.ai[0] >= 40) {
							NPC.ai[0] = 0;
							if (Main.netMode != NetmodeID.MultiplayerClient) {
								Vector2 velocity = GeometryUtils.Vec2FromPolar(8, NPC.rotation);
								int type = ModContent.ProjectileType<Shotgunfish_P>();
								for (int i = 4; i-- > 0;) {
									Projectile.NewProjectile(
										NPC.GetSource_FromAI(),
										NPC.Center,
										velocity.RotatedByRandom(0.25f) * Main.rand.NextFloat(0.9f, 1f),
										type,
										(int)(40 * ContentExtensions.DifficultyDamageMultiplier),
										1
									);
								}
							}
							NPC.ai[1]--;
							SoundEngine.PlaySound(SoundID.Item14.WithPitchRange(0.25f, 0.65f), NPC.Center);
						}
						direction *= -0.1f;
					} else {
						NPC.ai[0] = 0;
						//Dust.NewDust(TargetPos, 0, 0, 6);
						if (targetIsPrey && CanSeeTarget) {
							if (TargetPos.IsWithin(NPC.Center, 16 * 6)) {
								flipTargetRotation = true;
							} else {
								direction *= -1;
							}
						}
					}
				} else {
					NPC.ai[0] = 0;
					if (NPC.collideX && Math.Abs(NPC.velocity.X) > Math.Abs(NPC.velocity.Y) * 0.25f) {
						NPC.velocity.X = -NPC.direction;
						NPC.rotation += MathHelper.Pi;
					}
					NPC.direction = Math.Sign(NPC.velocity.X);
					if (NPC.direction == 0) NPC.direction = 1;
					direction = Vector2.UnitX * NPC.direction;
				}
				if (NPC.ai[3] > 150) {
					float dist = 16 * 25;
					int mossType = ModContent.TileType<Peat_Moss>();
					for (int i = -3; i < 3; i++) {
						Vector2 dir = Vector2.UnitX.RotatedBy(NPC.rotation + i * 0.5f * NPC.direction);
						float newDist = CollisionExt.Raymarch(NPC.Center, dir, dist);
						//OriginExtensions.DrawDebugLine(NPC.Center, NPC.Center + dir * newDist);
						if (TargetPos == default) {
							newDist *= 1 + (i * 0.01f);
						} else {
							newDist *= 1 - Vector2.Dot(direction, dir);
						}
						if (newDist < dist && Framing.GetTileSafely(NPC.Center + dir * (newDist + 2)).TileIsType(mossType)) {
							dist = newDist;
							direction = dir;
						}
					}
					if (dist < 28) {
						NPC.ai[3] = Main.rand.NextFloat(0, 30);
						if (NPC.ai[1] < 3) NPC.ai[1]++;
						NPC.netUpdate = true;
					}
				} else {
					NPC.ai[3]++;
					if (!flipTargetRotation) {
						const float dist = 16 * 4;
						float tileAvoidance = 0;
						for (int i = -3; i < 4; i++) {
							if (i == 0) continue;
							Vector2 dir = Vector2.UnitX.RotatedBy(NPC.rotation + i * 0.5f * NPC.direction);
							float newDist = CollisionExt.Raymarch(NPC.Center, dir, dist);
							//OriginExtensions.DrawDebugLine(NPC.Center, NPC.Center + dir * newDist);
							if (newDist < dist && Framing.GetTileSafely(NPC.Center + dir * (newDist + 2)).HasFullSolidTile()) {
								tileAvoidance += dist / (newDist * i + 1);
							}
						}
						if (tileAvoidance != 0) {
							direction = direction.RotatedBy(Math.Clamp(tileAvoidance * -0.5f * NPC.direction, -MathHelper.PiOver2, MathHelper.PiOver2));
						}
					}
				}
				float oldRot = NPC.rotation;
				GeometryUtils.AngularSmoothing(ref NPC.rotation, direction.ToRotation() + (flipTargetRotation ? MathHelper.Pi : 0), 0.1f);
				float diff = GeometryUtils.AngleDif(oldRot, NPC.rotation, out int turnDir) * 0.75f;
				NPC.velocity = NPC.velocity.RotatedBy(diff * turnDir) * (1 - diff * 0.1f);
				NPC.velocity *= 0.96f;
				NPC.velocity += direction * 0.2f;
				if (!Collision.WetCollision(NPC.position + NPC.velocity, 20, 20)) {
					NPC.velocity += direction;
				}
			} else {
				NPC.noGravity = false;
				NPC.rotation = NPC.velocity.ToRotation();
				if (NPC.collideY) NPC.velocity.X *= 0.94f;
			}
			NPC.spriteDirection = Math.Sign(Math.Cos(NPC.rotation));
		}
		public override bool? CanFallThroughPlatforms() => true;
		public override void FindFrame(int frameHeight) {
			if (NPC.wet) NPC.DoFrames(6);
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				Gore.NewGore(
					NPC.GetSource_Death(),
					NPC.Center + new Vector2(2 * NPC.direction, -2).RotatedBy(NPC.rotation),
					NPC.velocity,
					Mod.GetGoreSlot("Gores/NPCs/Shotgunfish1_Gore")
				);
				Gore.NewGore(
					NPC.GetSource_Death(),
					NPC.Center + new Vector2(-17 * NPC.direction, -3).RotatedBy(NPC.rotation),
					NPC.velocity,
					Mod.GetGoreSlot("Gores/NPCs/Shotgunfish2_Gore")
				);
				Gore.NewGore(
					NPC.GetSource_Death(),
					NPC.Center + new Vector2(-38 * NPC.direction, -2).RotatedBy(NPC.rotation),
					NPC.velocity,
					Mod.GetGoreSlot("Gores/NPCs/Shotgunfish3_Gore")
				);
			}
		}
		public AutoLoadingAsset<Texture2D> afTexture = $"{typeof(Shotgunfish).GetDefaultTMLName()}_AF";
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			SpriteEffects spriteEffects = SpriteEffects.FlipHorizontally;
			if (NPC.spriteDirection != 1) {
				spriteEffects |= SpriteEffects.FlipVertically;
			}
			if (NPC.IsABestiaryIconDummy) NPC.rotation = NPC.velocity.ToRotation();
			Texture2D texture = TextureAssets.Npc[Type].Value;
			Vector2 halfSize = new(texture.Width * 0.5f, (texture.Height / Main.npcFrameCount[NPC.type]) * 0.5f);
			Vector2 position = new(NPC.position.X - screenPos.X + (NPC.width / 2) - texture.Width * NPC.scale / 2f + halfSize.X * NPC.scale, NPC.position.Y - screenPos.Y + NPC.height - texture.Height * NPC.scale / Main.npcFrameCount[NPC.type] + 4f + halfSize.Y * NPC.scale + NPC.gfxOffY);
			Vector2 origin = new(halfSize.X * 1.6f, halfSize.Y);
			spriteBatch.Draw(
				TextureAssets.Npc[Type].Value,
				position,
				NPC.frame,
				drawColor,
				NPC.rotation,
				origin,
				NPC.scale,
				spriteEffects,
			0);
			return false;
		}
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {

		}
	}
	public class Shotgunfish_P : Peatball_P {
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.ignoreWater = true;
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Projectile.hostile = true;
			Projectile.npcProj = true;
			Projectile.appliesImmunityTimeOnSingleHits = true;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 10;
		}
		public override bool? CanHitNPC(NPC target) => target.type == ModContent.NPCType<Shotgunfish>() ? false : null;
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			if (Projectile.timeLeft > 0) {
				target.immune = true;
				Projectile.playerImmune[target.whoAmI] += 1;
				Projectile.Kill();
			}
		}
	}
}
