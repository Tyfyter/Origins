using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.MiscE {
    public class CorruptGlobalNPC : GlobalNPC {
		public static HashSet<int> NPCTypes { get; private set; }
		public static Dictionary<int, AssimilationAmount> AssimilationAmounts { get; private set; }
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
			};
			AssimilationAmounts = new() {
                [NPCID.Clinger] = 0.11f,
                [NPCID.CorruptGoldfish] = 0.05f,
                [NPCID.Corruptor] = 0.09f,
                [NPCID.CorruptSlime] = 0.04f,
                [NPCID.DesertGhoulCorruption] = 0.06f,
                [NPCID.DevourerHead] = 0.05f,
                [NPCID.DevourerBody] = 0.04f,
                [NPCID.DevourerTail] = 0.05f,
                [NPCID.EaterofSouls] = 0.07f,
                [NPCID.EaterofWorldsHead] = 0.14f,
                [NPCID.EaterofWorldsBody] = 0.11f,
                [NPCID.EaterofWorldsTail] = 0.11f,
                [NPCID.Slimeling] = 0.03f,
                [NPCID.Slimer] = 0.06f,
                [NPCID.VileSpit] = 0.14f,
				[NPCID.VileSpitEaterOfWorlds] = 0.09f,
                [ModContent.NPCType<Cranivore>()] = 0.02f,
                [ModContent.NPCType<Optiphage>()] = 0.02f,
			};
		}
		public override void Unload() {
			NPCTypes = null;
			AssimilationAmounts = null;
		}
		public override bool AppliesToEntity(NPC entity, bool lateInstantiation) {
			return NPCTypes.Contains(entity.type);
		}
		public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo) {
			if (AssimilationAmounts.TryGetValue(npc.type, out AssimilationAmount amount)) {
				target.GetModPlayer<OriginPlayer>().CorruptionAssimilation += amount.GetValue(npc, target);
			} else if (AssimilationAmounts.TryGetValue(-1, out amount)) {
				target.GetModPlayer<OriginPlayer>().CorruptionAssimilation += amount.GetValue(npc, target);
			}
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
			if (npc.dryadBane) {
				const float baseDPS = 2;
				int totalDPS = (int)(baseDPS * BiomeNPCGlobals.CalcDryadDPSMult());
				npc.lifeRegen -= 2 * totalDPS;
				damage += totalDPS / 3;
			}
		}
	}
}
