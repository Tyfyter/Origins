using AltLibrary.Common.Systems;
using Origins.Items;
using Origins.Items.Weapons.Ranged;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Origins.Questing {
	public class Cleansing_Station_Quest : Quest {
		//backing field for Stage property
		int progress = 0;
		const int target = 50;
		//Stage property so changing quest stage also updates its event handlers
		public override bool SaveToWorld => true;
		public override bool Started => Stage > 0;
		public override bool Completed => Stage > 2;
		public override bool HasStartDialogue(NPC npc) {
			return npc.type == NPCID.Dryad && !ShowInJournal();
		}
		public override bool HasDialogue(NPC npc) {
			if (npc.type != NPCID.Dryad) return false; // NPCs other than the merchant won't have any dialogue related to this quest
			switch (Stage) {
				case 1:
				return !LocalPlayerStarted;
				case 2: // killed enough enemies
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
			switch (Stage) {
				case 0: {// - stage 0 (not started) - 
					if (Origins.npcChatQuestSelected) {// - if the player has already inquired about a quest -
						Stage = 1;// - set stage to 1 (kill harpies)
						LocalPlayerStarted = true;
					} else {// - otherwise -
							// - set npc chat text to "start" text and mark that the player has inquired about a quest
						Main.npcChatText = Language.GetTextValue("Mods.Origins.Quests.Dryad.Cleansing_Station.Start", Main.LocalPlayer.name, WorldBiomeManager.GetWorldEvil(true).DisplayName);
						Origins.npcChatQuestSelected = true;// (npcChatQuestSelected is reset to false when the player closes the dialogue box)
					}
					break;
				}
				case 1: {
					if (!LocalPlayerStarted) goto case 0;
					break;
				}
				case 2: {// - stage 2 (killed enough harpies) - 
						 // - set npc chat text to "complete" text and quest stage to 3 (completed)

					Main.npcChatText = Language.GetTextValue("Mods.Origins.Quests.Dryad.Cleansing_Station.Complete");
					Stage = 3;
					ShouldSync = true;
					break;
				}
			}
		}
		public override bool ShowInJournal() => base.ShowInJournal() && LocalPlayerStarted;
		public override string GetJournalPage() {
			return Language.GetTextValue(
				"Mods.Origins.Quests.Dryad.Cleansing_Station.Journal", //translation key
				Main.LocalPlayer.name,
				WorldBiomeManager.GetWorldEvil(true).DisplayName,
				progress,
				target,
				StageTagOption(Stage >= 2) //used in a quest stage tag to show the stage as completed
			);
		}
		public override void SetStaticDefaults() {
			NameKey = "Mods.Origins.Quests.Dryad.Cleansing_Station.Name";
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
		public override void SendSync(BinaryWriter writer) {
			writer.Write(Stage);
			writer.Write(progress);
		}
		public override void ReceiveSync(BinaryReader reader) {
			Stage = reader.ReadInt32();
			progress = reader.ReadInt32();
		}
		public void UpdateProgress(int amount) {
			if (Stage == 1) {
				progress += amount;
				if (progress >= target) {
					Stage = 2;
				}
				ShouldSync = true;
			}
		}
	}
}
