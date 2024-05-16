using Origins.Buffs;
using Origins.NPCs.Riven.World_Cracker;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace Origins.NPCs.Riven {
    public class RivenGlobalNPC : GlobalNPC, IAssimilationProvider {
		public string AssimilationName => "RivenAssimilation";
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
                [ModContent.NPCType<Torn_Ghoul>()] = 0.10f,
            };
			BiomeNPCGlobals.assimilationProviders.Add(this);
		}
		public override void Unload() {
			AssimilationAmounts = null;
		}
		public override bool AppliesToEntity(NPC entity, bool lateInstantiation) {
			return entity.ModNPC is IRivenEnemy;
		}
		public override void SetDefaults(NPC entity) {
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
			AssimilationAmount amount = GetAssimilationAmount(npc);
			if (amount != default) {
				target.GetModPlayer<OriginPlayer>().RivenAssimilation += amount.GetValue(npc, target);
			}
		}
		public AssimilationAmount GetAssimilationAmount(NPC npc) {
			if (AssimilationAmounts.TryGetValue(npc.type, out AssimilationAmount amount)) {
				return amount;
			} else if (AssimilationAmounts.TryGetValue(-1, out amount)) {
				return amount;
			}
			return default;
		}
	}
	public interface IRivenEnemy {
	}
}
