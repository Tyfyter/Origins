using Origins.NPCs.MiscE;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Origins.NPCs.Riven {
	public class RivenGlobalNPC : GlobalNPC {
		public static Dictionary<int, AssimilationAmount> AssimilationAmounts { get; private set; }
		public override void Load() {
			AssimilationAmounts = new() {
				[ModContent.NPCType<Amebic_Slime>()] = 0.02f,
				[ModContent.NPCType<Cleaver_Head>()] = 0.06f,
				[ModContent.NPCType<Cleaver_Body>()] = 0.04f,
				[ModContent.NPCType<Cleaver_Tail>()] = 0.04f,
				[ModContent.NPCType<Flagellant>()] = 0.03f,
				[ModContent.NPCType<Rivenator_Head>()] = 0.06f,
				[ModContent.NPCType<Rivenator_Body>()] = 0.04f,
				[ModContent.NPCType<Rivenator_Tail>()] = 0.04f,
				[ModContent.NPCType<Riven_Fighter>()] = 0.01f,
			};
		}
		public override void Unload() {
			AssimilationAmounts = null;
		}
		public override bool AppliesToEntity(NPC entity, bool lateInstantiation) {
			return entity.ModNPC is IRivenEnemy;
		}
		public override void UpdateLifeRegen(NPC npc, ref int damage) {
			if (npc.poisoned) {
				npc.lifeRegen += 2;
			}
			if (npc.onFire) {
				npc.lifeRegen += 4;
			}
			if (npc.onFire2) {// cursed inferno
				npc.lifeRegen += 24;
				damage -= 5;
			}
			if (npc.onFire3) {// hellfire
				npc.lifeRegen += 15;
				damage -= 3;
			}
			if (npc.onFrostBurn) {
				npc.lifeRegen += 16;
				damage -= 1;
			}
			if (npc.onFrostBurn2) {
				npc.lifeRegen += 50;
				damage -= 5;
			}
			if (npc.shadowFlame) {
				npc.lifeRegen += 15;
				damage -= 3;
			}
			if (npc.oiled && (npc.onFire || npc.onFire2 || npc.onFire3 || npc.shadowFlame)) {
				npc.lifeRegen += 10;
				damage -= 2;
			}
			if (npc.daybreak) {
				npc.lifeRegen += 50 * 2;
				damage -= 10;
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
        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo) {
			if (AssimilationAmounts.TryGetValue(npc.type, out AssimilationAmount amount)) {
				target.GetModPlayer<OriginPlayer>().RivenAssimilation += amount.GetValue(npc, target);
			} else if (AssimilationAmounts.TryGetValue(-1, out amount)) {
				target.GetModPlayer<OriginPlayer>().RivenAssimilation += amount.GetValue(npc, target);
			}
		}
	}
	public interface IRivenEnemy {
	}
}
