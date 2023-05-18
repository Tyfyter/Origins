using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.MiscE {
	public class CorruptGlobalNPC : GlobalNPC {
		public static HashSet<int> NPCTypes { get; private set; }
		public override void Load() {
			NPCTypes = new() {
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

				NPCID.Corruptor,
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
			};
		}
		public override void Unload() {
			NPCTypes = null;
		}
		public override bool AppliesToEntity(NPC entity, bool lateInstantiation) {
			return NPCTypes.Contains(entity.type);
		}
		public override void UpdateLifeRegen(NPC npc, ref int damage) {
			if (npc.poisoned) {
				npc.lifeRegen += 2;
			}
			if (npc.onFire2) {// cursed inferno
				npc.lifeRegen += 24;
				damage -= 5;
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
				npc.lifeRegen -= 15;
				damage += 3;
			}
			if (npc.oiled && (npc.onFire || npc.onFire2 || npc.onFire3 || npc.shadowFlame)) {
				npc.lifeRegen -= 25;
				damage += 5;
			}
			if (npc.dryadBane) {
				const float baseDPS = 2;
				int totalDPS = (int)(baseDPS * CalcDryadDPSMult());
				npc.lifeRegen -= 2 * totalDPS;
				damage += totalDPS / 3;
			}
		}
		public static float CalcDryadDPSMult() {
			float damageMult = 1f;
			if (NPC.downedBoss1) {
				damageMult += 0.1f;
			}
			if (NPC.downedBoss2) {
				damageMult += 0.1f;
			}
			if (NPC.downedBoss3) {
				damageMult += 0.1f;
			}
			if (NPC.downedQueenBee) {
				damageMult += 0.1f;
			}
			if (Main.hardMode) {
				damageMult += 0.4f;
			}
			if (NPC.downedMechBoss1) {
				damageMult += 0.15f;
			}
			if (NPC.downedMechBoss2) {
				damageMult += 0.15f;
			}
			if (NPC.downedMechBoss3) {
				damageMult += 0.15f;
			}
			if (NPC.downedPlantBoss) {
				damageMult += 0.15f;
			}
			if (NPC.downedGolemBoss) {
				damageMult += 0.15f;
			}
			if (NPC.downedAncientCultist) {
				damageMult += 0.15f;
			}
			if (Main.expertMode) {
				damageMult *= Main.GameModeInfo.TownNPCDamageMultiplier;
			}
			return damageMult;
		}
	}
}
