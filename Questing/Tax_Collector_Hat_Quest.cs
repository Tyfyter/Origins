using Terraria;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace Origins.Questing {
	public class Tax_Collector_Hat_Quest : Quest {
		public override void Load() {
			Terraria.On_NPC.UsesPartyHat += (orig, self) => {
				if (self.type == NPCID.TaxCollector) {
					if (self.ForcePartyHatOn) {
						return true;
					}
					if (self.IsABestiaryIconDummy) {
						return false;
					}
					return BirthdayParty.PartyIsUp && (OriginSystem.Instance?.taxCollectorWearsPartyhat ?? false);
				}
				return orig(self);
			};
		}
		//backing field for Stage property
		int stage = 0;
		//Stage property so changing quest stage also updates its event handlers
		public override int Stage {
			get => stage;
			set {
				stage = value;
				// clear inventory event handlers so they only run if the quest is active
				PreUpdateInventoryEvent = null;
				UpdateInventoryEvent = null;
				switch (stage) {
					case 1:
					if (OriginSystem.Instance?.taxCollectorWearsPartyhat ?? false) {// in case someone somehow 
						Stage = 2;
					}
					break;
				}
			}
		}
		public override bool Started => Stage > 0;
		public override bool Completed => Stage > 2;
		public override bool HasStartDialogue(NPC npc) {
			return npc.type == NPCID.PartyGirl &&
				true && // replace with condition for prerequisite quests
				NPC.AnyNPCs(NPCID.TaxCollector);
		}
		public override bool HasDialogue(NPC npc) {
			if (npc.type != NPCID.PartyGirl && npc.type != NPCID.TaxCollector) return false;
			switch (Stage) {
				case 2: // task completed
				return npc.type == NPCID.PartyGirl;

				case 1: // task given
				return npc.type == NPCID.TaxCollector;
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
						Main.npcChatText = Language.GetTextValue("Mods.Origins.Quests.Party_Girl.Tax_Collector_Hat.Start", Main.LocalPlayer.Get2ndPersonReference("casual"));
						Origins.npcChatQuestSelected = true;//(npcChatQuestSelected is reset to false when the player closes the dialogue box)
					}
					break;
				}
				case 2: {// - stage 2 (killed enough harpies) - 
						 // - set npc chat text to "complete" text and quest stage to 3 (completed)
					Main.npcChatText = Language.GetTextValue("Mods.Origins.Quests.Party_Girl.Tax_Collector_Hat.Complete");
					Stage = 3;
					break;
				}
			}
		}
		public override string GetJournalPage() {
			return Language.GetTextValue(
				"Mods.Origins.Quests.Party_Girl.Tax_Collector_Hat.Journal", //translation key
				Main.LocalPlayer.Get2ndPersonReference("casual"), //gendered casual second person reference, at index 0 because it occurs in the start message, which is copied into the journal text, so it occurs at the same index in both
				Stage > 1 ? "" : "/completed" //used in a quest stage tag to show the stage as completed
			);
		}
		public override void SetStaticDefaults() {
			NameKey = "Mods.Origins.Quests.Party_Girl.Tax_Collector_Hat.Name";
		}
		public override void SaveData(TagCompound tag) {
			//save stage and kills
			tag.Add("Stage", Stage);
		}
		public override void LoadData(TagCompound tag) {
			//load stage and kills, note that it uses the Stage property so that it sets the event handlers
			//SafeGet returns the default value (0 for ints) if the tag doesn't have the data
			Stage = tag.SafeGet<int>("Stage");
		}
	}
}
