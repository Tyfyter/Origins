using Origins.Buffs;
using Origins.NPCs.Defiled;
using Origins.NPCs.Riven.World_Cracker;
using Origins.World.BiomeData;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ModLoader;

namespace Origins.NPCs.Riven {
    public class RivenGlobalNPC : GlobalNPC {
		public override bool AppliesToEntity(NPC entity, bool lateInstantiation) {
			if (entity.ModNPC is IRivenEnemy rivenEnemy) {
				if (rivenEnemy.Assimilation is AssimilationAmount amount) {
					BiomeNPCGlobals.NPCAssimilationAmounts[entity.type] = new() {
						[ModContent.GetInstance<Riven_Assimilation>().AssimilationType] = amount
					};
				}
				return true;
			}
			return false;
		}
		public override void SetDefaults(NPC entity) {
			entity.buffImmune[ModContent.BuffType<Torn_Debuff>()] = true;
			entity.buffImmune[Goo_Wall_Debuff.ID] = true;
		}
		public override void UpdateLifeRegen(NPC npc, ref int damage) {
			if (npc.poisoned) {
				npc.lifeRegen += 2;
			}
			if (npc.onFire) {
				npc.lifeRegen += 4;
			}
			if (npc.onFire2) {// cursed inferno
				npc.lifeRegen += 12;
			}
			if (npc.onFire3) {// hellfire
				npc.lifeRegen += 15;
				damage -= 3;
			}
			if (npc.onFrostBurn) {
				npc.lifeRegen -= 12;
			}
			if (npc.onFrostBurn2) {
				npc.lifeRegen -= 24;
			}
			if (npc.shadowFlame) {
				npc.lifeRegen += 10;
				damage -= 1;
			}
			if (npc.oiled && (npc.onFire || npc.onFire2 || npc.onFire3 || npc.shadowFlame)) {
				npc.lifeRegen += 10;
				damage -= 2;
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
			if (npc.HasBuff(Barnacled_Buff.ID)) {
				npc.lifeRegen += 6;
			}
		}
	}
	public interface IRivenEnemy {
		AssimilationAmount? Assimilation => null;
	}
}
