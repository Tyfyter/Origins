using Terraria;
using Terraria.ID;
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
						Main.npcChatText = "Hey boy, I've had a bit of a vendetta against those harpies. Go and get twelve of 'em for me eh? I'll give you a nice reward if you do!";
						Origins.npcChatQuestSelected = true;
					}
					break;
				}
				case 2: {
					Main.npcChatText = "Thank you. Those feather-ridden sky-folk were disrupting me from getting an important commodity! I'll offer you this new item at a discounted price for your services!";
					Stage = 3;
					break;
				}
			}
		}
		public override string GetJournalPage() {
			return $"To the Skies!\nClient: Merchant\n\n'Hey boy, I've had a bit of a vendetta against those harpies. Go and get twelve of 'em for me eh? I'll give you a nice reward if you do!'\n\n - Slay 12 Harpies.\nStage {Stage}, {progress}/{target} Harpies slain\n\nUnlocks [Blue_Bovine.png]";
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
