using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs {
	public static class BiomeNPCGlobals {
		public static float CalcDryadDPSMult() {
			float damageMult = 1f;
			if (NPC.downedBoss1) {
				damageMult += 0.1f;
			}
			if (NPC.downedBoss2) {
				damageMult += 0.1f;
			}
			if (NPC.downedBoss3) {
				damageMult += 0.1f;
			}
			if (NPC.downedQueenBee) {
				damageMult += 0.1f;
			}
			if (Main.hardMode) {
				damageMult += 0.4f;
			}
			if (NPC.downedMechBoss1) {
				damageMult += 0.15f;
			}
			if (NPC.downedMechBoss2) {
				damageMult += 0.15f;
			}
			if (NPC.downedMechBoss3) {
				damageMult += 0.15f;
			}
			if (NPC.downedPlantBoss) {
				damageMult += 0.15f;
			}
			if (NPC.downedGolemBoss) {
				damageMult += 0.15f;
			}
			if (NPC.downedAncientCultist) {
				damageMult += 0.15f;
			}
			if (Main.expertMode) {
				damageMult *= Main.GameModeInfo.TownNPCDamageMultiplier;
			}
			return damageMult;
		}
	}
	public struct AssimilationAmount {
		public Func<NPC, Player, float> Function { get; init; }
		public float ClassicAmount { get; init; }
		public float? ExpertAmount { get; init; }
		public float? MasterAmount { get; init; }
		public AssimilationAmount(float classicAmount, float? expertAmount = null, float? masterAmount = null) {
			Function = null;
			ClassicAmount = classicAmount;
			ExpertAmount = expertAmount;
			MasterAmount = masterAmount;
		}
		public AssimilationAmount(Func<NPC, Player, float> function) {
			Function = function;
			ClassicAmount = 0;
			ExpertAmount = null;
			MasterAmount = null;
		}
		public float GetValue(NPC attacker, Player victim) {
			if (Function is not null) {
				return Function(attacker, victim);
			}
			if (Main.masterMode && MasterAmount.HasValue) {
				return MasterAmount.Value;
			}
			if (Main.expertMode && ExpertAmount.HasValue) {
				return ExpertAmount.Value;
			}
			return ClassicAmount;
		}
		public static implicit operator AssimilationAmount(float value) => new(value);
		public static implicit operator AssimilationAmount((float classic, float expert) value) => new(value.classic, value.expert);
		public static implicit operator AssimilationAmount((float classic, float expert, float master) value) => new(value.classic, value.expert, value.master);
		public static implicit operator AssimilationAmount(Func<NPC, Player, float> function) => new(function);
	}
}
