using Terraria.ID;

namespace Origins.Journal {
	public class Fallen_Star_Entry : VanillaItemJournalEntry {
		public override int ItemType => ItemID.FallenStar;
		public override JournalSortIndex SortIndex => new("Arabel", 2);
	}

	public class Life_Fruit_Entry : VanillaItemJournalEntry {
		public override int ItemType => ItemID.LifeFruit;
		public override JournalSortIndex SortIndex => new("Birth_Of_The_Gods", 1);
	}
	public class Broken_Hero_Sword_Entry : VanillaItemJournalEntry {
		public override int ItemType => ItemID.BrokenHeroSword;
		public override JournalSortIndex SortIndex => new("Birth_Of_The_Gods", 2);
	}
	public class Luminite_Entry : VanillaItemJournalEntry {
		public override int ItemType => ItemID.LunarOre;
		public override JournalSortIndex SortIndex => new("Birth_Of_The_Gods", 3);
	}

	public class Neptunes_Shell_Entry : VanillaItemJournalEntry {
		public override int ItemType => ItemID.NeptunesShell;
		public override JournalSortIndex SortIndex => new("Ocean_Tribe", 1);
	}
	public class Shark_Entry : VanillaNPCJournalEntry {
		public override int NPCType => NPCID.Shark;
		public override JournalSortIndex SortIndex => new("Ocean_Tribe", 2);
	}
	public class Shell_Pile_Entry : VanillaItemJournalEntry {
		public override int ItemType => ItemID.ShellPileBlock;
		public override JournalSortIndex SortIndex => new("Ocean_Tribe", 3);
	}
	public class Diving_Helmet_Entry : VanillaItemJournalEntry {
		public override int ItemType => ItemID.DivingHelmet;
		public override JournalSortIndex SortIndex => new("Ocean_Tribe", 5);
	}

	public class Bladetongue_Entry : VanillaItemJournalEntry {
		public override int ItemType => ItemID.Bladetongue;
		public override JournalSortIndex SortIndex => new("The_Crimson", 1);
	}
	public class Vicious_Mushroom_Entry : VanillaItemJournalEntry {
		public override int ItemType => ItemID.ViciousMushroom;
		public override JournalSortIndex SortIndex => new("The_Crimson", 2);
	}
	public class The_Rotted_Fork_Entry : VanillaItemJournalEntry {
		public override int ItemType => ItemID.TheRottedFork;
		public override JournalSortIndex SortIndex => new("The_Crimson", 3);
	}
	public class Crimson_Heart_Entry : VanillaItemJournalEntry {
		public override int ItemType => ItemID.CrimsonHeart;
		public override JournalSortIndex SortIndex => new("The_Crimson", 4);
	}
	public class The_Undertaker_Entry : VanillaItemJournalEntry {
		public override int ItemType => ItemID.TheUndertaker;
		public override JournalSortIndex SortIndex => new("The_Crimson", 5);
	}
	public class Crimson_Rod_Entry : VanillaItemJournalEntry {
		public override int ItemType => ItemID.CrimsonRod;
		public override JournalSortIndex SortIndex => new("The_Crimson", 6);
	}
	public class Panic_Necklace_Entry : VanillaItemJournalEntry {
		public override int ItemType => ItemID.PanicNecklace;
		public override JournalSortIndex SortIndex => new("The_Crimson", 7);
	}
	public class Bone_Rattle_Entry : VanillaItemJournalEntry {
		public override int ItemType => ItemID.BoneRattle;
		public override JournalSortIndex SortIndex => new("The_Crimson", 8);
	}
	public class Tissue_Sample_Entry : VanillaItemJournalEntry {
		public override int ItemType => ItemID.TissueSample;
		public override JournalSortIndex SortIndex => new("The_Crimson", 9);
	}
	public class Ichor_Entry : VanillaItemJournalEntry {
		public override int ItemType => ItemID.Ichor;
		public override JournalSortIndex SortIndex => new("The_Crimson", 12);
	}
	public class Brain_Of_Cthulhu_Entry : VanillaNPCJournalEntry {
		public override int NPCType => NPCID.BrainofCthulhu;
		public override JournalSortIndex SortIndex => new("The_Crimson", 13);
	}
}
