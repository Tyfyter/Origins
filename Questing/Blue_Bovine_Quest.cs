using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace Origins.Questing {
    public class Blue_Bovine_Quest : Quest {
		int stage = 0;
		int progress = 0;
		const int target = 12;
		public override int Stage {
			get => stage;
			set {
				stage = value;
				KillEnemyEvents = null;
				switch (stage) {
					case 1:
					KillEnemyEvents.Add((npc) => {
						if (npc.type == NPCID.Harpy) {
							if (++progress >= target) {
								Stage = 2;
							}
						}
					});
					break;
				}
			}
		}
		public override bool Started => Stage > 0;
		public override bool Completed => Stage > 2;
		public override bool HasDialogue(NPC npc) {
			if (npc.type != NPCID.Merchant) return false;
			return !Started || !Completed;
		}
		public override string GetDialogue() {
			if (Origins.npcChatQuestSelected) {
				return "Accept";
			}
			return "Quest";
		}
		public override void OnDialogue() {
			switch (stage) {
				case 0: {
					if (Origins.npcChatQuestSelected) {
						Stage = 1;
					} else {
						Main.npcChatText = Language.GetTextValue("Mods.Origins.Quests.Merchant.Blue_Bovine.Start", Main.LocalPlayer.Get2ndPersonReference("casual"));
						Origins.npcChatQuestSelected = true;
					}
					break;
				}
				case 2: {
					Main.npcChatText = Language.GetTextValue("Mods.Origins.Quests.Merchant.Blue_Bovine.Complete");
					Stage = 3;
					break;
				}
			}
		}
		public override string GetJournalPage() {
			return Language.GetTextValue("Mods.Origins.Quests.Merchant.Blue_Bovine.Journal", Main.LocalPlayer.Get2ndPersonReference("casual"), progress, target, progress < target ? "00" : "55");
		}
		public override void SaveData(TagCompound tag) {
			tag.Add("Stage", Stage);
			tag.Add("Progress", progress);
		}
		public override void LoadData(TagCompound tag) {
			Stage = tag.SafeGet<int>("Stage");
			progress = tag.SafeGet<int>("Progress");
		}
	}
}
