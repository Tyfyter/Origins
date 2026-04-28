using Origins.Items.Materials;
using Origins.Journal;
using Terraria;
using Terraria.ID;
using JournalSortIndex = Origins.Journal.JournalEntry.JournalSortIndex;

namespace Origins.Items.Other.Consumables;
public class Classified_Blueprint : MaterialItem, IJournalEntryProvider {
	public override int ResearchUnlockCount => 1;
	public override int Rare => ItemRarityID.Orange;
	public override int Value => Item.sellPrice(gold: 3);
	public override bool Hardmode => true;
	public static Condition Condition { get; } = RecipeConditions.WithJournalEntry<Classified_Blueprint>($"Mods.Origins.Items.{nameof(Classified_Blueprint)}.RecipeCondition");
}
