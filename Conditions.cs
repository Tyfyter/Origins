using Origins.Journal;
using Origins.World.BiomeData;
using System;
using System.Reflection;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins {
	public class RecipeConditions : ILoadable {
		public static Condition ShimmerTransmutation { get; private set; } = new Condition(Language.GetOrRegister("Mods.Origins.Conditions.ShimmerTransmutation"), () => false);
		public static Condition RivenWater { get; private set; } = new Condition(Language.GetOrRegister("Mods.Origins.Conditions.RivenWater"), () => Main.LocalPlayer.adjWater && Main.LocalPlayer.InModBiome<Riven_Hive>());
		public static Condition WithJournalEntry<TProvider>(string textKey = "Mods.Origins.Conditions.WithJournalEntry") where TProvider : class, IJournalEntrySource, ILoadable {
			return WithJournalEntry(ModContent.GetInstance<TProvider>().EntryName, textKey);
		}
		public static Condition WithJournalEntry(string entryName, string textKey = "Mods.Origins.Conditions.WithJournalEntry") {
			if (!Journal_Registry.Entries.TryGetValue(entryName, out JournalEntry entry)) throw new ArgumentException($"Entry name must be the full internal name of a journal entry, {entryName} is not", nameof(entryName));
			return new(Language.GetOrRegister(textKey).WithFormatArgs(entry.DisplayName), () => Main.LocalPlayer.OriginPlayer().unlockedJournalEntries.Contains(entry.FullName));
		}
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
		public readonly struct NotFromItemsCondition : IItemDropRuleCondition {
			public readonly bool CanDrop(DropAttemptInfo info) => info.item <= ItemID.None;
			public readonly bool CanShowItemDropInUI() => false;
			public readonly string GetConditionDescription() => null;
		}
		public static IItemDropRuleCondition PlayerInteraction { get; private set; } = new PlayerInteractionCondition();
		public static IItemDropRuleCondition HardmodeBossBag { get; private set; } = new HardmodeBossBagCondition();
		public static NotFromItemsCondition NotFromItems => default;
		public void Load(Mod mod) { }
		public void Unload() {
			foreach (FieldInfo item in GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)) {
				item.SetValue(null, null);
			}
		}
	}
}
