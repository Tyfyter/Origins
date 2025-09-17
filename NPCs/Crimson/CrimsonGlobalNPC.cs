using Origins.Buffs;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Crimson {
	public class CrimsonGlobalNPC : GlobalNPC {
		public static HashSet<int> NPCTypes { get; private set; } = [
			NPCID.BloodCrawler,
			NPCID.BloodCrawlerWall,
			NPCID.CrimsonBunny,
			NPCID.CrimsonGoldfish,
			NPCID.CrimsonPenguin,
			NPCID.FaceMonster,
			NPCID.Crimera,
			NPCID.BrainofCthulhu,
			NPCID.Creeper,

			NPCID.Herpling,
			NPCID.Crimslime,
			NPCID.BloodJelly,
			NPCID.BloodFeeder,

			NPCID.CrimsonAxe,
			NPCID.IchorSticker,
			NPCID.FloatyGross,
			NPCID.BigMimicCrimson,

			NPCID.BloodMummy,
			NPCID.DesertGhoulCrimson,

			NPCID.PigronCrimson,
		];
		public override void Load() {
			static void AddAssimilation(int npc, AssimilationAmount amount) => AssimilationLoader.AddNPCAssimilation<Crimson_Assimilation>(npc, amount);
			AddAssimilation(NPCID.BloodCrawler, 0.05f);
			AddAssimilation(NPCID.BloodCrawlerWall, 0.05f);
			AddAssimilation(NPCID.BloodFeeder, 0.06f);
			AddAssimilation(NPCID.BloodJelly, 0.08f);
			AddAssimilation(NPCID.BloodMummy, 0.08f);
			AddAssimilation(NPCID.SandsharkCrimson, 0.09f);
			AddAssimilation(NPCID.BrainofCthulhu, 0.16f);
			AddAssimilation(NPCID.Creeper, 0.002f);
			AddAssimilation(NPCID.Crimera, 0.05f);
			AddAssimilation(NPCID.Crimslime, 0.06f);
			AddAssimilation(NPCID.CrimsonGoldfish, 0.05f);
			AddAssimilation(NPCID.DesertGhoulCrimson, 0.06f);
			AddAssimilation(NPCID.FaceMonster, 0.08f);
			AddAssimilation(NPCID.FloatyGross, 0.08f);
			AddAssimilation(NPCID.Herpling, 0.06f);
			AddAssimilation(NPCID.IchorSticker, 0.06f);
		}
		public override void Unload() {
			NPCTypes = null;
		}
		public override bool AppliesToEntity(NPC entity, bool lateInstantiation) {
			return NPCTypes.Contains(entity.type);
		}
		public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers) {
			if (npc.ichor) {
				modifiers.Defense.Flat += 10;
			}
		}
		public override void ResetEffects(NPC npc) {
			int confusionIndex = npc.FindBuffIndex(BuffID.Confused);
			if (confusionIndex > -1 && Main.rand.NextBool()) {
				npc.buffTime[confusionIndex]--;
			}
		}
		public override void UpdateLifeRegen(NPC npc, ref int damage) {
			if (npc.poisoned) {
				npc.lifeRegen -= 5;
			}
			if (npc.venom) {
				npc.lifeRegen -= 10;
			}
            if (npc.onFire) {
				npc.lifeRegen -= 4;
				damage += 2;
			}
			if (npc.onFire3) {
				npc.lifeRegen -= 15;
				damage += 3;
			}
			if (npc.oiled && (npc.onFire || npc.onFire2 || npc.onFire3 || npc.shadowFlame)) {
				npc.lifeRegen -= 15;
			}
			if (npc.daybreak) {
				npc.lifeRegen += 25 * 2;
				damage -= 5;
			}
			if (npc.javelined) {
				npc.lifeRegen -= 6;
				damage += 3;
			}
			if (npc.dryadBane) {
				const float baseDPS = 2;
				int totalDPS = (int)(baseDPS * BiomeNPCGlobals.CalcDryadDPSMult());
				npc.lifeRegen -= 2 * totalDPS;
				damage += totalDPS / 3;
			}
		}
	}
}
