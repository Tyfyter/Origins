using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.World.BiomeData;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Riven.World_Cracker {
	public class World_Cracker_Exoskeleton : Glowing_Mod_NPC, IRivenEnemy, IWikiNPC {
		public Rectangle DrawRect => new(0, 0, 50, 26);
		public int AnimationFrames => 3;
		public int FrameDuration => 6;
		public NPCExportType ImageExportType => NPCExportType.SpriteSheet;
		public AssimilationAmount? Assimilation => 0.03f;
		public override string Texture => "Origins/Items/Weapons/Summoner/Minions/Flying_Exoskeleton";
		public virtual void SafeSetStaticDefaults() { }
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 3;
			NPCID.Sets.NPCBestiaryDrawOffset[NPC.type] = NPCExtensions.HideInBestiary;
			SafeSetStaticDefaults();
			ContentSamples.NpcBestiaryRarityStars[Type] = 3;
			NPCID.Sets.DontDoHardmodeScaling[NPC.type] = true;
		}

		public override void SetDefaults() {
			NPC.aiStyle = NPCAIStyleID.ActuallyNone;
			NPC.lifeMax = 30;
			NPC.damage = 11;
			NPC.width = 28;
			NPC.height = 28;
			NPC.noTileCollide = true;
			NPC.noGravity = true;
			NPC.npcSlots = 0;
			NPC.scale = 1.15f;
			NPC.HitSound = SoundID.NPCHit2;
			NPC.DeathSound = SoundID.NPCDeath2;
			SpawnModBiomes = [
				ModContent.GetInstance<Riven_Hive>().Type
			];
		}
		public override bool CheckActive() {
			int headType = ModContent.NPCType<World_Cracker_Head>();
			int bodyType = ModContent.NPCType<World_Cracker_Body>();
			int tailType = ModContent.NPCType<World_Cracker_Tail>();
			foreach (NPC npc in Main.ActiveNPCs) {
				if (npc.type == headType || npc.type == bodyType || npc.type == tailType) {
					return false;
				}
			}
			NPC.EncourageDespawn(5);
			return true;
		}
		public override void BossHeadSlot(ref int index) {
		}
		public override void AI() {
			List<NPC> owners = new(World_Cracker_Head.DifficultyScaledSegmentCount);
			NPC head = null;
			int headType = ModContent.NPCType<World_Cracker_Head>();
			int bodyType = ModContent.NPCType<World_Cracker_Body>();
			int tailType = ModContent.NPCType<World_Cracker_Tail>();
			foreach (NPC npc in Main.ActiveNPCs) {
				if (npc.type == headType) {
					head = npc;
					owners.Add(npc);
				} else if (npc.type == bodyType || npc.type == tailType) {
					owners.Add(npc);
				}
			}
			#region Find target
			// Starting search distance
			float targetDist = 640f;
			float targetAngle = -2;
			Vector2 targetCenter = head?.Center ?? NPC.Center;
			int target = -1;
			bool foundTarget = false;
			foreach (Player player in Main.ActivePlayers) {
				if (player.dead) continue;
				Vector2 diff = player.Center - NPC.Center;
				float dist = diff.Length();
				if (dist > targetDist) continue;
				float dot = OriginExtensions.NormDotWithPriorityMult(diff, NPC.velocity, 1f);
				if (owners.Count > 0) dot -= (owners.Min(owner => owner.DistanceSQ(player.Center)) / (640 * 640));
				bool inRange = dist <= targetDist;
				bool lineOfSight = Collision.CanHitLine(NPC.position, NPC.width, NPC.height, player.position, player.width, player.height);
				if (
					((dot >= targetAngle && inRange) || !foundTarget) &&
					(player.whoAmI == head?.target || lineOfSight || player.whoAmI == NPC.ai[0])
					) {
					targetDist = dist;
					targetAngle = dot;
					targetCenter = player.Center;
					target = player.whoAmI;
					foundTarget = true;
				}
			}
			#endregion

			#region Movement
			// Default movement parameters (here for attacking)
			if (NPC.justHit) NPC.ai[1] = 0;
			float speed = 5 + World_Cracker_Head.DifficultyMult;
			float turnSpeed = 0.6f + Math.Min(NPC.ai[1] / 180f, World_Cracker_Head.DifficultyMult / 6f);
			float currentSpeed = NPC.velocity.Length();
			if (foundTarget) {
				if (NPC.ai[0] != target) {
					NPC.ai[0] = target;
					NPC.ai[1] = 0;
				} else {
					if (++NPC.ai[1] > 180) {
						NPC.ai[1] = -30;
					}
				}
				if ((int)Math.Ceiling(targetAngle) == -1) {
					targetCenter.Y -= 16;
				}
			}
			MathUtils.LinearSmoothing(ref currentSpeed, speed, (currentSpeed < 1 || currentSpeed > speed) ? 1 : 0.1f);
			Vector2 direction = targetCenter - NPC.Center;
			if (direction != Vector2.Zero) {
				direction.Normalize();
				NPC.velocity = Vector2.Normalize(NPC.velocity + direction * turnSpeed) * currentSpeed;
			}
			#endregion

			#region Animation and visuals

			NPC.rotation = (float)Math.Atan(NPC.velocity.Y / NPC.velocity.X);
			NPC.spriteDirection = Math.Sign(NPC.velocity.X);

			NPC.DoFrames(6);
			#endregion
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
			NPC.ai[1] = 0;
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Texture2D texture = TextureAssets.Npc[Type].Value;
			Main.EntitySpriteDraw(
				texture,
				NPC.Center - screenPos,
				NPC.frame,
				drawColor,
				NPC.rotation,
				new Vector2(25 + 15 * NPC.direction, 16),
				NPC.scale,
				NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None
			);
			Color lightColor = Lighting.GetColor(NPC.Center.ToTileCoordinates());
			Color glowColor;
			if (NPC.IsABestiaryIconDummy) {
				glowColor = Color.White;
			} else {
				glowColor = new Color((lightColor.R + 255) / 510f, (lightColor.G + 255) / 510f, (lightColor.B + 255) / 510f, 0.5f);
				NPCLoader.DrawEffects(NPC, ref glowColor);
				glowColor = NPC.GetNPCColorTintedByBuffs(glowColor);
			}
			Main.EntitySpriteDraw(
				GlowTexture,
				NPC.Center - screenPos,
				NPC.frame,
				glowColor,
				NPC.rotation,
				new Vector2(25 + 15 * NPC.direction, 16),
				NPC.scale,
				NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None
			);
			return false;
		}
		public override void HitEffect(NPC.HitInfo hit) {
			//spawn gore if npc is dead after being hit
			if (NPC.life <= 0) {
				for (int i = 0; i < 3; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4));
				for (int i = 0; i < 4; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Meat1");
			}
		}
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) { }
	}
}
