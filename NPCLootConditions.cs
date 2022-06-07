using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.LootConditions {
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
	public class DesertKeyCondition : IItemDropRuleCondition {
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
}
