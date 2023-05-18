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
		}
	}
}
