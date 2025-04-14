namespace Origins.Journal {
	public interface IJournalEntrySource {
		public string EntryName { get; }
	}
	//internal because it assumes the journal entry is from TO
	internal interface IJournalEntrySource<TEntry> : IJournalEntrySource where TEntry : JournalEntry {
		string IJournalEntrySource.EntryName => "Origins/" + typeof(TEntry).Name;
	}
}
