using System;
using Terraria.ModLoader;
using static Origins.Journal.JournalEntry;

namespace Origins.Journal {
	public interface IJournalEntrySource {
		public string EntryName { get; }
	}
	public interface IJournalEntrySource<TEntry> : IJournalEntrySource where TEntry : JournalEntry {
		string IJournalEntrySource.EntryName => ModContent.GetInstance<TEntry>().FullName;
	}
	public interface IJournalEntryProvider : IModType, IJournalEntrySource, IAutoload<IJournalEntryProvider.AutoloadImpl> {
		string IJournalEntrySource.EntryName => $"{Mod.Name}/{GetType().Name}_Entry";
		public virtual static JournalSortIndex SortIndex => new("µUncategorized", 0);
		class AutoloadImpl : IAutoloader {
			public static void Autoload(Mod mod, Type type) {
				mod.AddContent((JournalEntry)Activator.CreateInstance(typeof(Automatic_Entry<>).MakeGenericType(type)));
			}
		}
		public class Automatic_Entry<T> : JournalEntry where T : IJournalEntryProvider {
			public override string Name => typeof(T).Name + "_Entry";
			public override string TextKey => typeof(T).Name;
			public override JournalSortIndex SortIndex => T.SortIndex;
		}
	}
}
