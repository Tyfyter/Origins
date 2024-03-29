﻿using System.Collections.Generic;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Questing {
	public class Quest_Registry : ILoadable {
		public static Dictionary<string, int> QuestIDs { get; internal set; }
		public static List<Quest> Quests { get; internal set; }
		public static List<Quest> NetQuests { get; internal set; }
		/// <summary>
		/// </summary>
		/// <exception cref="KeyNotFoundException">
		/// The key does not exist in QuestIDs.
		/// </exception>
		public static Quest GetQuestByKey(string key) => QuestIDs.TryGetValue(key, out int type) ? Quests[type] : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");
		public static Quest GetQuestByType(int type) => Quests[type];
		public static void RegisterQuest(Quest quest) {
			Setup();
			quest.Type = Quests.Count;
			QuestIDs.Add(quest.FullName, quest.Type);
			Quests.Add(quest);

			if (quest.SaveToWorld) {
				Origins.instance.Logger.Info($"Added netQuest {quest.NameValue}");
				quest.NetID = NetQuests.Count;
				NetQuests.Add(quest);
			}
			if (!Main.dedServ) Language.GetOrRegister(quest.NameKey);
		}
		static void Setup() {
			if (QuestIDs is null) QuestIDs = new Dictionary<string, int>();
			if (Quests is null) Quests = new List<Quest>();
			if (NetQuests is null) NetQuests = new List<Quest>();
		}
		public void Load(Mod mod) {
			Setup();
		}

		public void Unload() {
			QuestIDs = null;
			Quests = null;
			NetQuests = null;
		}
	}
}
