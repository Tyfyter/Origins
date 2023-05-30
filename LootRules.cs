using Origins.Tiles.Defiled;
using Origins.Tiles.Riven;
using Origins.World.BiomeData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.LootConditions {
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
			ItemDropAttemptResult result = default(ItemDropAttemptResult);
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
}
