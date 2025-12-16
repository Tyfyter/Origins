using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.NPCs.Critters;
using Origins.World.BiomeData;
using PegasusLib;
using ReLogic.Content;
using System;
using System.IO;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Riven {
	public class Barnacle_Mound : ModNPC, IRivenEnemy, IWikiNPC, ICustomWikiStat {
		public override void Load() => this.AddBanner();
		private Asset<Texture2D> _glowTexture;
		public Texture2D GlowTexture => (_glowTexture ??= (ModContent.RequestIfExists<Texture2D>(Texture + "_Glow", out Asset<Texture2D> asset) ? asset : null))?.Value;
		public Rectangle DrawRect => new(0, -41, 34, 22);
		public int AnimationFrames => 1;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 13;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Position = new(0, 20),
				PortraitPositionYOverride = 40
			};
			ModContent.GetInstance<Riven_Hive.SpawnRates>().AddSpawn(Type, SpawnChance);
		}
		public bool? Hardmode => true;
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.BloodJelly);
			NPC.aiStyle = Terraria.ID.NPCAIStyleID.FaceClosestPlayer;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.lifeMax = 90;
			NPC.defense = 18;
			NPC.damage = 0;
			NPC.width = 24;
			NPC.height = 24;
			NPC.knockBackResist = 0;
			NPC.value = 100;
			SpawnModBiomes = [
				ModContent.GetInstance<Riven_Hive>().Type,
				ModContent.GetInstance<Underground_Riven_Hive_Biome>().Type
			];
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Bud_Barnacle>(), 1, 3, 8));
		}
		public override void OnKill() {
			if (Main.rand.NextBool(4, 7)) {
				int type = ModContent.NPCType<Amoeba_Buggy>();
				for (int i = Main.rand.Next(1, 3); i-- > 0;) {
					NPC.NewNPCDirect(NPC.GetSource_FromAI(), (int)NPC.position.X, (int)NPC.position.Y, type);
				}
			}
		}
		public override int SpawnNPC(int tileX, int tileY) {
			tileY = OriginGlobalNPC.GetAerialSpawnPosition(tileX, tileY, this);
			if (tileY == -1) return Main.maxNPCs;
			return NPC.NewNPC(null, tileX * 16 + 8, tileY * 16, NPC.type);
		}
		public override void AI() {
			if (Main.netMode == NetmodeID.MultiplayerClient) return;
			if (NPC.ai[1] == 0) {
				NPC.ai[1] = 1;
				const float offsetLen = 10;
				Vector2 bestPosition = default;
				float dist = 800;
				Vector2[] directions = [
					Vector2.UnitX,
					-Vector2.UnitX,
					Vector2.UnitY,
					-Vector2.UnitY
				];
				for (int i = 0; i < directions.Length; i++) {
					float newDist = CollisionExt.Raymarch(NPC.Center, directions[i], dist);
					if (newDist < dist) {
						dist = newDist;
						bestPosition = NPC.Center + directions[i] * (dist - offsetLen);
						NPC.rotation = directions[i].ToRotation() - MathHelper.PiOver2;
					}
				}
				NPC.Center = bestPosition;
				NPC.oldVelocity = Vector2.Zero;
				NPC.oldPosition = NPC.position;
				NPC.netUpdate = true;
			} else {
				NPC.position = NPC.oldPosition;
			}
			NPC.TargetClosest(faceTarget: false);
			int spawnTime = (Main.masterMode ? 420 : (Main.expertMode ? 540 : 600)) + (int)NPC.ai[2];
			if (Main.netMode != NetmodeID.MultiplayerClient && NPC.HasValidTarget && ++NPC.ai[0] > spawnTime) {
				int type = ModContent.NPCType<Amoeba_Bugger>();
				NPC.ai[0] = 0;
				NPC.ai[2] = 0;
				for (int i = Main.rand.Next(4, 7); i-- > 0;) {
					NPC.NewNPCDirect(NPC.GetSource_FromAI(), (int)NPC.position.X, (int)NPC.position.Y, type, ai0: Main.rand.NextFloat(-4, 4), ai1: Main.rand.NextFloat(-4, 4));
					NPC.ai[2] += 30;
				}
			}
			const float frame_time = 7;
			if (NPC.ai[0] < frame_time * 6) {
				NPC.frame.Y = ((int)(NPC.ai[0] / frame_time) + 7) * NPC.frame.Height;
			} else {
				float startTime = spawnTime - frame_time * 7;
				if (NPC.ai[0] >= startTime) {
					NPC.frame.Y = ((int)((NPC.ai[0] - startTime) / frame_time)) * NPC.frame.Height;
				}
				if (NPC.frame.Y < 0) {

				}
			}
			NPC.velocity = Vector2.Zero;
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write((sbyte)Math.Round(NPC.rotation / MathHelper.PiOver2));
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			NPC.rotation = reader.ReadSByte() * MathHelper.PiOver2;
		}
		public new static float SpawnChance(NPCSpawnInfo spawnInfo) {
			return Riven_Hive.SpawnRates.FlyingEnemyRate(spawnInfo, true) * Riven_Hive.SpawnRates.Barnacle;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers) {
			modifiers.DisableKnockback();
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Vector2 halfSize = new(23, 32);
			Vector2 position = NPC.Center + new Vector2(0, 12).RotatedBy(NPC.rotation);
			spriteBatch.Draw(
				TextureAssets.Npc[Type].Value,
				position - screenPos,
				NPC.frame,
				drawColor,
				NPC.rotation,
				halfSize,
				NPC.scale,
				SpriteEffects.None,
			0);
			if (GlowTexture is not null) {
				spriteBatch.Draw(
					GlowTexture,
					position - screenPos,
					NPC.frame,
					Riven_Hive.GetGlowAlpha(drawColor),
					NPC.rotation,
					halfSize,
					NPC.scale,
					SpriteEffects.None,
				0);
			}
			return false;
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				for (int i = 0; i < 3; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4));
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Meat" + Main.rand.Next(1, 4));
			}
		}
	}
}
