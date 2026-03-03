using Origins.Buffs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Ashen {
    public class AshenGlobalNPC : GlobalNPC {
		public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.ModNPC is IAshenEnemy AshenEnemy;
		public override void SetStaticDefaults() {
			for (int i = 0; i < NPCLoader.NPCCount; i++) {
				if (ContentSamples.NpcsByNetId[i].ModNPC is IAshenEnemy { IsRobotic: true }) {
					NPCID.Sets.SpecificDebuffImmunity[i][BuffID.Poisoned] = true;
					NPCID.Sets.SpecificDebuffImmunity[i][BuffID.OnFire] = true;
					NPCID.Sets.SpecificDebuffImmunity[i][BuffID.OnFire3] = true;
				}
			}
		}
		public override void UpdateLifeRegen(NPC npc, ref int damage) {
			if (npc.ModNPC is IAshenEnemy { IsRobotic: true }) {
				npc.buffImmune[BuffID.OnFire] = !npc.oiled;
				npc.buffImmune[BuffID.OnFire3] = !npc.oiled;
			}
			if (npc.dryadBane) {
				const float baseDPS = 1;
				int totalDPS = Main.rand.RandomRound(baseDPS * BiomeNPCGlobals.CalcDryadDPSMult());
				npc.lifeRegen -= 2 * totalDPS;
				damage += totalDPS / 3;
			}
		}
	}
	public interface IAshenEnemy {
		public bool IsRobotic => true;
	}
}
