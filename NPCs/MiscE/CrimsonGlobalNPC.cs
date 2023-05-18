using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.MiscE {
	public class CrimsonGlobalNPC : GlobalNPC {
		public static HashSet<int> NPCTypes { get; private set; }
		public override void Load() {
			NPCTypes = new() {
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
			};
		}
		public override void Unload() {
			NPCTypes = null;
		}
		public override bool AppliesToEntity(NPC entity, bool lateInstantiation) {
			return NPCTypes.Contains(entity.type);
		}
		public override void ResetEffects(NPC npc) {
			int confusionIndex = npc.FindBuffIndex(BuffID.Confused);
			if (confusionIndex > -1 && Main.rand.NextBool()) {
				npc.buffTime[confusionIndex]--;
			}
		}
		public override void UpdateLifeRegen(NPC npc, ref int damage) {
			if (npc.poisoned) {
				npc.lifeRegen += 2;
			}
			if (npc.venom) {
				npc.lifeRegen += 30;
				damage -= 5;
			}
			// not sure how to give resistance to confused, but crimson enemies probably shouldn't be entirely immune to it
			if (npc.onFire) {
				npc.lifeRegen -= 4;
				damage += 3;
			}
			if (npc.onFire3) {
				npc.lifeRegen -= 15;
				damage += 3;
			}
			if (npc.oiled && (npc.onFire || npc.onFire2 || npc.onFire3 || npc.shadowFlame)) {
				npc.lifeRegen -= 25;
				damage += 5;
			}
			if (npc.daybreak) {
				npc.lifeRegen -= 50 * 2;
				damage += 25;
			}
			if (npc.dryadBane) {
				const float baseDPS = 2;
				int totalDPS = (int)(baseDPS * CorruptGlobalNPC.CalcDryadDPSMult());
				npc.lifeRegen -= 2 * totalDPS;
				damage += totalDPS / 3;
			}
		}
	}
}
