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
	public static class LootFixers {
		public static void WorldEvilFixer(List<IItemDropRule> dropRules, Action<ItemDropWithConditionRule> onCorruptRule = null) {
			IItemDropRule entry;
			int len = dropRules.Count;
			for (int i = 0; i < len; i++) {
				entry = dropRules[i];
				if (entry is ItemDropWithConditionRule rule) {
					if (rule.condition is Conditions.IsCorruption) {
						rule.condition = new IsWorldEvil(OriginSystem.evil_corruption);
					} else if (rule.condition is Conditions.IsCrimson) {
						rule.condition = new IsWorldEvil(OriginSystem.evil_crimson);
					} else if (rule.condition is Conditions.IsCorruptionAndNotExpert) {
						rule.condition = new IsWorldEvilAndNotExpert(OriginSystem.evil_corruption);
						if(onCorruptRule is not null) onCorruptRule(rule);
					} else if (rule.condition is Conditions.IsCrimsonAndNotExpert) {
						rule.condition = new IsWorldEvilAndNotExpert(OriginSystem.evil_crimson);
					}
				} else if (entry is LeadingConditionRule leadingRule) {
					if (leadingRule.condition is Conditions.IsCorruption) {
						leadingRule.condition = new IsWorldEvil(OriginSystem.evil_corruption);
					} else if (leadingRule.condition is Conditions.IsCrimson) {
						leadingRule.condition = new IsWorldEvil(OriginSystem.evil_crimson);
					} else if (leadingRule.condition is Conditions.IsCorruptionAndNotExpert) {
						leadingRule.condition = new IsWorldEvilAndNotExpert(OriginSystem.evil_corruption);
					} else if (leadingRule.condition is Conditions.IsCrimsonAndNotExpert) {
						leadingRule.condition = new IsWorldEvilAndNotExpert(OriginSystem.evil_crimson);
					}
				}
			}
		}
	}
	public class IsWorldEvil : IItemDropRuleCondition {
		int worldEvil;
		public IsWorldEvil(int worldEvil) {
			this.worldEvil = worldEvil;
		}
		public bool CanDrop(DropAttemptInfo info) {
			return ModContent.GetInstance<OriginSystem>().worldEvil == worldEvil;
		}

		public bool CanShowItemDropInUI() {
			return ModContent.GetInstance<OriginSystem>().worldEvil == worldEvil;
		}

		public string GetConditionDescription() {
			return worldEvil switch {
				OriginSystem.evil_corruption => Language.GetTextValue("Bestiary_ItemDropConditions.IsCorruption"),
				OriginSystem.evil_crimson => Language.GetTextValue("Bestiary_ItemDropConditions.IsCrimson"),
				OriginSystem.evil_wastelands => Language.GetTextValue("Bestiary_ItemDropConditions.IsCorruption"),
				OriginSystem.evil_riven => Language.GetTextValue("Bestiary_ItemDropConditions.IsCrimson"),
				_ => Language.GetTextValue("Bestiary_ItemDropConditions.IsCorruption")
			};
		}
	}
	public class IsWorldEvilAndNotExpert : IItemDropRuleCondition {
		int worldEvil;
		public IsWorldEvilAndNotExpert(int worldEvil) {
			this.worldEvil = worldEvil;
		}
		public bool CanDrop(DropAttemptInfo info) {
			return !Main.expertMode && ModContent.GetInstance<OriginSystem>().worldEvil == worldEvil;
		}

		public bool CanShowItemDropInUI() {
			return !Main.expertMode && ModContent.GetInstance<OriginSystem>().worldEvil == worldEvil;
		}

		public string GetConditionDescription() {
			return worldEvil switch {
				OriginSystem.evil_corruption => Language.GetTextValue("Bestiary_ItemDropConditions.IsCorruptionAndNotExpert"),
				OriginSystem.evil_crimson => Language.GetTextValue("Bestiary_ItemDropConditions.IsCrimsonAndNotExpert"),
				OriginSystem.evil_wastelands => Language.GetTextValue("Bestiary_ItemDropConditions.IsCorruptionAndNotExpert"),
				OriginSystem.evil_riven => Language.GetTextValue("Bestiary_ItemDropConditions.IsCrimsonAndNotExpert"),
				_ => Language.GetTextValue("Bestiary_ItemDropConditions.IsCorruptionAndNotExpert")
			};
		}
	}
	public class DefiledKeyCondition : IItemDropRuleCondition {
		public bool CanDrop(DropAttemptInfo info) {
			return info.npc.value > 0f && Main.hardMode && !info.IsInSimulation && info.player.GetModPlayer<OriginPlayer>().ZoneDefiled;
		}

		public bool CanShowItemDropInUI() {
			return true;
		}

		public string GetConditionDescription() {
			return Language.GetTextValue("Bestiary_ItemDropConditions.DesertKeyCondition");
		}
	}
	public class SoulOfNight : IItemDropRuleCondition, IProvideItemConditionDescription {
		public bool CanDrop(DropAttemptInfo info) {
			if (Conditions.SoulOfWhateverConditionCanDrop(info)) {
				return info.player.ZoneCorrupt || info.player.ZoneCrimson || info.player.InModBiome<Defiled_Wastelands>() || info.player.InModBiome<Riven_Hive>();
			}
			return false;
		}

		public bool CanShowItemDropInUI() {
			return true;
		}

		public string GetConditionDescription() {
			return Language.GetTextValue("Mods.Origins.ItemDropConditions.SoulOfNight");
		}
	}
}
