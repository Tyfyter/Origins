using Origins.Buffs;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Corrupt {
    public class CorruptGlobalNPC : GlobalNPC {
		public static HashSet<int> NPCTypes { get; private set; } = [
			NPCID.EaterofSouls,
			NPCID.CorruptBunny,
			NPCID.CorruptGoldfish,
			NPCID.CorruptPenguin,
			NPCID.DevourerHead,
			NPCID.DevourerBody,
			NPCID.DevourerTail,
			NPCID.EaterofWorldsHead,
			NPCID.EaterofWorldsBody,
			NPCID.EaterofWorldsTail,
			NPCID.VileSpitEaterOfWorlds,

			NPCID.Corruptor,
			NPCID.VileSpit,
			NPCID.CorruptSlime,
			NPCID.Slimeling,
			NPCID.Slimer,
			NPCID.Slimer2,
			NPCID.SeekerHead,
			NPCID.SeekerBody,
			NPCID.SeekerTail,

			NPCID.CursedHammer,
			NPCID.Clinger,
			NPCID.BigMimicCorruption,

			NPCID.DarkMummy,
			NPCID.DesertGhoulCorruption,

			NPCID.PigronCorruption,
		];
		public override void Load() {
			static void AddAssimilation(int npc, AssimilationAmount amount) => AssimilationLoader.AddNPCAssimilation<Corrupt_Assimilation>(npc, amount);
			AddAssimilation(NPCID.Clinger, 0.11f);
			AddAssimilation(NPCID.CorruptGoldfish, 0.05f);
			AddAssimilation(NPCID.Corruptor, 0.09f);
			AddAssimilation(NPCID.CorruptSlime, 0.04f);
			AddAssimilation(NPCID.DesertGhoulCorruption, 0.06f);
			AddAssimilation(NPCID.DarkMummy, 0.08f);
			AddAssimilation(NPCID.SandsharkCorrupt, 0.09f);
			AddAssimilation(NPCID.DevourerHead, 0.05f);
			AddAssimilation(NPCID.DevourerBody, 0.04f);
			AddAssimilation(NPCID.DevourerTail, 0.05f);
			AddAssimilation(NPCID.SeekerHead, 0.10f);
			AddAssimilation(NPCID.SeekerBody, 0.08f);
			AddAssimilation(NPCID.SeekerTail, 0.10f);
			AddAssimilation(NPCID.EaterofSouls, 0.07f);
			AddAssimilation(NPCID.EaterofWorldsHead, 0.07f);
			AddAssimilation(NPCID.EaterofWorldsBody, 0.05f);
			AddAssimilation(NPCID.EaterofWorldsTail, 0.07f);
			AddAssimilation(NPCID.Slimeling, 0.03f);
			AddAssimilation(NPCID.Slimer, 0.06f);
			AddAssimilation(NPCID.VileSpit, 0.14f);
			AddAssimilation(NPCID.VileSpitEaterOfWorlds, 0.05f);
		}
		public override void Unload() {
			NPCTypes = null;
		}
		public override bool AppliesToEntity(NPC entity, bool lateInstantiation) {
			return NPCTypes.Contains(entity.type);
		}
		public override void UpdateLifeRegen(NPC npc, ref int damage) {
			if (npc.poisoned) {
				npc.lifeRegen += 4;
			}
			if (npc.onFire2) {// cursed inferno
				npc.lifeRegen += 36;
				damage -= 8;
			}
			if (npc.onFire) {
				npc.lifeRegen -= 4;
				damage += 3;
			}
			if (npc.onFire3) {
				npc.lifeRegen -= 15;
				damage += 3;
			}
			if (npc.shadowFlame) {
				npc.lifeRegen += 7;
				damage -= 3;
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
