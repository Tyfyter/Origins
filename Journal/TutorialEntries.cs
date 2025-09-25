using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Journal; 
public abstract class TutorialJournalEntry : JournalEntry {
	public override JournalSortIndex SortIndex => new("µTutorial", 0);
}
public class Explosive_Weapons_Entry : TutorialJournalEntry {
	public void AddEntryToItems() {
		for (int i = 0; i < ItemLoader.ItemCount; i++) {
			if (ContentSamples.ItemsByType[i].CountsAsClass<Explosive>()) AddJournalEntry(ref OriginsSets.Items.JournalEntries[i], FullName);
		}
	}
}
