using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using Origins.Gores;
using PegasusLib;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.MiscE {
	public class Cannihound : ModNPC, IWikiNPC {
		public Rectangle DrawRect => new(0, 0, 82, 50);
		public int AnimationFrames => 120;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		private readonly List<Projectile> goreList = [];
		private Projectile? projTgt;
		private const int jumpDst = 230;
		private const int healAmt = 20;
		public bool wasHit;
		public bool turning;
		public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 18;
			NPCID.Sets.UsesNewTargetting[Type] = true;
			CrimsonGlobalNPC.NPCTypes.Add(Type);
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft;
			AssimilationLoader.AddNPCAssimilation<Crimson_Assimilation>(Type, 0.04f);
		}
		public override void SetDefaults() {
			NPC.aiStyle = NPCAIStyleID.Fighter;
			NPC.lifeMax = 198;
			NPC.defense = 20;
			NPC.damage = 25;
			NPC.width = 54;
			NPC.height = 50;
			NPC.friendly = false;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.knockBackResist = 0.3f;
			NPC.value = 75;
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (spawnInfo.PlayerFloorY > Main.worldSurface + 50 || spawnInfo.SpawnTileY >= Main.worldSurface - 50) return 0;
			if (!spawnInfo.Player.ZoneCrimson) return 0;
			return 0.1f * (spawnInfo.Player.ZoneSkyHeight ? 2 : 1) * (spawnInfo.Player.ZoneSnow ? 1.5f : 1);
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
			if (Main.rand.NextBool()) target.AddBuff(BuffID.Bleeding, 20);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit) {
			if (Main.rand.NextBool()) target.AddBuff(BuffID.Bleeding, 20);
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText(),
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheCrimson
			);
		}
		public override bool ModifyCollisionData(Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox) {
			if (NPC.ai[3] > 0) damageMultiplier *= 1.1f;
			return true;
		}
		public override void UpdateLifeRegen(ref int damage) {
			if (NPC.ai[3] > 0) NPC.lifeRegen += 4;
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ItemID.Vertebrae));
		}
		public override bool CanHitNPC(NPC target) {
			return NPC.frame.Y / NPC.frame.Height == 12;
		}
		public override bool CanHitPlayer(Player target, ref int cooldownSlot) {
			return NPC.frame.Y / NPC.frame.Height == 12;
		}
		public override bool PreAI() {
			NPCAimedTarget tgt = NPC.GetTargetData();
			switch (NPC.aiAction) {
				case 0:
				if (NPC.ai[1]-- <= 0 && NPC.collideY) {
					if (NPC.GetLifePercent() <= 0.35f || NPC.ai[2] > 0) {
						if (goreList.Count > 0) projTgt = goreList[Main.rand.Next(0, goreList.Count - 1)];
						if (projTgt is not null && NPC.Center.Distance(projTgt.Center) <= jumpDst && projTgt.active) {
							NPC.aiAction = 2; // jump to food action
							NPC.ai[0] = 43; // time in ticks for the anim of the jump
							NPC.ai[2] = 180; // time in ticks to be looking for cannihound gores to consume
							Main.NewText(NPC.Center.Distance(projTgt.Center));
							NPC.velocity.X = 0;
							return false;
						} else if (projTgt is null || !projTgt.active) {
							goreList.Remove(projTgt);
							goto default;
						}
					}
					if (NPC.Center.Distance(tgt.Center) <= jumpDst && NPC.HasValidTarget) {
						NPC.aiAction = 1; // jump to target action
						NPC.ai[0] = 43; // time in ticks for the anim of the jump
						Main.NewText(NPC.Center.Distance(tgt.Center));
						NPC.velocity.X = 0;
						return false;
					}
				}
				break;
				case 1:
				if (NPC.ai[0]-- == 16) {
					Vector2 pos = tgt.Center - new Vector2(0, tgt.Height / 2);
					Vector2 velocity;
					float speed = 15;
					if (GeometryUtils.AngleToTarget(pos - NPC.Center, speed, 0.2f, false) is float angle) {
						velocity = angle.ToRotationVector2() * speed;
					} else {
						float val = 0.70710678119f;
						velocity = new Vector2(val * NPC.direction, -val) * speed;
					}
					NPC.velocity = velocity;
				}
				if (NPC.ai[0] <= 5 && (NPC.collideX || NPC.collideY || NPC.Center.Distance(tgt.Center) >= 230 || NPC.getRect().Intersects(tgt.Hitbox))) {
					NPC.aiAction = 0;
					NPC.ai[1] = 60; // time in ticks before can jump again
				}
				return false;
				case 2:
				if (NPC.ai[0]-- == 16) {
					Vector2 pos = projTgt.Center - new Vector2(0, projTgt.height / 2);
					Vector2 velocity;
					float speed = 15;
					if (GeometryUtils.AngleToTarget(pos - NPC.Center, speed, 0.2f, false) is float angle) {
						velocity = angle.ToRotationVector2() * speed;
					} else {
						float val = 0.70710678119f;
						velocity = new Vector2(val * NPC.direction, -val) * speed;
					}
					NPC.velocity = velocity;
				}
				if (NPC.ai[0] <= 5 && (NPC.collideX || NPC.collideY || NPC.Center.Distance(projTgt.Center) >= 230 || NPC.getRect().Intersects(projTgt.Hitbox))) {
					if (NPC.getRect().Intersects(projTgt.Hitbox)) {
						int trueHealAmt = healAmt;
						if (NPC.life + healAmt > NPC.lifeMax) {
							trueHealAmt = NPC.lifeMax - NPC.life;
							if (NPC.ai[3] <= 0) NPC.ai[3] = 3 * 60; // time in ticks for the cannihound buffs
							else NPC.ai[3] += healAmt / 2;
						}
						NPC.life = Math.Max(NPC.life + trueHealAmt, NPC.lifeMax);
						NPC.HealEffect(trueHealAmt);
						NPC.netUpdate = true;
					}
					NPC.aiAction = 0;
					NPC.ai[1] = 60;
				}
				return false;
				default: {
					foreach (Projectile proj in Main.projectile) {
						if (proj?.ModProjectile is GoreProjectile && !goreList.Contains(proj) && !proj.active) goreList.Add(proj);
					}
					break;
				}
			}
			NPC.ai[2]--;
			return true;
		}
		public override void AI() {
			NPC.TargetClosest();
			NPCAimedTarget tgt = NPC.GetTargetData();
			if (!tgt.Invalid) NPC.velocity.X += NPC.DirectionTo(tgt.Center).X >= 0 ? 3 : -3;
			else NPC.velocity.X += 1 * NPC.direction;
			NPC.velocity.X = Math.Clamp(NPC.velocity.X, -6, 6);
			NPC.spriteDirection = NPC.direction;
		}
		public override void FindFrame(int frameHeight) {
			float speed = Math.Abs(NPC.velocity.X);
			if (speed > 5) speed = 2;
			else if (speed > 2) speed = 1;
			if (NPC.aiAction == 0) {
				if (NPC.ai[1] > 0) NPC.DoFrames(6, (NPC.frame.Y / NPC.frame.Height)..15);
				else if (turning && !wasHit) NPC.DoFrames(4, 16..18);
				else NPC.DoFrames(4 - (int)speed, 0..5);
			}
			if ((NPC.aiAction == 1 || NPC.aiAction == 2) && NPC.ai[0] >= 16) NPC.DoFrames(4, 6..15);
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				const float spread = 2;
				SoundEngine.PlaySound(SoundID.NPCDeath1, NPC.Center);
				Projectile.NewProjectileDirect(
					NPC.GetSource_Death(),
					NPC.Center + new Vector2(22 * NPC.direction, -4),
					NPC.velocity + Main.rand.NextVector2Circular(spread, spread),
					ModContent.ProjectileType<Cannihound_Gore1>(),
					0,0
				);
				Projectile.NewProjectileDirect(
					NPC.GetSource_Death(),
					NPC.Center + new Vector2(-10 * NPC.direction, 11),
					NPC.velocity + Main.rand.NextVector2Circular(spread, spread),
					ModContent.ProjectileType<Cannihound_Gore2>(),
					0,0
				);
				Projectile.NewProjectileDirect(
					NPC.GetSource_Death(),
					NPC.Center + new Vector2(-18 * NPC.direction, 19),
					NPC.velocity + Main.rand.NextVector2Circular(spread, spread),
					ModContent.ProjectileType<Cannihound_Gore3>(),
					0,0
				);
				for (int i = Main.rand.Next(16, 25); i >= 0; i--) {
					Dust.NewDustDirect(
						NPC.position + Vector2.One * 2,
						NPC.width - 4,
						NPC.height - 4,
						DustID.Blood,
						NPC.velocity.X,
						NPC.velocity.Y
					);
				}
			}
		}
	}
}
