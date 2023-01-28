using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace Origins.Questing {
    public class Blue_Bovine_Quest : Quest {
		//backing field for Stage property
		int stage = 0;
		int progress = 0;
		const int target = 12;
		//Stage property so changing quest stage also updates its event handlers
		public override int Stage {
			get => stage;
			set {
				stage = value;
				// clear kill event handlers so they only run if the quest is active
				KillEnemyEvent = null;
				switch (stage) {
					case 1:
					KillEnemyEvent = (npc) => { // when the player kills something - 
						if (npc.type == NPCID.Harpy) {// - if it's a harpy -
							if (++progress >= target) { // - increment "progress", if the resulting value is at least "target" - 
								Stage = 2; // - set stage to 2 (killed enough harpies)
							}
						}
					};
					break;
				}
			}
		}
		public override bool Started => Stage > 0;
		public override bool Completed => Stage > 2;
		public override bool HasStartDialogue(NPC npc) {
			return npc.type == NPCID.Merchant && Stage == 0;
		}
		public override bool HasDialogue(NPC npc) {
			if (npc.type != NPCID.Merchant) return false; // NPCs other than the merchant won't have any dialogue related to this quest
			switch (Stage) {
				case 2: // killed enough harpies
				return true;
			}
			return false;
		}
		public override string GetDialogue() {
			switch (Stage) {
				case 2:
				return "Complete Quest";

				default:
				if (Origins.npcChatQuestSelected) {
					return "Accept";
				}
				return Language.GetTextValue(NameKey);
			}
		}
		//when the player clicks the dialogue button - 
		public override void OnDialogue() {
			// - if they're on -
			switch (stage) {
				case 0: {// - stage 0 (not started) - 
					if (Origins.npcChatQuestSelected) {// - if the player has already inquired about a quest -
						Stage = 1;// - set stage to 1 (kill harpies)
					} else {// - otherwise -
						// - set npc chat text to "start" text and mark that the player has inquired about a quest
						Main.npcChatText = Language.GetTextValue("Mods.Origins.Quests.Merchant.Blue_Bovine.Start", Main.LocalPlayer.Get2ndPersonReference("casual"));
						Origins.npcChatQuestSelected = true;// (npcChatQuestSelected is reset to false when the player closes the dialogue box)
					}
					break;
				}
				case 2: {// - stage 2 (killed enough harpies) - 
					// - set npc chat text to "complete" text and quest stage to 3 (completed)
					Main.npcChatText = Language.GetTextValue("Mods.Origins.Quests.Merchant.Blue_Bovine.Complete");
					Stage = 3;
					break;
				}
			}
		}
		public override string GetJournalPage() {
			return Language.GetTextValue(
				"Mods.Origins.Quests.Merchant.Blue_Bovine.Journal", //translation key
				Main.LocalPlayer.Get2ndPersonReference("casual"), //gendered casual second person reference, at index 0 because it occurs in the start message, which is copied into the journal text, so it occurs at the same index in both
				progress,
				target,
				StageTagOption(progress >= target) //used in a quest stage tag to show the stage as completed
			);
		}
		public override void SetStaticDefaults() {
			NameKey = "Mods.Origins.Quests.Merchant.Blue_Bovine.Name";
		}
		public override void SaveData(TagCompound tag) {
			//save stage and kills
			tag.Add("Stage", Stage);
			tag.Add("Progress", progress);
		}
		public override void LoadData(TagCompound tag) {
			//load stage and kills, note that it uses the Stage property so that it sets the event handlers
			//SafeGet returns the default value (0 for ints) if the tag doesn't have the data
			Stage = tag.SafeGet<int>("Stage");
			progress = tag.SafeGet<int>("Progress");
		}
	}
}
