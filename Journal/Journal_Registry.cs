﻿using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ModLoader;

namespace Origins.Journal {
	public class Journal_Registry : ILoadable {
		public static Dictionary<string, JournalEntry> Entries { get; internal set; }
		public static List<JournalEntry> OrderedEntries { get; internal set; }
		public static JournalEntry GetJournalEntryByTextKey(string key) {
			return Entries.Values.Where((e) => e.TextKey == key).FirstOrDefault();
		}
		public static void SetupContent() {
			OrderedEntries = Entries.Values.Order().ToList();
		}
		public void Load(Mod mod) {
			On_NPCWasChatWithTracker.RegisterChatStartWith += On_NPCWasChatWithTracker_RegisterChatStartWith;
		}
		static void On_NPCWasChatWithTracker_RegisterChatStartWith(On_NPCWasChatWithTracker.orig_RegisterChatStartWith orig, NPCWasChatWithTracker self, NPC npc) {
			if (!string.IsNullOrWhiteSpace(OriginsSets.NPCs.JournalEntries[npc.type])) Main.LocalPlayer.OriginPlayer().UnlockJournalEntry(OriginsSets.NPCs.JournalEntries[npc.type]);
			orig(self, npc);
		}
		public void Unload() {
			Entries = null;
			OrderedEntries = null;
		}
	}
}
