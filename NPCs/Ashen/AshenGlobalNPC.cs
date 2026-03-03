using Origins.Buffs;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Ashen {
    public class AshenGlobalNPC : GlobalNPC {
		static List<ModNPC> npcCache = [];
		public override bool AppliesToEntity(NPC entity, bool lateInstantiation) {
			if (lateInstantiation) return false;
			if (entity.ModNPC is not IAshenEnemy) return false;
			(npcCache ??= []).Add(entity.ModNPC);
			return true;
		}
		public new static void SetStaticDefaults() {
			for (int i = 0; i < npcCache.Count; i++) {
				int type = npcCache[i].Type;
				if (npcCache[i] is IAshenEnemy { IsRobotic: true }) {
					NPCID.Sets.SpecificDebuffImmunity[type][BuffID.Poisoned] = true;
					NPCID.Sets.SpecificDebuffImmunity[type][BuffID.OnFire] = true;
					NPCID.Sets.SpecificDebuffImmunity[type][BuffID.OnFire3] = true;
				}
			}
			npcCache = null;
		}
		public override void UpdateLifeRegen(NPC npc, ref int damage) {
			if (npc.ModNPC is IAshenEnemy { FireImmuneUnlessOiled: true }) {
				bool anyToClear = false;
				anyToClear |= npc.buffImmune[BuffID.OnFire].TrySet(!npc.oiled);
				anyToClear |= npc.buffImmune[BuffID.OnFire3].TrySet(!npc.oiled);
				if (anyToClear && !npc.oiled) npc.ClearImmuneToBuffs(out _);
			}
			if (npc.dryadBane) {
				const float baseDPS = 1;
				int totalDPS = Main.rand.RandomRound(baseDPS * BiomeNPCGlobals.CalcDryadDPSMult());
				npc.lifeRegen -= 2 * totalDPS;
				damage += totalDPS / 3;
			}
		}
		class BleedOil : GlobalBuff {
			public override void Update(int type, NPC npc, ref int buffIndex) {
				if (npc.ModNPC is not IAshenEnemy { IsRobotic: true }) return;
				switch (type) {
					case BuffID.Bleeding:
					npc.oiled = true;
					break;
				}
			}
		}
	}
	public interface IAshenEnemy {
		public bool IsRobotic => true;
		public bool FireImmuneUnlessOiled => IsRobotic;
	}
}
