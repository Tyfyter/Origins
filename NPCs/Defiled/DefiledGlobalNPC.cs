using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Origins.NPCs.Defiled
{
    public class DefiledGlobalNPC : GlobalNPC {
		public static Dictionary<int, AssimilationAmount> AssimilationAmounts { get; private set; }
        public override void Load()
        {
            AssimilationAmounts = new()
            {
                [ModContent.NPCType<Chunky_Slime>()] = 0.05f,
                [ModContent.NPCType<Defiled_Amalgamation>()] = 0.11f,
                [ModContent.NPCType<Defiled_Brute>()] = 0.08f,
                [ModContent.NPCType<Defiled_Cyclops>()] = 0.08f,
                [ModContent.NPCType<Defiled_Digger_Head>()] = 0.06f,
                [ModContent.NPCType<Defiled_Digger_Body>()] = 0.03f,
                [ModContent.NPCType<Defiled_Digger_Tail>()] = 0.03f,
                [ModContent.NPCType<Defiled_Ekko>()] = 0.04f,
                [ModContent.NPCType<Defiled_Flyer>()] = 0.05f,
                [ModContent.NPCType<Defiled_Swarmer>()] = 0.02f,
                [ModContent.NPCType<Defiled_Tripod>()] = 0.07f,
                [ModContent.NPCType<Shattered_Mummy>()] = 0.07f,
            };
        }
        public override void Unload() {
			AssimilationAmounts = null;
		}
		public override bool AppliesToEntity(NPC entity, bool lateInstantiation) {
			return entity.ModNPC is IDefiledEnemy;
		}
		public override void UpdateLifeRegen(NPC npc, ref int damage) {
			if (npc.ModNPC is IDefiledEnemy defiledEnemy) {
				if (npc.life < npc.lifeMax && defiledEnemy.Mana > 0) {
					defiledEnemy.Regenerate(out int lifeRegen);
					if (!npc.HasBuff(BuffID.Bleeding)) npc.lifeRegen += lifeRegen;
				}
			} else {
				Mod.Logger.Error("something has gone extremely wrong and a non-defiled enemy has a DefiledGlobalNPC");
			}
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
				target.GetModPlayer<OriginPlayer>().DefiledAssimilation += amount.GetValue(npc, target);
			} else if (AssimilationAmounts.TryGetValue(-1, out amount)) {
				target.GetModPlayer<OriginPlayer>().DefiledAssimilation += amount.GetValue(npc, target);
			}
		}
        public override void OnKill(NPC npc) {
			if (npc.ModNPC is IDefiledEnemy defiledEnemy) {
				defiledEnemy.SpawnWisp(npc);
			}
		}
		public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter) {
			if (npc.ModNPC is IDefiledEnemy defiledEnemy && defiledEnemy.ForceSyncMana) {
				binaryWriter.Write(defiledEnemy.Mana);
			}
		}
		public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader) {
			if (npc.ModNPC is IDefiledEnemy defiledEnemy && defiledEnemy.ForceSyncMana) {
				defiledEnemy.Mana = binaryReader.ReadSingle();
			}
		}
	}
	public interface IDefiledEnemy {
		int MaxMana => 0;
		int MaxManaDrain => 0;
		float Mana { get; set; }
		bool ForceSyncMana => true;
		void Regenerate(out int lifeRegen) { lifeRegen = 0; }
		void SpawnWisp(NPC npc) {
			if (Main.expertMode) NPC.NewNPC(npc.GetSource_Death(), (int)npc.position.X, (int)npc.position.Y, ModContent.NPCType<Defiled_Wisp>());
		}
	}
	public static class IDefiledEnemyExt {
		public static void DrainMana(this IDefiledEnemy defiledEnemy, Player target) {
			int maxDrain = (int)Math.Min(defiledEnemy.MaxMana - defiledEnemy.Mana, defiledEnemy.MaxManaDrain);
			int manaDrain = Math.Min(maxDrain, target.statMana);
			target.statMana -= manaDrain;
			defiledEnemy.Mana += manaDrain;
			if (target.manaRegenDelay < 10) target.manaRegenDelay = 10;
		}
	}
}
