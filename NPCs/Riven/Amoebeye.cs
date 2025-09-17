using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using Origins.Gores.NPCs;
using Origins.Items.Materials;
using Origins.Journal;
using Origins.World.BiomeData;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Riven {
	public class Amoebeye : ModNPC, IRivenEnemy, IWikiNPC, IJournalEntrySource, ICustomWikiStat {
		public string EntryName => "Origins/" + typeof(Amoebeye_Entry).Name;
		public class Amoebeye_Entry : JournalEntry {
			public override string TextKey => "Amoebeye";
			public override JournalSortIndex SortIndex => new("Riven", 7);
		}
		public Rectangle DrawRect => new(0, 0, 70, 98);
		public int AnimationFrames => 32;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		public AssimilationAmount? Assimilation => 0.04f;
		public static int ID { get; private set; }
		public override void Load() => this.AddBanner();
		public AutoLoadingAsset<Texture2D> glowTexture = typeof(Amoebeye).GetDefaultTMLName() + "_Glow";
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 4;
			NPCID.Sets.UsesNewTargetting[Type] = true;
			ID = Type;
			ModContent.GetInstance<Riven_Hive.SpawnRates>().AddSpawn(Type, SpawnChance);
		}
		public bool? Hardmode => true;
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.IchorSticker);
			NPC.aiStyle = NPCAIStyleID.Hovering;
			NPC.lifeMax = 300;
			NPC.defense = 60;
			NPC.damage = 52;
			NPC.width = 72;
			NPC.height = 64;
			NPC.friendly = false;
			NPC.HitSound = SoundID.NPCHit13;
			NPC.DeathSound = SoundID.NPCDeath15;
			NPC.knockBackResist = 0.75f;
			NPC.value = 5000;
			SpawnModBiomes = [
				ModContent.GetInstance<Riven_Hive>().Type,
				ModContent.GetInstance<Underground_Riven_Hive_Biome>().Type
			];
		}
		public new static float SpawnChance(NPCSpawnInfo spawnInfo) {
			return Riven_Hive.SpawnRates.FlyingEnemyRate(spawnInfo, true) * Riven_Hive.SpawnRates.Amoebeye;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Bud_Barnacle>(), 3));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Alkahest>(), 1, 2, 5));
		}
		public override bool PreAI() {
			if (Main.rand.NextBool(1000)) {
				SoundEngine.PlaySound(SoundID.Zombie47.WithPitch(-1f).WithVolume(0.35f), NPC.Center);
				SoundEngine.PlaySound(Origins.Sounds.WCIdle.WithPitchRange(1.35f, 1.55f).WithVolume(0.15f), NPC.Center);
			}
			NPC.aiStyle = NPCAIStyleID.Hovering;
			if (NPC.ai[3] != 0) {
				NPC.defense = 0;
				bool isAggro = NPC.ai[3] == -1;
				if (!isAggro) {
					NPC bubble = Main.npc[(int)NPC.ai[3] - 1];
					if (!bubble.active || bubble.type != Amoebeye_P.ID) {
						SoundEngine.PlaySound(SoundID.Zombie61.WithPitch(1f), NPC.Center);
						SoundEngine.PlaySound(Origins.Sounds.WCHit.WithPitchRange(1.35f, 1.55f), NPC.Center);
						NPC.ai[3] = -1;
						isAggro = true;
					} else if (bubble.ai[0] != 0) {
						NPC.target = (int)bubble.ai[0] - 1;
						isAggro = true;
					}
				}
				if (isAggro) {
					NPC.aiStyle = NPCAIStyleID.None;
				}
			}
			return true;
		}
		public override bool? CanFallThroughPlatforms() => true;
		public override void AI() {
			const float attack_range = 16 * 16;
			if (NPC.ai[3] == 0) {
				NPC.defense = NPC.defDefense;
				NPC.TargetClosestUpgraded();
				NPCAimedTarget target = NPC.GetTargetData();
				if (!target.Invalid) {
					if (NPC.DistanceSQ(target.Center) < attack_range * attack_range) {
						if (++NPC.localAI[3] > 30) {
							NPC.ai[3] = 1 + NPC.NewNPC(
								NPC.GetSource_FromAI(),
								(int)NPC.Center.X,
								(int)NPC.Center.Y,
								ModContent.NPCType<Amoebeye_P>(),
								ai3: NPC.whoAmI
							);
							SoundEngine.PlaySound(Main.rand.NextBool() ? SoundID.Item111 : SoundID.Item112, NPC.Center);
						} else {
							Vector2 offset = Main.rand.NextVector2CircularEdge(32, 32);
							Vector2 pos = NPC.Center + offset;
							for (int i = Main.rand.Next(3, 6); i-- > 0;) {
								Gore.NewGore(NPC.GetSource_FromAI(), pos, offset * -0.125f + NPC.velocity, ModContent.GoreType<R_Effect_Blood1_Small>());
							}
						}
					} else if (NPC.localAI[3] > 0) NPC.localAI[3]--;
				}
			} else if (NPC.aiStyle == NPCAIStyleID.None) {
				NPC.rotation = NPC.velocity.X * 0.1f;
				NPCAimedTarget target = NPC.GetTargetData();
				Vector2 vectorToTargetPosition = target.Center - NPC.Center;
				float speed = 16f;
				float inertia = 32f;
				vectorToTargetPosition.Normalize();
				vectorToTargetPosition *= speed;
				NPC.velocity = (NPC.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;
				Vector2 nextVel = Collision.TileCollision(NPC.position, NPC.velocity, NPC.width, NPC.height, true, true);
				if (nextVel.X != NPC.velocity.X) NPC.velocity.X *= -0.9f;
				if (nextVel.Y != NPC.velocity.Y) NPC.velocity.Y *= -0.9f;
			}
			if (NPC.aiStyle == NPCAIStyleID.Hovering) NPC.spriteDirection = NPC.direction;
		}
		public override void FindFrame(int frameHeight) {
			if (++NPC.frameCounter > 7) {
				NPC.frame = new Rectangle(0, (NPC.frame.Y + 98) % 392, 70, 96);
				NPC.frameCounter = 0;
			}
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				SpawnGore<Amoebeye1_Gore>(NPC.Center + new Vector2(12, -12));
				SpawnGore<Amoebeye2_Gore>(NPC.Center + new Vector2(-12, 2));
				SpawnGore<Amoebeye3_Gore>(NPC.Center + new Vector2(21, 5));
				for (int i = 0; i < 3; i++) {
					Gore.NewGore(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, Main.rand.Next(R_Effect_Blood1.GoreIDs));
				}
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/R_Effect_Meat" + Main.rand.Next(1, 4));
			}
			NPC.frameCounter = 0;
		}
		void SpawnGore<T>(Vector2 position) where T : ModGore{
			Gore.NewGoreDirect(NPC.GetSource_Death(), position, NPC.velocity, ModContent.GoreType<T>());
		}
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Glowing_Mod_NPC.DrawGlow(spriteBatch, screenPos, glowTexture, NPC, Riven_Hive.GetGlowAlpha(drawColor));
		}
	}
	public class Amoebeye_P : ModNPC, IRivenEnemy, IWikiNPC {
		public Rectangle DrawRect => new(0, 0, 58, 60);
		public int AnimationFrames => 4;
		public int FrameDuration => 8;
		public NPCExportType ImageExportType => NPCExportType.SpriteSheet;
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 4;
			NPCID.Sets.ProjectileNPC[Type] = true;
			NPCID.Sets.UsesNewTargetting[Type] = true;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.HideInBestiary;
			ID = Type;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.IchorSticker);
			NPC.aiStyle = NPCAIStyleID.None;
			NPC.lifeMax = 90;
			NPC.defense = 0;
			NPC.damage = 20;
			NPC.width = 58;
			NPC.height = 58;
			NPC.friendly = false;
			NPC.HitSound = SoundID.NPCHit13;
			NPC.DeathSound = SoundID.NPCDeath15;
			NPC.knockBackResist = 0.01f;
			NPC.value = 0;
		}
		public override bool? CanFallThroughPlatforms() => true;
		public override void AI() {
			NPC parent = Main.npc[(int)NPC.ai[3]];
			NPCAimedTarget target;
			bool returning = false;
			if (!parent.active || parent.type != Amoebeye.ID) {
				if (Main.netMode != NetmodeID.MultiplayerClient) NPC.StrikeInstantKill();
				return;
			} else if (NPC.ai[0] != 0) {
				NPC.target = (int)NPC.ai[0] - 1;
				target = parent.GetTargetData();
				if (NPC.ai[1] < 1) NPC.ai[1] += 0.05f;
				NPC.Center = Vector2.Lerp(NPC.Center, target.Center, NPC.ai[1]);
				if (target.Type == NPCTargetType.Player) {
					Player playerTarget =  Main.player[NPC.target];
					playerTarget.GetAssimilation<Riven_Assimilation>().Percent += 0.05f / 60f;
					playerTarget.velocity *= 0.95f;
				}
				NPC.defense = 40;
				return;
			}
			NPC.defense = NPC.defDefense;

			target = parent.GetTargetData();
			Vector2 targetPosition = target.Center;
			NPC.ai[1] = 0;
			if (++NPC.ai[2] >= 450) {
				targetPosition = parent.Center;
				returning = true;
				NPC.noTileCollide = true;
			}
			Vector2 vectorToTargetPosition = targetPosition - NPC.Center;
			if (returning && vectorToTargetPosition.LengthSquared() < 16 * 16) {
				parent.ai[3] = 0;
				NPC.active = false;
			}
			float speed = 8f;
			float inertia = 18f;
			vectorToTargetPosition.Normalize();
			vectorToTargetPosition *= speed;
			NPC.velocity = (NPC.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;
			Vector2 nextVel = Collision.TileCollision(NPC.position, NPC.velocity, NPC.width, NPC.height, true, true);
			if (nextVel.X != NPC.velocity.X) NPC.velocity.X *= -1.2f;
			if (nextVel.Y != NPC.velocity.Y) NPC.velocity.Y *= -1.2f;

			if (++NPC.frameCounter > 6) {
				NPC.frame = new Rectangle(0, (NPC.frame.Y + 60) % 240, 58, 58);
				NPC.frameCounter = 0;
			}
			NPC.spriteDirection = 1;
		}
		public override Color? GetAlpha(Color drawColor) => Riven_Hive.GetGlowAlpha(drawColor);
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
			if (NPC.ai[0] == 0) {
				NPC.ai[0] = target.whoAmI + 1;
			}
		}
		public override bool CanHitPlayer(Player target, ref int cooldownSlot) {
			return target.whoAmI + 1 != NPC.ai[0];
		}
		public override void HitEffect(NPC.HitInfo hit) {
			for (int i = (int)((NPC.life <= 0 ? 12 : 4) * Main.gfxQuality) ; i-->0;) {
				Gore.NewGore(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, Main.rand.Next(R_Effect_Blood1.GoreIDs));
			}
		}
	}
}
