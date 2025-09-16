using Origins.Gores.NPCs;
using PegasusLib;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.MiscE {
	public class Creme_Filled_Bat : ModNPC {
		public override void SetStaticDefaults() {
			NPCID.Sets.ShimmerTransformToNPC[NPCID.GiantFlyingFox] = Type;
			Main.npcFrameCount[Type] = 5;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new() {
				PortraitPositionYOverride = -20,
				Velocity = 1
			};
		}
		public override void SetDefaults() {
			NPC.width = 38;
			NPC.height = 34;
			NPC.aiStyle = NPCAIStyleID.Bat;
			NPC.damage = 80;
			NPC.defense = 24;
			NPC.lifeMax = 220;
			NPC.HitSound = SoundID.NPCHit1.WithPitch(-1);
			NPC.knockBackResist = 1;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.value = Item.buyPrice(gold: 2);
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText(alt: true),
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Caverns
			);
		}
		public override void AI() {
			if (Main.rand.NextBool(250)) SoundEngine.PlaySound(Origins.Sounds.BikeHorn, NPC.Center);
		}
		public override bool PreAI() {
			if (NPC.ai[3] > 0) {
				NPC.immortal = true;
				NPC.life = NPC.lifeMax;
				NPC.velocity.Y += 0.16f;
				NPC.rotation += NPC.ai[0] * 0.125f;
				NPC.ai[0] *= 0.99f;
				if (NPC.collideY) {
					if (float.IsNaN(NPC.ai[0])) NPC.ai[0] = 0;
					if (Math.Sign(NPC.velocity.X) != Math.Sign(NPC.ai[0]) || Math.Abs(NPC.velocity.X) < Math.Abs(NPC.ai[0] * 2)) {
						NPC.velocity.X += NPC.ai[0] * 0.125f;
						NPC.ai[0] *= 0.97f;
					}
					if (NPC.collideX) {
						NPC.ai[0] *= 0.5f;
					}
					NPC.velocity *= 0.98f;
				}
				if (NPC.localAI[0].Warmup(2)) {
					NPC.localAI[0] = 0;
					{
						Vector2 diff = new Vector2(2, -7).RotatedBy(NPC.rotation);
						Gore.NewGore(NPC.GetSource_Death(), NPC.Center + diff, diff + NPC.velocity, Main.rand.Next(R_Effect_Blood1.GoreIDs));
					}
					if (NPC.ai[3] >= 2) {
						Vector2 diff = new Vector2(1, 5).RotatedBy(NPC.rotation);
						Gore.NewGore(NPC.GetSource_Death(), NPC.Center + diff, diff + NPC.velocity, Main.rand.Next(R_Effect_Blood1.GoreIDs));
					}
					if (NPC.ai[3] >= 3) {
						Vector2 diff = new Vector2(7, -1).RotatedBy(NPC.rotation);
						Gore.NewGore(NPC.GetSource_Death(), NPC.Center + diff, diff + NPC.velocity, Main.rand.Next(R_Effect_Blood1.GoreIDs));
					}
					if (NPC.ai[3] >= 4) {
						Vector2 diff = new Vector2(-7, -1).RotatedBy(NPC.rotation);
						Gore.NewGore(NPC.GetSource_Death(), NPC.Center + diff, diff + NPC.velocity, Main.rand.Next(R_Effect_Blood1.GoreIDs));
					}
					if (NPC.ai[3] >= 5) {
						Vector2 diff = new Vector2(7, -9).RotatedBy(NPC.rotation);
						Gore.NewGore(NPC.GetSource_Death(), NPC.Center + diff, diff + NPC.velocity, Main.rand.Next(R_Effect_Blood1.GoreIDs));
					}
				}
				if (NPC.ai[3] >= 6) {
					NPC.immortal = false;
					if (!NetmodeActive.MultiplayerClient) NPC.StrikeInstantKill();
				}
				return false;
			}
			NPC.spriteDirection = NPC.direction;
			NPC.rotation = NPC.velocity.X * 0.1f;
			return base.PreAI();
		}
		public override void FindFrame(int frameHeight) {
			if (NPC.ai[3] <= 0) {
				NPC.DoFrames(6);
				if (NPC.frameCounter == 0 && NPC.frame.Y / frameHeight == 3) {
					//SoundEngine.PlaySound(NPC.HitSound, NPC.Center);
					SoundEngine.PlaySound(Origins.Sounds.BoneBreakBySoundEffectsFactory, NPC.Center);
					//SoundEngine.PlaySound(SoundID.NPCHit2.WithPitch(-1), NPC.Center);
				}
			}
		}
		public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) {
			return base.DrawHealthBar(hbPosition, ref scale, ref position);
		}
		public override bool CheckDead() {
			if (NPC.ai[3] <= 0) {
				NPC.ai[3] = 1;
				NPC.life = NPC.lifeMax;
			}
			return NPC.ai[3] >= 6;
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {

		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				if (NPC.ai[3] > 0) {
					for (int i = 0; i < 100; i++) {
						Gore.NewGore(NPC.GetSource_Death(), NPC.Center, Main.rand.NextVector2CircularEdge(1, 1) * Main.rand.NextFloat(1, 8) + NPC.velocity, Main.rand.Next(R_Effect_Blood1.GoreIDs));
					}
					SoundEngine.PlaySound(NPC.DeathSound, NPC.Center);
				} else {
					NPC.ai[0] = hit.Knockback * hit.HitDirection;
					SoundEngine.PlaySound(SoundID.NPCDeath4, NPC.Center);
				}
			} else if (NPC.ai[3] > 0) {
				if (NPC.ai[0] * hit.HitDirection < hit.Knockback) {
					NPC.ai[0] = hit.Knockback * hit.HitDirection;
				}
				NPC.DoFrames(1, 0..4);
				NPC.ai[3]++;
			}
			Gore.NewGore(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, Main.rand.Next(R_Effect_Blood1.GoreIDs));
		}
	}
}
