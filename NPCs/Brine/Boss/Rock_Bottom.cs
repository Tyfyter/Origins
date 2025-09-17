using Origins.Dev;
using Origins.World.BiomeData;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Brine.Boss {
	public class Rock_Bottom : Brine_Pool_NPC, ICustomWikiStat {
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 15;
			NPCID.Sets.CantTakeLunchMoney[Type] = true;
			NPCID.Sets.MPAllowedEnemies[Type] = true;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.HideInBestiary;
			NPCID.Sets.BossBestiaryPriority.Add(Type);
			NPCID.Sets.ImmuneToAllBuffs[Type] = true;
			Mildew_Creeper.FriendlyNPCTypes.Add(Type);
			ModContent.GetInstance<Brine_Pool.SpawnRates>().AddSpawn(Type, SpawnChance);
		}
		public bool? Hardmode => true;
		public override void SetDefaults() {
			NPC.aiStyle = NPCAIStyleID.ActuallyNone;
			NPC.lifeMax = 1800;
			NPC.defense = 24;
			NPC.noGravity = true;
			NPC.chaseable = false;
			NPC.width = 76;
			NPC.height = 58;
			NPC.knockBackResist = 0;
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) => (!Main.hardMode || NPC.AnyNPCs(Type)) ? 0 : Brine_Pool.SpawnRates.Dead_Guy;
		public override bool PreAI() {
			NPC.chaseable = false;
			NPC.velocity.X = 0f;
			NPC.velocity.Y = 0f;
			if (NPC.ai[1] == 0) return false;
			NPC.dontTakeDamage = true;
			switch ((int)NPC.ai[0]) {
				default:
				NPC.ai[0] += 1f / 15;
				break;
			}
			NPC.frame.Y = NPC.frame.Height * (int)NPC.ai[0];
			if (NPC.ai[0] >= Main.npcFrameCount[Type]) {
				NPC.Transform(ModContent.NPCType<Lost_Diver>());
			}
			return false;
		}
		public override bool CheckDead() {
			NPC.life = NPC.lifeMax;
			NPC.dontTakeDamage = true;
			NPC.ai[1] = 1;
			return false;
		}
		public override int SpawnNPC(int tileX, int tileY) {
			return NPC.NewNPC(null, tileX * 16 + 8, tileY * 16, NPC.type);
		}
	}
}
