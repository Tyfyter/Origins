using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Core;
using Origins.Dev;
using Origins.Items.Accessories;
using Origins.Items.Materials;
using Origins.Items.Other.Consumables;
using Origins.Tiles;
using Origins.World.BiomeData;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.OriginExtensions;

namespace Origins.NPCs.Defiled {
	public class Defiled_Tripod : Glowing_Mod_NPC, ICustomCollisionNPC, IDefiledEnemy, IWikiNPC, ICustomWikiStat {
		public Rectangle DrawRect => new(0, 4, 98, 100);
		public int AnimationFrames => 24;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		public AssimilationAmount? Assimilation => 0.04f;
		public const float horizontalSpeed = 3.2f;
		public const float horizontalAirSpeed = 2f;
		public const float verticalSpeed = 4f;
		static Asset<Texture2D> glowTexture;
		public override Texture2D GlowTexture => glowTexture.Value;
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 4;
			NPCID.Sets.TrailCacheLength[NPC.type] = 4;
			NPCID.Sets.TrailingMode[NPC.type] = 1;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Scale = 0.6f,
				PortraitScale = 1f,
				Velocity = 2f,
			};
			ModContent.GetInstance<Defiled_Wastelands.SpawnRates>().AddSpawn(Type, SpawnChance);
			AprilFoolsTextures.AddNPC(this);
			AprilFoolsTextures.Create(() => ref glowTexture, Texture + "_Glow");
		}
		public bool? Hardmode => true;
		public override void SetDefaults() {
			NPC.aiStyle = NPCAIStyleID.None;//NPCAIStyleID.Fighter;
			NPC.lifeMax = 475;
			NPC.defense = 28;
			NPC.damage = 52;
			NPC.width = 56;
			NPC.height = 90;
			NPC.scale = 0.85f;
			NPC.friendly = false;
			NPC.HitSound = OriginsModIntegrations.CheckAprilFools() ? SoundID.Meowmere : Origins.Sounds.DefiledHurt;
			NPC.DeathSound = Origins.Sounds.DefiledKill;
			NPC.value = 2300;
			NPC.knockBackResist = 0.5f;
			SpawnModBiomes = [
				ModContent.GetInstance<Defiled_Wastelands>().Type,
				ModContent.GetInstance<Underground_Defiled_Wastelands_Biome>().Type
			];
			this.CopyBanner<Defiled_Banner_NPC>();
		}
		public int MaxMana => 160;
		public int MaxManaDrain => 32;
		public float Mana { get; set; }
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
			target.AddBuff(ModContent.BuffType<Rasterized_Debuff>(), 100);
		}
		public void Regenerate(out int lifeRegen) {
			int factor = 20;
			lifeRegen = factor;
			Mana -= factor / 60f;// 2 mana for every 1 health regenerated
		}
		public new static float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (spawnInfo.PlayerSafe || spawnInfo.DesertCave || spawnInfo.SpawnTileY <= Main.worldSurface) return 0;
			return Defiled_Wastelands.SpawnRates.LandEnemyRate(spawnInfo, true) * Defiled_Wastelands.SpawnRates.Tripod;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.StatusImmunityItem(ItemID.Vitamins, 100));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Strange_String>(), 1, 1, 3));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Latchkey>(), 5, 3, 7));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Black_Bile>(), 1, 1, 3));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Tripod_Nip>(), 48));
		}
		public override void AI() {
			if (Main.rand.NextBool(400)) SoundEngine.PlaySound(Origins.Sounds.DefiledIdle, NPC.Center);
			if (Main.rand.NextBool(1600)) SoundEngine.PlaySound(SoundID.Zombie124, NPC.Center);
			NPC.TargetClosest();
			if (NPC.HasPlayerTarget) {
				NPC.spriteDirection = NPC.direction;
			}

			NPC.velocity = NPC.oldVelocity * new Vector2(NPC.collideX ? 0.5f : 1, NPC.collideY ? 0.75f : 1);
			int targetVMinDirection = Math.Sign(NPC.targetRect.Bottom - NPC.Bottom.Y);
			int targetVMaxDirection = Math.Sign(NPC.targetRect.Top - NPC.Bottom.Y);
			if (NPC.collideY && targetVMaxDirection == 1) {
				NPC.position.Y++;
			}
			float moveDir = Math.Sign(NPC.velocity.X);
			float absX = moveDir == 0 ? 0 : NPC.velocity.X / moveDir;

			if (NPC.collideY) {
				NPC.ai[1] = 0;
				//npc.rotation = 0;
				LinearSmoothing(ref NPC.rotation, 0, 0.15f);
				if (moveDir != -NPC.direction) {
					if (absX < horizontalSpeed) {
						NPC.velocity.X += NPC.direction * 0.5f;
					} else {
						LinearSmoothing(ref NPC.velocity.X, NPC.direction * horizontalSpeed, 0.1f);
					}
				} else {
					NPC.velocity.X += NPC.direction * 0.15f;
				}
				if (NPC.ai[0] > 0) {
					NPC.ai[0]--;
				} else {
					if (NPC.collideX) {
						if (NPC.targetRect.Bottom > NPC.Top.Y) {
							//npc.velocity.X *= 2;
							NPC.position.X += NPC.direction;
						} else {
							if (NPC.velocity.Y > -4) {
								NPC.velocity.Y -= 1;
							}
							if (moveDir != -NPC.direction) {
								LinearSmoothing(ref NPC.rotation, -MathHelper.PiOver2 * moveDir, 0.15f);
								//npc.rotation = -MathHelper.PiOver2 * moveDir;
								if (targetVMinDirection == -1) {
									NPC.position.Y--;
								}
							}
						}
					} else if (moveDir == NPC.direction && absX > 3) {
						float dist = NPC.targetRect.Distance(NPC.Center);
						if (dist > 96 && dist < 240) {
							NPC.velocity.X += moveDir * 4;
							NPC.velocity.Y -= (NPC.Center.Y - NPC.targetRect.Center.Y > 80) ? 8 : 4f;
							NPC.ai[0] = 35;
						}
					}
				}
			} else if (NPC.collideX) {
				NPC.ai[1] = 0;
				if (targetVMinDirection == -1) {
					if (NPC.velocity.Y > -verticalSpeed) NPC.velocity.Y -= 1;
					if (moveDir != -NPC.direction) {
						LinearSmoothing(ref NPC.rotation, -MathHelper.PiOver2 * moveDir, 0.15f);
						//npc.rotation = -MathHelper.PiOver2 * moveDir;
					}
				} else {
					//npc.velocity.X *= 2;
					NPC.position.X += NPC.direction;
				}
			} else {
				if (++NPC.ai[1] > 1) LinearSmoothing(ref NPC.rotation, 0, 0.15f);
				if (moveDir != -NPC.direction) {
					if (absX < horizontalAirSpeed) NPC.velocity.X += NPC.direction * 0.2f;
				}
			}
		}
		public override void FindFrame(int frameHeight) {
			if (NPC.velocity.RotatedBy(-NPC.rotation).X * NPC.direction > 0.5f && ++NPC.frameCounter > 6) {
				//add frame height to frame y position and modulo by frame height multiplied by walking frame count
				NPC.frame = new Rectangle(0, (NPC.frame.Y + 100) % 400, 98, 98);
				NPC.frameCounter = 0;
				if (NPC.collideY) {
					Vector2 stepPos = new Vector2(NPC.spriteDirection * -45, 50).RotatedBy(NPC.rotation) + NPC.Center;
					SoundEngine.PlaySound(SoundID.MenuTick.WithPitchRange(-0.2f, 0.2f).WithVolume(Main.rand.NextFloat(0.7f, 0.95f)), stepPos);
				}
			}
		}
		public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone) {
			Rectangle spawnbox = projectile.Hitbox.MoveToWithin(NPC.Hitbox);
			for (int i = Main.rand.Next(3); i-- > 0;) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVectorIn(spawnbox), projectile.velocity, "Gores/NPCs/DF_Effect_Small" + Main.rand.Next(1, 4));
		}
		public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone) {
			int halfWidth = NPC.width / 2;
			int baseX = player.direction > 0 ? halfWidth : 0;
			for (int i = Main.rand.Next(3); i-- > 0;) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(baseX + Main.rand.Next(halfWidth), Main.rand.Next(NPC.height)), hit.GetKnockbackFromHit(), "Gores/NPCs/DF_Effect_Small" + Main.rand.Next(1, 4));
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				for (int i = 0; i < 3; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/DF3_Gore");
				for (int i = 0; i < 6; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/DF_Effect_Medium" + Main.rand.Next(1, 4));
			}
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(Mana);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			Mana = reader.ReadSingle();
		}
		bool phasing = false;
		List<int> alteredTiles;
		public void PreUpdateCollision() {
			if (phasing = (NPC.targetRect.Top - NPC.Bottom.Y > 0 || !NPC.collideY)) {
				if (alteredTiles is null) {
					alteredTiles = [];
					for (int i = 0; i < OriginTile.DefiledTiles.Count; i++) {
						int tile = OriginTile.DefiledTiles[i];
						if (Main.tileSolid[tile] && !Main.tileSolidTop[tile]) {
							alteredTiles.Add(tile);
						}
					}
				}
				for (int i = 0; i < alteredTiles.Count; i++) {
					Main.tileSolidTop[alteredTiles[i]] = true;
					Main.tileSolid[alteredTiles[i]] = false;
				}
			}
		}
		public void PostUpdateCollision() {
			if (phasing) {
				for (int i = 0; i < alteredTiles.Count; i++) {
					Main.tileSolidTop[alteredTiles[i]] = false;
					Main.tileSolid[alteredTiles[i]] = true;
				}
			}
		}
	}
}
