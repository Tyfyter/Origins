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
				[NPCID.EaterofSouls] = 0.03f,
				[NPCID.DevourerHead] = 0.06f,
				[NPCID.DevourerBody] = 0.04f,
				[NPCID.DevourerTail] = 0.04f,
				[NPCID.Corruptor] = 0.09f,
				[NPCID.VileSpit] = 0.14f,
				[NPCID.Clinger] = 0.11f,
				[NPCID.EaterofWorldsHead] = 0.14f,
				[NPCID.EaterofWorldsBody] = 0.11f,
				[NPCID.EaterofWorldsTail] = 0.11f,
				[NPCID.VileSpitEaterOfWorlds] = 0.14f,
				[ModContent.NPCType<Optiphage>()] = 0.01f,
				[ModContent.NPCType<Cranivore>()] = 0.01f,
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
				int totalDPS = (int)(baseDPS * BiomeNPCGlobals.CalcDryadDPSMult());
				npc.lifeRegen -= 2 * totalDPS;
				damage += totalDPS / 3;
			}
		}
	}
}
