﻿using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;

namespace Origins.Journal {
	public class Journal_Registry : ILoadable {
		public static Dictionary<string, JournalEntry> Entries { get; internal set; }
		public static JournalEntry GetJournalEntryByTextKey(string key) {
			return Entries.Values.Where((e) => e.TextKey == key).FirstOrDefault();
		}

		public void Load(Mod mod) { }

		public void Unload() {
			Entries = null;
		}
	}
}
