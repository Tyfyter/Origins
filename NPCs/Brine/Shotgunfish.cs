using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using Origins.Items.Weapons.Demolitionist;
using Origins.Items.Weapons.Melee;
using Origins.NPCs.Fiberglass;
using Origins.Projectiles;
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
	public class Shotgunfish : Brine_Pool_NPC {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.npcFrameCount[NPC.type] = 8;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Velocity = 1f
			};
		}
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
		public override void AI() {
			DoTargeting();
			Vector2 direction;
			if (NPC.wet) {
				bool flipTargetRotation = false;
				NPC.noGravity = true;
				if (TargetPos != default) {
					if (!NPC.WithinRange(TargetPos, 16 * 12) || (targetIsRipple && NPC.ai[1] > 0)) {
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
					if (NPC.ai[1] > 0) {
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
										40,
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
					}
				} else {
					NPC.ai[0] = 0;
					if (NPC.collideX) NPC.velocity.X = -NPC.direction;
					NPC.direction = Math.Sign(NPC.velocity.X);
					if (NPC.direction == 0) NPC.direction = 1;
					direction = Vector2.UnitX * NPC.direction;
				}
				if (NPC.ai[3] > 150) {
					float dist = 16 * 25;
					int mossType = ModContent.TileType<Peat_Moss>();
					for (int i = -3; i < 4; i++) {
						Vector2 dir = Vector2.UnitX.RotatedBy(NPC.rotation + i * 0.5f);
						float newDist = CollisionExt.Raymarch(NPC.Center, dir, dist);
						if (newDist < dist && Framing.GetTileSafely(NPC.Center + dir * dist * 2).TileIsType(mossType)) {
							dist = newDist;
							direction = dir;
						}
					}
					if (dist < 28) {
						NPC.ai[3] = 0;
						if (NPC.ai[1] < 3) NPC.ai[1]++;
					}
				} else {
					NPC.ai[3]++;
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
			float frame = (NPC.IsABestiaryIconDummy ? (float)++NPC.frameCounter : NPC.ai[2]) / 45;
			if (NPC.frameCounter >= 40) NPC.frameCounter = 0;
			NPC.frame = new Rectangle(0, 32 * (int)(frame * Main.npcFrameCount[Type]), 62, 30);
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				/*Origins.instance.SpawnGoreByName(
					NPC.GetSource_Death(),
					NPC.Center,
					Vector2.Zero,
					$"Gores/NPC/{nameof(Shotgunfish)}_Gore_3"
				);
				Origins.instance.SpawnGoreByName(
					NPC.GetSource_Death(),
					NPC.Center + GeometryUtils.Vec2FromPolar(-16, NPC.rotation),
					Vector2.Zero,
					$"Gores/NPC/{nameof(Shotgunfish)}_Gore_2"
				);
				Origins.instance.SpawnGoreByName(
					NPC.GetSource_Death(),
					NPC.Center + GeometryUtils.Vec2FromPolar(-32, NPC.rotation),
					Vector2.Zero,
					$"Gores/NPC/{nameof(Shotgunfish)}_Gore_1"
				);*/
			}
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			SpriteEffects spriteEffects = SpriteEffects.FlipHorizontally;
			if (NPC.spriteDirection != 1) {
				spriteEffects |= SpriteEffects.FlipVertically;
			}
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
