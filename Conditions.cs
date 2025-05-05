using Origins.World.BiomeData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins {
	public class RecipeConditions : ILoadable {
		public static Condition ShimmerTransmutation { get; private set; } = new Condition(Language.GetOrRegister("Mods.Origins.Conditions.ShimmerTransmutation"), () => false);
		public static Condition RivenWater { get; private set; } = new Condition(Language.GetOrRegister("Mods.Origins.Conditions.RivenWater"), () => Main.LocalPlayer.adjWater && Main.LocalPlayer.InModBiome<Riven_Hive>());
		public void Load(Mod mod) { }
		public void Unload() {
			foreach (FieldInfo item in GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)) {
				item.SetValue(null, null);
			}
		}
	}
	public class DropConditions : ILoadable {
		public class PlayerInteractionCondition : IItemDropRuleCondition {
			public bool CanDrop(DropAttemptInfo info) => info.npc?.GetWereThereAnyInteractions() ?? true;
			public bool CanShowItemDropInUI() => true;
			public string GetConditionDescription() => null;
		}
		public class HardmodeBossBagCondition : IItemDropRuleCondition {
			public bool CanDrop(DropAttemptInfo info) => !ItemID.Sets.PreHardmodeLikeBossBag.IndexInRange(info.item) || !ItemID.Sets.PreHardmodeLikeBossBag[info.item] || Main.tenthAnniversaryWorld;
			public bool CanShowItemDropInUI() => true;
			public string GetConditionDescription() => Language.GetOrRegister("Mods.Origins.Conditions.HardmodeBossBag").Value;
		}
		public static IItemDropRuleCondition PlayerInteraction { get; private set; } = new PlayerInteractionCondition();
		public static IItemDropRuleCondition HardmodeBossBag { get; private set; } = new HardmodeBossBagCondition();
		public void Load(Mod mod) { }
		public void Unload() {
			foreach (FieldInfo item in GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)) {
				item.SetValue(null, null);
			}
		}
	}
}
