using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace Origins.NPCs {
	public class BiomeNPCGlobals : ILoadable {
		public static Dictionary<int, Dictionary<int, AssimilationAmount>> assimilationDisplayOverrides = [];
		public static Dictionary<int, Dictionary<int, AssimilationAmount>> NPCAssimilationAmounts { get; private set; } = [];
		public static Dictionary<int, Dictionary<int, AssimilationAmount>> ProjectileAssimilationAmounts { get; private set; } = [];
		public static Dictionary<int, Dictionary<int, AssimilationAmount>> DebuffAssimilationAmounts { get; private set; } = [];
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

		public void Load(Mod mod) { }
		public void Unload() {
			assimilationDisplayOverrides = null;
			NPCAssimilationAmounts = null;
			ProjectileAssimilationAmounts = null;
			DebuffAssimilationAmounts = null;
		}
	}
	public readonly struct AssimilationAmount {
		public Func<Entity, Player, float> Function { get; init; }
		public float ClassicAmount { get; init; }
		public float? ExpertAmount { get; init; }
		public float? MasterAmount { get; init; }
		public AssimilationAmount(float classicAmount, float? expertAmount = null, float? masterAmount = null) {
			Function = null;
			ClassicAmount = classicAmount;
			ExpertAmount = expertAmount;
			MasterAmount = masterAmount;
		}
		public AssimilationAmount(Func<Entity, Player, float> function) {
			Function = function;
			ClassicAmount = 0;
			ExpertAmount = null;
			MasterAmount = null;
		}
		public readonly float GetValue(Entity attacker, Player victim) {
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
		public readonly object GetText() {
			if (Function is not null) {
				return Language.GetOrRegister("Mods.Origins.Generic.VariableAssimilation");
			}
			if (Main.masterMode && MasterAmount.HasValue) {
				return $"{MasterAmount.Value:P0}";
			}
			if (Main.expertMode && ExpertAmount.HasValue) {
				return $"{ExpertAmount.Value:P0}";
			}
			return $"{ClassicAmount:P0}";
		}
		public static implicit operator AssimilationAmount(float value) => new(value, value * 1.3f, value * 1.5f);
		public static implicit operator AssimilationAmount((float classic, float expert) value) => new(value.classic, value.expert);
		public static implicit operator AssimilationAmount((float classic, float expert, float master) value) => new(value.classic, value.expert, value.master);
		public static implicit operator AssimilationAmount(Func<Entity, Player, float> function) => new(function);
		public static bool operator ==(AssimilationAmount a, AssimilationAmount b) {
			if (a.Function is not null) {
				return b.Function is not null && a.Function == b.Function;
			}
			return a.ClassicAmount == b.ClassicAmount && a.ExpertAmount == b.ExpertAmount && a.MasterAmount == b.MasterAmount;
		}
		public static bool operator !=(AssimilationAmount a, AssimilationAmount b) => !(a == b);
		public override bool Equals(object obj) => obj is AssimilationAmount other && this == other;
		public override int GetHashCode() => HashCode.Combine(ClassicAmount, ExpertAmount, MasterAmount, Function);
	}

}
