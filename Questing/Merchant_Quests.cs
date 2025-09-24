using System.IO;
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
							ShouldSync = true;
							if (++progress >= target) { // - increment "progress", if the resulting value is at least "target" - 
								HasNotification = true;
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
		public override bool CanStart(NPC npc) {
			return npc.type == NPCID.Merchant && Stage == 0;
		}
		public override string GetInquireText(NPC npc) => Language.GetTextValue("Mods.Origins.Quests.Merchant.Blue_Bovine.Inquire", Main.LocalPlayer.Get2ndPersonReference("casual"));
		public override void OnAccept(NPC npc) {
			Stage = 1;
			Main.npcChatText = Language.GetTextValue("Mods.Origins.Quests.Merchant.Blue_Bovine.Start");
			ShouldSync = true;
		}
		public override bool CanComplete(NPC npc) => npc.type == NPCID.Merchant && Stage == 2;
		public override string ReadyToCompleteText(NPC npc) => Language.GetOrRegister("Mods.Origins.Quests.Merchant.Blue_Bovine.ReadyToComplete").Value;
		public override void OnComplete(NPC npc) {
			Main.npcChatText = Language.GetTextValue("Mods.Origins.Quests.Merchant.Blue_Bovine.Complete");
			Stage = 3;
			ShouldSync = true;
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
		public override void SendSync(BinaryWriter writer) {
			writer.Write(Stage);
			writer.Write(progress);
		}
		public override void ReceiveSync(BinaryReader reader) {
			Stage = reader.ReadInt32();
			progress = reader.ReadInt32();
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
	public class Lottery_Ticket_Quest : Quest {
		int stage = 0;
		int progress = 0;
		const int target = 10;
		public override int Stage {
			get => stage;
			set {
				stage = value;
				KillEnemyEvent = null;
				switch (stage) {
					case 1:
					KillEnemyEvent = (npc) => {
						if (npc.rarity >= 2) {
							ShouldSync = true;
							if (++progress >= target) {
								HasNotification = true;
								Stage = 2;
							}
						}
					};
					break;
				}
			}
		}
		public override bool Started => Stage > 0;
		public override bool Completed => Stage > 2;
		public override bool CanStart(NPC npc) {
			return npc.type == NPCID.Merchant && Stage == 0;
		}
		public override string GetInquireText(NPC npc) => Language.GetTextValue("Mods.Origins.Quests.Merchant.Lottery_Ticket.Inquire");
		public override void OnAccept(NPC npc) {
			Stage = 1;
			Main.npcChatText = Language.GetTextValue("Mods.Origins.Quests.Merchant.Lottery_Ticket.Start");
			ShouldSync = true;
		}
		public override bool CanComplete(NPC npc) => npc.type == NPCID.Merchant && Stage == 2;
		public override string ReadyToCompleteText(NPC npc) => Language.GetOrRegister("Mods.Origins.Quests.Merchant.Lottery_Ticket.ReadyToComplete").Value;
		public override void OnComplete(NPC npc) {
			Main.npcChatText = Language.GetTextValue("Mods.Origins.Quests.Merchant.Lottery_Ticket.Complete");
			Stage = 3;
			ShouldSync = true;
		}
		public override string GetJournalPage() {
			return Language.GetTextValue(
				"Mods.Origins.Quests.Merchant.Lottery_Ticket.Journal",
				progress,
				target,
				StageTagOption(progress >= target)
			);
		}
		public override void SetStaticDefaults() {
			NameKey = "Mods.Origins.Quests.Merchant.Lottery_Ticket.Name";
		}
		public override void SendSync(BinaryWriter writer) {
			writer.Write(Stage);
			writer.Write(progress);
		}
		public override void ReceiveSync(BinaryReader reader) {
			Stage = reader.ReadInt32();
			progress = reader.ReadInt32();
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
