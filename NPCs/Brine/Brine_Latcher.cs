using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.World.BiomeData;
using PegasusLib;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Brine {
	public class Brine_Latcher_Swarm : Brine_Pool_NPC, ICustomWikiStat {
		public override string Texture => typeof(Brine_Latcher).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.HideInBestiary;
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			return Brine_Pool.SpawnRates.EnemyRate(spawnInfo, Brine_Pool.SpawnRates.Swarm);
		}
		public override void OnSpawn(IEntitySource source) {
			NPC.active = false;
			if (Main.netMode != NetmodeID.MultiplayerClient) {
				int type = ModContent.NPCType<Brine_Latcher>();
				for (int i = 3 + Main.rand.Next(0, 3) * 2; i > 0; i--) {
					Vector2 position = NPC.position + Main.rand.NextVector2Circular(1, 1);
					NPC.NewNPC(
						source,
						(int)position.X,
						(int)position.Y,
						type
					);
				}
			}
		}
		public override void AI() {
			//NPC.active = false;
		}
	}
	public class Brine_Latcher : Brine_Pool_NPC {
		[CloneByReference]
		public HashSet<int> PreyNPCTypes { get; private set; } = [];
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.npcFrameCount[Type] = 6;
			NPCID.Sets.SpawnFromLastEmptySlot[Type] = true;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Velocity = 1f,
				IsWet = true
			};
			TargetNPCTypes.Add(ModContent.NPCType<Airsnatcher>());
			//TargetNPCTypes.Add(ModContent.NPCType<Mildew_Creeper>());
			PreyNPCTypes.Add(ModContent.NPCType<Airsnatcher>());
		}
		public bool? Hardmode => true;
		public override void SetDefaults() {
			NPC.aiStyle = NPCAIStyleID.ActuallyNone;
			NPC.lifeMax = 40;
			NPC.defense = 8;
			NPC.damage = 10;
			NPC.width = 22;
			NPC.height = 24;
			NPC.friendly = false;
			NPC.HitSound = SoundID.NPCHit19;
			NPC.DeathSound = SoundID.NPCDeath47;
			NPC.knockBackResist = 0.95f;
			NPC.value = 500;
			NPC.noGravity = true;
			SpawnModBiomes = [
				ModContent.GetInstance<Brine_Pool>().Type
			];
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags([
				this.GetBestiaryFlavorText()
			]);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(new LeadingConditionRule(DropConditions.PlayerInteraction).WithOnSuccess(
				ItemDropRule.ByCondition(new Conditions.IsHardmode(), ModContent.ItemType<Alkaliphiliac_Tissue>(), 10)
			).WithOnSuccess(
				ItemDropRule.ByCondition(new Conditions.PlayerNeedsHealing(), ItemID.Heart, 10) //Metroid reference
			));
		}
		public override bool CanTargetNPC(NPC other) {
			if (base.CanTargetNPC(other)) return true;
			if (!other.WithinRange(NPC.Center, 16 * 6 + NPC.width + Math.Max(other.width, other.height))) return false;
			return other.type == ModContent.NPCType<Sea_Dragon>();
		}
		public override bool CanTargetPlayer(Player player) {
			return base.CanTargetPlayer(player) && player.WithinRange(NPC.Center, 16 * 10 + NPC.width + Math.Max(player.width, player.height));
		}
		public override void AI() {
			Lighting.AddLight(NPC.Center, 0f, 0.4f, 0f);
			DoTargeting();
			Vector2 direction = default;
			if (NPC.wet) {
				NPC.noGravity = true;
				bool targetIsPrey = TargetPos != default && !TargetIsRipple && NPC.HasNPCTarget && PreyNPCTypes.Contains(Main.npc[NPC.TranslatedTargetIndex].type);
				if (TargetPos != default) {
					if (!targetIsPrey && !NPC.WithinRange(TargetPos, 16 * 12)) {
						TargetPos = default;
					} else if (NPC.HasPlayerTarget) {
						Player target = Main.player[NPC.target];
						if (!target.active || target.dead || !CanTargetPlayer(target)) TargetPos = default;
					} else if (NPC.HasNPCTarget) {
						NPC target = Main.npc[NPC.TranslatedTargetIndex];
						if (!target.active || !CanTargetNPC(target)) TargetPos = default;
					}
				}
				float friendMult = 0.1f;
				if (TargetPos != default) {
					if (NPC.HasNPCTarget && Main.npc[NPC.TranslatedTargetIndex].type == ModContent.NPCType<Sea_Dragon>()) {
						direction = NPC.DirectionTo(TargetPos);
						direction *= -1f;
						direction = direction.RotatedBy(Math.Clamp(-GeometryUtils.AngleDif(direction.ToRotation(), Main.npc[NPC.TranslatedTargetIndex].velocity.ToRotation()), -MathHelper.PiOver2, MathHelper.PiOver2)) * 1.5f;
						friendMult = 0f;
					} else {
						Vector2 diff = TargetPos - NPC.Center;
						float dist = diff.Length();
						if (dist > 8) direction = diff / dist;
					}
				} else {
					if (NPC.collideX && Math.Abs(NPC.velocity.X) > Math.Abs(NPC.velocity.Y) * 0.25f) {
						NPC.velocity.X = -NPC.direction;
					}
					NPC.direction = Math.Sign(NPC.velocity.X);
					if (NPC.direction == 0) NPC.direction = 1;
					direction = Vector2.UnitX * NPC.direction * 0.5f;
					friendMult = 0.2f;
				}
				Vector2 totalFriendDir = Vector2.Zero;
				int totalFriendCount = 0;
				foreach (NPC other in Main.ActiveNPCs) {
					if (other.type == Type && NPC.Center != other.Center && CollisionExt.CanHitRay(NPC.Center, other.Center)) {
						totalFriendDir += NPC.WithinRange(other.Center, 16 * 3) ? NPC.DirectionFrom(other.Center) : NPC.DirectionTo(other.Center);
						totalFriendCount++;
					}
				}
				if (totalFriendCount > 0) NPC.velocity += (totalFriendDir / totalFriendCount) * friendMult;
				NPC.velocity *= 0.96f;
				NPC.velocity += direction * 0.15f;
				MathUtils.LinearSmoothing(ref NPC.rotation, MathHelper.Clamp(NPC.velocity.X, -0.1f, 0.1f), 0.03f);
			} else {
				NPC.noGravity = false;
				NPC.rotation += MathHelper.Clamp(NPC.velocity.X, -0.1f, 0.1f);
				if (NPC.collideY) NPC.velocity.X *= 0.94f;
			}
			NPC.spriteDirection = Math.Sign(Math.Cos(NPC.rotation));
			if (NPC.ai[0] > 0) NPC.ai[0]--;
		}
		public override bool? CanFallThroughPlatforms() => true;
		public override void FindFrame(int frameHeight) {
			if (NPC.wet) NPC.DoFrames(6);
		}
		public override bool ModifyCollisionData(Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox) {
			if (NPC.ai[0] > 0) {
				npcHitbox = default;
			}
			return true;
		}
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			base.ModifyHitPlayer(target, ref modifiers);// inherit armor penetration
			modifiers.Knockback *= 0.25f;
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.Knockback *= 0.25f;
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
			NPC.ai[0] = 30;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit) {
			NPC.ai[0] = 30;
			target.immune[255] /= 3;
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				const float factor = 1f;
				const float r_factor = 1f / factor;
				for (int i = 0; i < 12; i++) {
					Dust dust = Dust.NewDustDirect(
						NPC.position,
						NPC.width,
						NPC.height,
						DustID.GreenBlood,
						NPC.velocity.X * r_factor,
						NPC.velocity.Y * r_factor
					);
					dust.velocity *= factor;
					dust = Dust.NewDustDirect(
						NPC.position,
						NPC.width,
						NPC.height,
						DustID.SparkForLightDisc,
						NPC.velocity.X * r_factor,
						NPC.velocity.Y * r_factor,
						newColor: new(0f, 0.4f, 0f, 0.4f)
					);
					dust.velocity *= factor;
				}
			}
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			SpriteEffects spriteEffects = SpriteEffects.FlipHorizontally;
			if (NPC.spriteDirection != 1) {
				spriteEffects |= SpriteEffects.FlipVertically;
			}
			if (NPC.IsABestiaryIconDummy) NPC.rotation = NPC.velocity.ToRotation();
			Texture2D texture = TextureAssets.Npc[Type].Value;
			Vector2 halfSize = new(15, 19);
			Vector2 position = new(NPC.position.X - screenPos.X + (NPC.width / 2) - texture.Width * NPC.scale / 2f + halfSize.X * NPC.scale, NPC.position.Y - screenPos.Y + NPC.height - texture.Height * NPC.scale / Main.npcFrameCount[NPC.type] + 4f + halfSize.Y * NPC.scale + NPC.gfxOffY);
			Vector2 origin = new(halfSize.X, halfSize.Y);
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
}
