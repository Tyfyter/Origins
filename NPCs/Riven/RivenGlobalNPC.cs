using Origins.Buffs;
using Origins.NPCs.Riven.World_Cracker;
using Origins.World.BiomeData;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace Origins.NPCs.Riven
{
    public class RivenGlobalNPC : GlobalNPC {
		public static Dictionary<int, AssimilationAmount> AssimilationAmounts { get; private set; }
		public override void Load() {
			AssimilationAmounts = new() {
				[ModContent.NPCType<Amebic_Slime>()] = 0.04f,
                [ModContent.NPCType<Amoeba_Bugger>()] = 0.03f,
                [ModContent.NPCType<Barnacleback>()] = 0.05f,
                [ModContent.NPCType<Cleaver_Head>()] = 0.04f,
				[ModContent.NPCType<Cleaver_Body>()] = 0.04f,
				[ModContent.NPCType<Cleaver_Tail>()] = 0.04f,
				[ModContent.NPCType<Flagellant>()] = 0.11f,
                [ModContent.NPCType<Measly_Moeba>()] = 0.05f,
                [ModContent.NPCType<Pustule_Jelly>()] = 0.08f,
                [ModContent.NPCType<Rivenator_Head>()] = 0.06f,
				[ModContent.NPCType<Rivenator_Body>()] = 0.06f,
				[ModContent.NPCType<Rivenator_Tail>()] = 0.06f,
				[ModContent.NPCType<Riven_Fighter>()] = 0.09f,
                [ModContent.NPCType<Riven_Mummy>()] = 0.07f,
                [ModContent.NPCType<Single_Cellular_Nautilus>()] = 0.03f,
                [ModContent.NPCType<Spider_Amoeba>()] = 0.04f,
                [ModContent.NPCType<World_Cracker_Head>()] = 0.07f,
                [ModContent.NPCType<World_Cracker_Body>()] = 0.04f,
                [ModContent.NPCType<World_Cracker_Tail>()] = 0.09f,
            };
		}
		public override void Unload() {
			AssimilationAmounts = null;
		}
		public override bool AppliesToEntity(NPC entity, bool lateInstantiation) {
			return entity.ModNPC is IRivenEnemy;
		}
		public override void SetDefaults(NPC entity) {
			if (entity.ModNPC is not null) {
				if ((entity.ModNPC.SpawnModBiomes?.Length ?? 0) == 0) {
					entity.ModNPC.SpawnModBiomes = new int[] {
						ModContent.GetInstance<Riven_Hive>().Type
					};
				} else {
					int[] spawnModBiomes = new int[entity.ModNPC.SpawnModBiomes.Length + 1];
					entity.ModNPC.SpawnModBiomes.CopyTo(spawnModBiomes, 1);
					spawnModBiomes[0] = ModContent.GetInstance<Riven_Hive>().Type;
					entity.ModNPC.SpawnModBiomes = spawnModBiomes;
				}
			}
			entity.buffImmune[ModContent.BuffType<Torn_Debuff>()] = true;
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
