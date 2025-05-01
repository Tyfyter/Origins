using Terraria.ModLoader;

namespace Origins.Journal {
	public interface IJournalEntrySource {
		public string EntryName { get; }
	}
	public interface IJournalEntrySource<TEntry> : IJournalEntrySource where TEntry : JournalEntry {
		string IJournalEntrySource.EntryName => ModContent.GetInstance<TEntry>().FullName;
	}
}
