using Origins.World.BiomeData;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;

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
	public class DropInstancedPerClient(int itemId, int chanceDenominator = 1, int amountDroppedMinimum = 1, int amountDroppedMaximum = 1, IItemDropRuleCondition condition = null) : CommonDrop(itemId, chanceDenominator, amountDroppedMinimum, amountDroppedMaximum) {
		public IItemDropRuleCondition condition = condition;
		public override bool CanDrop(DropAttemptInfo info) => condition is null || condition.CanDrop(info);
		public override ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info) {
			ItemDropAttemptResult result;
			if (info.rng.Next(chanceDenominator) < chanceNumerator) {
				NPC npc = info.npc;
				int stack = info.rng.Next(amountDroppedMinimum, amountDroppedMaximum + 1);
				if (Main.netMode == NetmodeID.Server && npc is not null) {
					int num = Item.NewItem(npc.GetSource_Loot(), (int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, itemId, stack, noBroadcast: true, -1);
					Main.timeItemSlotCannotBeReusedFor[num] = 54000;
					foreach (Player player in Main.ActivePlayers) {
						if (npc.playerInteraction[player.whoAmI] && CanDropForPlayer(player)) {
							NetMessage.SendData(MessageID.InstancedItem, player.whoAmI, -1, null, num);
						}
					}
					Main.item[num].active = false;
				} else {
					if (CanDropForPlayer(Main.LocalPlayer)) CommonCode.DropItem(info, itemId, stack);
				}
				result = default;
				result.State = ItemDropAttemptResultState.Success;
				return result;
			}
			result = default;
			result.State = ItemDropAttemptResultState.FailedRandomRoll;
			return result;
		}
		public virtual bool CanDropForPlayer(Player player) => true;
	}
	public class Ashen_Key_Condition : IItemDropRuleCondition {
		public bool CanDrop(DropAttemptInfo info) {
			return info.npc.value > 0f && Main.hardMode && !info.IsInSimulation && info.player.InModBiome<Ashen_Biome>();
		}
		public bool CanShowItemDropInUI() => Main.hardMode;
		public string GetConditionDescription() {
			return Language.GetOrRegister("Mods.Origins.Conditions.BiomeKey").Format(Language.GetOrRegister("Ashen"));
		}
	}
	public class Dawn_Key_Condition : IItemDropRuleCondition {
#if false
		public bool CanDrop(DropAttemptInfo info) {
			return false && info.npc.value > 0f && Main.hardMode && !info.IsInSimulation && info.player.InModBiome<Defiled_Wastelands>(); //Dawn
		}
		public bool CanShowItemDropInUI() => Main.hardMode;
#else
		public bool CanDrop(DropAttemptInfo info) => false;
		public bool CanShowItemDropInUI() => false;
#endif
		public string GetConditionDescription() {
			return Language.GetOrRegister("Mods.Origins.Conditions.BiomeKey").Format(Language.GetOrRegister("Mods.Origins.Generic.Defiled_Wastelands"));
		}
    }
    public class Defiled_Key_Condition : IItemDropRuleCondition {
		public bool CanDrop(DropAttemptInfo info) {
			return info.npc.value > 0f && Main.hardMode && !info.IsInSimulation && info.player.InModBiome<Defiled_Wastelands>();
		}
		public bool CanShowItemDropInUI() => Main.hardMode;
		public string GetConditionDescription() {
			if (OriginsModIntegrations.CheckAprilFools()) return Language.GetOrRegister("Mods.Origins.Conditions.BiomeKeyNoThe").Format(Language.GetOrRegister("Defiled_Wastelands"));
			return Language.GetOrRegister("Mods.Origins.Conditions.BiomeKey").Format(Language.GetOrRegister("Defiled_Wastelands"));
		}
	}
	public class Dusk_Key_Condition : IItemDropRuleCondition {
#if false
		public bool CanDrop(DropAttemptInfo info) {
			return info.npc.value > 0f && Main.hardMode && !info.IsInSimulation && info.player.InModBiome<Dusk>();
		}
		public bool CanShowItemDropInUI() => Main.hardMode;
#else
		public bool CanDrop(DropAttemptInfo info) => false;
		public bool CanShowItemDropInUI() => false;
#endif
		public string GetConditionDescription() {
			return Language.GetOrRegister("Mods.Origins.Conditions.BiomeKey").Format(Language.GetOrRegister("Mods.Origins.Generic.Dusk"));
		}
	}
	public class Hell_Key_Condition : IItemDropRuleCondition {
#if false
		public bool CanDrop(DropAttemptInfo info) {
			return info.npc.value > 0f && Main.hardMode && !info.IsInSimulation && info.player.ZoneUnderworldHeight;
		}
		public bool CanShowItemDropInUI() => Main.hardMode;
#else
		public bool CanDrop(DropAttemptInfo info) => false;
		public bool CanShowItemDropInUI() => false;
#endif
		public string GetConditionDescription() {
			return Language.GetOrRegister("Mods.Origins.Conditions.BiomeKey").Format(Language.GetOrRegister("Mods.Origins.Generic.Defiled_Wastelands"));
		}
	}
	public class Mushroom_Key_Condition : IItemDropRuleCondition {
#if false
		public bool CanDrop(DropAttemptInfo info) {
			return info.npc.value > 0f && Main.hardMode && !info.IsInSimulation && info.player.ZoneGlowshroom;
		}
		public bool CanShowItemDropInUI() => Main.hardMode;
#else
		public bool CanDrop(DropAttemptInfo info) => false;
		public bool CanShowItemDropInUI() => false;
#endif
		public string GetConditionDescription() {
			return Language.GetOrRegister("Mods.Origins.Conditions.BiomeKey").Format(Language.GetOrRegister("Mods.Origins.Generic.Defiled_Wastelands"));
		}
	}
	public class Ocean_Key_Condition : IItemDropRuleCondition {
#if false
		public bool CanDrop(DropAttemptInfo info) {
			return info.npc.value > 0f && Main.hardMode && !info.IsInSimulation && info.player.ZoneBeach;
		}
		public bool CanShowItemDropInUI() => Main.hardMode;
#else
		public bool CanDrop(DropAttemptInfo info) => false;
		public bool CanShowItemDropInUI() => false;
#endif
		public string GetConditionDescription() {
			return Language.GetOrRegister("Mods.Origins.Conditions.BiomeKey").Format(Language.GetOrRegister("Mods.Origins.Generic.Defiled_Wastelands"));
		}
	}
	public class Riven_Key_Condition : IItemDropRuleCondition {
		public bool CanDrop(DropAttemptInfo info) {
			return info.npc.value > 0f && Main.hardMode && !info.IsInSimulation && info.player.InModBiome<Riven_Hive>();
		}
		public bool CanShowItemDropInUI() => Main.hardMode;
		public string GetConditionDescription() {
			return Language.GetOrRegister("Mods.Origins.Conditions.BiomeKey").Format(Language.GetOrRegister("Riven_Hive"));
		}
	}
	public class Brine_Key_Condition : IItemDropRuleCondition {
		public bool CanDrop(DropAttemptInfo info) {
			return info.npc.value > 0f && Main.hardMode && !info.IsInSimulation && info.player.InModBiome<Brine_Pool>();
		}
		public bool CanShowItemDropInUI() => Main.hardMode;
		public string GetConditionDescription() {
			return Language.GetOrRegister("Mods.Origins.Conditions.BiomeKey").Format(Language.GetOrRegister("Brine_Pool"));
		}
	}
	public class Lost_Picture_Frame_Condition : IItemDropRuleCondition {
		public bool CanDrop(DropAttemptInfo info) {
			return info.npc.value > 0f && Main.hardMode && !info.IsInSimulation && Brine_Pool.SpawnRates.IsInBrinePool(info.npc.position) && info.player.InModBiome<Brine_Pool>() && info.npc.AnyInteractions();
		}
		public bool CanShowItemDropInUI() => Main.hardMode;
		public string GetConditionDescription() {
			return Language.GetOrRegister("Mods.Origins.Conditions.BiomeKey").Format(Language.GetOrRegister("Brine_Pool"));
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
				return info.player.ZoneCorrupt || info.player.ZoneCrimson || info.player.InModBiome<Defiled_Wastelands>() || info.player.InModBiome<Riven_Hive>() || info.player.InModBiome<Ashen_Biome>();
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
		public bool CanDrop(DropAttemptInfo info) {
			for (int i = 0; i < ChainedRules.Count; i++) {
				if (ChainedRules[i].RuleToChain.CanDrop(info)) return true;
			}
			return false;
		}
		public void ReportDroprates(List<DropRateInfo> drops, DropRateInfoChainFeed ratesInfo) {
			List<IItemDropRuleCondition> addConditions = [];
			for (int i = 0; i < ChainedRules.Count; i++) {
				List<DropRateInfo> _drops = [];
				ChainedRules[i].RuleToChain.ReportDroprates(_drops, ratesInfo);
				for (int j = 0; j < _drops.Count; j++) {
					addConditions = _drops[j].conditions;
					if ((addConditions?.Count ?? 0) != 0) break;
				}
			}

			ratesInfo.conditions = (ratesInfo.conditions ?? []).Concat(addConditions ?? []).ToList();
			drops.Add(new DropRateInfo(iconicItem, 1, 1, ratesInfo.parentDroprateChance, ratesInfo.conditions));
		}
		public ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info) {
			ItemDropAttemptResult result = default;
			result.State = ItemDropAttemptResultState.Success;
			return result;
		}
	}
}
