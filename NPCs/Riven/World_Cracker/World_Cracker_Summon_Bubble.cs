using Origins.Dev;
using Origins.Gores.NPCs;
using Origins.World.BiomeData;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.NPCs.Riven.World_Cracker {
	public class World_Cracker_Summon_Bubble : Glowing_Mod_NPC, IRivenEnemy, IWikiNPC {
		public Rectangle DrawRect => new(0, 0, 50, 52);
		public int AnimationFrames => 4;
		public int FrameDuration => 8;
		public NPCExportType ImageExportType => NPCExportType.SpriteSheet;
		public static int ID { get; private set; }
		public static List<int> ValidSpawns { get; private set; } = [];
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 4;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.HideInBestiary;
			NPCID.Sets.DontDoHardmodeScaling[NPC.type] = true;
			ID = Type;
			World_Cracker_Head.Minions.Add(Type);
		}
		public static void MakeWorldCrackerMinion(ModNPC modNPC) {
			ValidSpawns.Add(modNPC.Type);
			NPCID.Sets.NPCBestiaryDrawOffset[modNPC.Type] = NPCExtensions.HideInBestiary;
			ContentSamples.NpcBestiaryRarityStars[modNPC.Type] = 3;
			NPCID.Sets.DontDoHardmodeScaling[modNPC.Type] = true;
			World_Cracker_Head.Minions.Add(modNPC.Type);
		}
		public override void Unload() => ValidSpawns = null;
		public override void SetDefaults() {
			NPC.noGravity = true;
			NPC.aiStyle = NPCAIStyleID.None;
			NPC.lifeMax = 75;
			NPC.defense = 0;
			NPC.damage = 0;
			NPC.width = 50;
			NPC.height = 50;
			NPC.friendly = false;
			NPC.HitSound = SoundID.NPCHit13;
			NPC.DeathSound = SoundID.NPCDeath15;
			NPC.knockBackResist = 0.01f;
			NPC.scale = 0.95f;
			NPC.value = 0;
		}
		public static int HatchTime => 360 - 60 * World_Cracker_Head.DifficultyMult;
		public override bool? CanFallThroughPlatforms() => true;
		public override void OnSpawn(IEntitySource source) {
			if (NPC.ai[0] == 0) NPC.ai[0] = Main.rand.Next(ValidSpawns);
		}
		public override void AI() {
			Vector2 targetPosition = new(NPC.ai[1], NPC.ai[2]);
			if (++NPC.ai[3] >= HatchTime && Main.netMode != NetmodeID.MultiplayerClient) {
				NPC.StrikeInstantKill();
			}
			Vector2 vectorToTargetPosition = targetPosition - NPC.Center;
			float speed = 4f;
			float inertia = 18f;
			vectorToTargetPosition.Normalize();
			vectorToTargetPosition *= speed;
			NPC.velocity = (NPC.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;
			Vector2 nextVel = Collision.TileCollision(NPC.position, NPC.velocity, NPC.width, NPC.height, true, true);
			if (nextVel.X != NPC.velocity.X) NPC.velocity.X *= -1.2f;
			if (nextVel.Y != NPC.velocity.Y) NPC.velocity.Y *= -1.2f;

			NPC.DoFrames(6);
			NPC.spriteDirection = 1;
		}
		public override void HitEffect(NPC.HitInfo hit) {
			for (int i = (int)((NPC.life <= 0 ? 12 : 4) * Main.gfxQuality); i-- > 0;) {
				Gore.NewGore(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, Main.rand.Next(R_Effect_Blood1.GoreIDs));
			}
		}
		public override void OnKill() {
			if (Main.netMode == NetmodeID.MultiplayerClient) return;
			const float guaranteed_health = 0.05f;
			int npcType = (int)NPC.ai[0];
			float healthPercent = (guaranteed_health + (NPC.ai[3] / HatchTime) * (1 - guaranteed_health));
			if (npcType == ModContent.NPCType<Amoeba_Bugger_WC>()) {
				for (int i = (int)((3 + World_Cracker_Head.DifficultyMult) * healthPercent); i >= 0; i--) {
					NPC.NewNPC(
						NPC.GetSource_Death(),
						(int)NPC.Center.X,
						(int)NPC.Center.Y,
						npcType,
						ai0: Main.rand.NextFloat(-4, 4),
						ai1: Main.rand.NextFloat(-4, 4)
					);
				}
			} else if (npcType == ModContent.NPCType<World_Cracker_Exoskeleton_WC>()) {
				for (int i = 0; i < 2 * healthPercent; i++) {
					int index = NPC.NewNPC(
						NPC.GetSource_Death(),
						(int)NPC.Center.X,
						(int)NPC.Center.Y,
						npcType
					);
					NPC newNPC = Main.npc[index];
					newNPC.life = (int)(newNPC.lifeMax * healthPercent);
					newNPC.velocity.X = 3 - 6 * i;
					NetMessage.SendData(MessageID.SyncNPC, number: index);
				}
			} else {
				int index = NPC.NewNPC(
					NPC.GetSource_Death(),
					(int)NPC.Center.X,
					(int)NPC.Center.Y,
					npcType
				);
				NPC newNPC = Main.npc[index];
				newNPC.life = (int)(newNPC.lifeMax * healthPercent);
				NetMessage.SendData(MessageID.SyncNPC, number: index);
			}
		}
		public override Color GetGlowColor(Color drawColor) => Riven_Hive.GetGlowAlpha(drawColor);
	}
	public class Riven_Fighter_WC : Riven_Fighter, ICustomWikiStat {
		string ICustomWikiStat.CustomStatPath => nameof(Riven_Fighter_WC);
		public override string Texture => typeof(Riven_Fighter).GetDefaultTMLName();
		public override LocalizedText DisplayName => Language.GetOrRegister(Mod.GetLocalizationKey($"{LocalizationCategory}.Riven_Fighter.DisplayName"));
		public override void Load() { }
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			World_Cracker_Summon_Bubble.MakeWorldCrackerMinion(this);
		}
		public override void SetDefaults() {
			base.SetDefaults();
			NPC.lifeMax /= 4;
			this.CopyBanner<Riven_Fighter>();
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) => 0;
	}
	public class Amebic_Slime_WC : Amebic_Slime, ICustomWikiStat {
		string ICustomWikiStat.CustomStatPath => nameof(Amebic_Slime_WC);
		public override string Texture => typeof(Amebic_Slime).GetDefaultTMLName();
		public override LocalizedText DisplayName => Language.GetOrRegister(Mod.GetLocalizationKey($"{LocalizationCategory}.Amebic_Slime.DisplayName"));
		public override void Load() { }
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			World_Cracker_Summon_Bubble.MakeWorldCrackerMinion(this);
		}
		public override void SetDefaults() {
			base.SetDefaults();
			NPC.lifeMax /= 4;
			this.CopyBanner<Amebic_Slime>();
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) => 0;
	}
	public class Amoeba_Bugger_WC : Amoeba_Bugger, ICustomWikiStat {
		string ICustomWikiStat.CustomStatPath => nameof(Amoeba_Bugger_WC);
		public override string Texture => typeof(Amoeba_Bugger).GetDefaultTMLName();
		public override LocalizedText DisplayName => Language.GetOrRegister(Mod.GetLocalizationKey($"{LocalizationCategory}.Amoeba_Bugger.DisplayName"));
		public override void Load() { }
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			World_Cracker_Summon_Bubble.MakeWorldCrackerMinion(this);
		}
		public override void SetDefaults() {
			base.SetDefaults();
			NPC.lifeMax /= 2;
		}
	}
	public class World_Cracker_Exoskeleton_WC : World_Cracker_Exoskeleton, ICustomWikiStat {
		string ICustomWikiStat.CustomStatPath => nameof(World_Cracker_Exoskeleton_WC);
		public override LocalizedText DisplayName => Language.GetOrRegister(Mod.GetLocalizationKey($"{LocalizationCategory}.World_Cracker_Exoskeleton.DisplayName"));
		public override void Load() { }
		public override void SafeSetStaticDefaults() {
			World_Cracker_Summon_Bubble.MakeWorldCrackerMinion(this);
			NPCID.Sets.NPCBestiaryDrawOffset.Remove(Type);
		}
		public override void SetDefaults() {
			base.SetDefaults();
			NPC.lifeMax /= 2;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				ModContent.GetInstance<World_Cracker_Exoskeleton>().GetBestiaryFlavorText()
			);
		}
	}
}
