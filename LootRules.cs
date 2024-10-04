using Origins.World.BiomeData;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace Origins.LootConditions {
	public class VaryingRateLeadingRule(int chanceDenominator, int chanceNumerator, params (IItemDropRuleCondition condition, int chanceDenominator, int chanceNumerator)[] alternates) : IItemDropRule {
		public List<IItemDropRuleChainAttempt> ChainedRules { get; } = [];
		public bool CanDrop(DropAttemptInfo info) => true;
		public void ReportDroprates(List<DropRateInfo> drops, DropRateInfoChainFeed ratesInfo) {
			int denominator = chanceDenominator;
			int numerator = chanceNumerator;
			for (int i = 0; i < alternates.Length; i++) {
				if (alternates[i].condition.CanShowItemDropInUI()) {
					denominator = alternates[i].chanceDenominator;
					numerator = alternates[i].chanceNumerator;
				}
			}
			Chains.ReportDroprates(ChainedRules, numerator / (float)denominator, drops, ratesInfo);
		}
		public ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info) {
			int denominator = chanceDenominator;
			int numerator = chanceNumerator;
			for (int i = 0; i < alternates.Length; i++) {
				if (alternates[i].condition.CanDrop(info)) {
					denominator = alternates[i].chanceDenominator;
					numerator = alternates[i].chanceNumerator;
				}
			}
			ItemDropAttemptResult result = default;
			if (info.player.RollLuck(denominator) < numerator) {
				result.State = ItemDropAttemptResultState.Success;
				return result;
			}
			result.State = ItemDropAttemptResultState.FailedRandomRoll;
			return result;
		}
	}
	public class LeadingSuccessRule : IItemDropRule {
		public List<IItemDropRuleChainAttempt> ChainedRules { get; }
		public LeadingSuccessRule() {
			ChainedRules = new List<IItemDropRuleChainAttempt>();
		}
		public bool CanDrop(DropAttemptInfo info) => true;
		public void ReportDroprates(List<DropRateInfo> drops, DropRateInfoChainFeed ratesInfo) {
			Chains.ReportDroprates(ChainedRules, 1f, drops, ratesInfo);
		}
		public ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info) {
			ItemDropAttemptResult result = default;
			result.State = ItemDropAttemptResultState.Success;
			return result;
		}
	}
	public class Dawn_Key_Condition : IItemDropRuleCondition {
		public bool CanDrop(DropAttemptInfo info) {
			return info.npc.value > 0f && Main.hardMode && !info.IsInSimulation && info.player.InModBiome<Defiled_Wastelands>(); //Dawn
		}
		public bool CanShowItemDropInUI() => Main.hardMode;
		public string GetConditionDescription() {
			return Language.GetTextValue("Bestiary_ItemDropConditions.DesertKeyCondition");
		}
	}
	public class Defiled_Key_Condition : IItemDropRuleCondition {
		public bool CanDrop(DropAttemptInfo info) {
			return info.npc.value > 0f && Main.hardMode && !info.IsInSimulation && info.player.InModBiome<Defiled_Wastelands>();
		}
		public bool CanShowItemDropInUI() => Main.hardMode;
		public string GetConditionDescription() {
			return Language.GetTextValue("Bestiary_ItemDropConditions.DesertKeyCondition");
		}
	}
	public class Dusk_Key_Condition : IItemDropRuleCondition {
		public bool CanDrop(DropAttemptInfo info) {
			return info.npc.value > 0f && Main.hardMode && !info.IsInSimulation && info.player.InModBiome<Dusk>();
		}
		public bool CanShowItemDropInUI() => Main.hardMode;
		public string GetConditionDescription() {
			return Language.GetTextValue("Bestiary_ItemDropConditions.DesertKeyCondition");
		}
	}
	public class Hell_Key_Condition : IItemDropRuleCondition {
		public bool CanDrop(DropAttemptInfo info) {
			return info.npc.value > 0f && Main.hardMode && !info.IsInSimulation && info.player.ZoneUnderworldHeight;
		}
		public bool CanShowItemDropInUI() => Main.hardMode;
		public string GetConditionDescription() {
			return Language.GetTextValue("Bestiary_ItemDropConditions.DesertKeyCondition");
		}
	}
	public class Mushroom_Key_Condition : IItemDropRuleCondition {
		public bool CanDrop(DropAttemptInfo info) {
			return info.npc.value > 0f && Main.hardMode && !info.IsInSimulation && info.player.ZoneGlowshroom;
		}
		public bool CanShowItemDropInUI() => Main.hardMode;
		public string GetConditionDescription() {
			return Language.GetTextValue("Bestiary_ItemDropConditions.DesertKeyCondition");
		}
	}
	public class Ocean_Key_Condition : IItemDropRuleCondition {
		public bool CanDrop(DropAttemptInfo info) {
			return info.npc.value > 0f && Main.hardMode && !info.IsInSimulation && info.player.ZoneBeach;
		}
		public bool CanShowItemDropInUI() => Main.hardMode;
		public string GetConditionDescription() {
			return Language.GetTextValue("Bestiary_ItemDropConditions.DesertKeyCondition");
		}
	}
	public class Riven_Key_Condition : IItemDropRuleCondition {
		public bool CanDrop(DropAttemptInfo info) {
			return info.npc.value > 0f && Main.hardMode && !info.IsInSimulation && info.player.InModBiome<Riven_Hive>();
		}
		public bool CanShowItemDropInUI() => Main.hardMode;
		public string GetConditionDescription() {
			return Language.GetTextValue("Bestiary_ItemDropConditions.DesertKeyCondition");
		}
	}
	public class DownedPlantera : IItemDropRuleCondition {
		public bool CanDrop(DropAttemptInfo info) {
			return NPC.downedPlantBoss;
		}
		public bool CanShowItemDropInUI() => Main.hardMode;
		public string GetConditionDescription() {
			return Language.GetTextValue("Mods.Origins.ItemDropConditions.DownedPlantBoss");
		}
	}
	public class SoulOfNight : IItemDropRuleCondition, IProvideItemConditionDescription {
		public bool CanDrop(DropAttemptInfo info) {
			if (Conditions.SoulOfWhateverConditionCanDrop(info)) {
				return info.player.ZoneCorrupt || info.player.ZoneCrimson || info.player.InModBiome<Defiled_Wastelands>() || info.player.InModBiome<Riven_Hive>();
			}
			return false;
		}
		public bool CanShowItemDropInUI() => Main.hardMode;
		public string GetConditionDescription() {
			return Language.GetTextValue("Mods.Origins.ItemDropConditions.SoulOfNight");
		}
	}
	public class AnyPlayerInteraction : IItemDropRuleCondition, IProvideItemConditionDescription {
		public bool CanDrop(DropAttemptInfo info) {
			return info.npc.AnyInteractions();
		}
		public bool CanShowItemDropInUI() => true;
		public string GetConditionDescription() {
			return "";
		}
	}
	public class DropAsSetRule(int iconicItem) : IItemDropRule {
		public List<IItemDropRuleChainAttempt> ChainedRules { get; } = [];
		public bool CanDrop(DropAttemptInfo info) => true;
		public void ReportDroprates(List<DropRateInfo> drops, DropRateInfoChainFeed ratesInfo) {
			drops.Add(new DropRateInfo(iconicItem, 1, 1, ratesInfo.parentDroprateChance, ratesInfo.conditions));
		}
		public ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info) {
			ItemDropAttemptResult result = default;
			result.State = ItemDropAttemptResultState.Success;
			return result;
		}
	}
}
