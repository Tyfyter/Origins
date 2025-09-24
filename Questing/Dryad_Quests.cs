using AltLibrary.Common.Systems;
using Origins.Items.Other.Consumables;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static Terraria.GameContent.Bestiary.IL_BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions;

namespace Origins.Questing {
	public class Cleansing_Station_Quest : Quest {
		//backing field for Stage property
		int progress = 0;
		const int target = 50;
		//Stage property so changing quest stage also updates its event handlers
		public override bool Started => Stage > 0;
		public override bool Completed => Stage > 2;
		public override bool CanStart(NPC npc) {
			return npc.type == NPCID.Dryad && !ShowInJournal();
		}
		public override string GetInquireText(NPC npc) => Language.GetTextValue("Mods.Origins.Quests.Dryad.Cleansing_Station.Inquire");
		public override void OnAccept(NPC npc) {
			Stage = 1;// - set stage to 1 (kill harpies)
			Main.npcChatText = Language.GetTextValue("Mods.Origins.Quests.Dryad.Cleansing_Station.Start");
			LocalPlayerStarted = true;
		}
		public override bool CanComplete(NPC npc) {
			return npc.type == NPCID.Dryad && Stage == 2;
		}
		public override string ReadyToCompleteText(NPC npc) => Language.GetOrRegister("Mods.Origins.Quests.Dryad.Cleansing_Station.ReadyToComplete").Value;
		public override void OnComplete(NPC npc) {
			Main.npcChatText = Language.GetTextValue("Mods.Origins.Quests.Dryad.Cleansing_Station.Complete");
			Stage = 3;
			ShouldSync = true;
			Main.LocalPlayer.QuickSpawnItem(npc.GetSource_GiftOrReward(FullName), ModContent.ItemType<Mojo_Flask>());
		}
		public override bool ShowInJournal() => Completed || (base.ShowInJournal() && LocalPlayerStarted);
		public override string GetJournalPage() {
			return Language.GetTextValue(
				"Mods.Origins.Quests.Dryad.Cleansing_Station.Journal", //translation key
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
					HasNotification = true;
				}
				ShouldSync = true;
			}
		}
    }
	public class Bloombomb_Quest : Quest {
		public const string loc_key = "Mods.Origins.Quests.Dryad.Bloombomb.";
		//backing field for Stage property
		int progress = 0;
		const int target = 200;
		//Stage property so changing quest stage also updates its event handlers
		public override bool Started => Stage > 0;
		public override bool Completed => Stage > 2;
		public override bool CanStart(NPC npc) {
			return npc.type == NPCID.Dryad && ModContent.GetInstance<Cleansing_Station_Quest>().Completed && !ShowInJournal();
		}
		public override string GetInquireText(NPC npc) => Language.GetTextValue(loc_key + "Inquire");
		public override void OnAccept(NPC npc) {
			Stage = 1;// - set stage to 1 (kill harpies)
			Main.npcChatText = Language.GetTextValue(loc_key + "Start");
			LocalPlayerStarted = true;
		}
		public override bool CanComplete(NPC npc) {
			return npc.type == NPCID.Dryad && Stage == 2;
		}
		public override string ReadyToCompleteText(NPC npc) => Language.GetOrRegister(loc_key + "ReadyToComplete").Value;
		public override void OnComplete(NPC npc) {
			Main.npcChatText = Language.GetTextValue(loc_key + "Complete");
			Stage = 3;
			ShouldSync = true;
		}
		public override bool ShowInJournal() => Completed || (base.ShowInJournal() && LocalPlayerStarted);
		public override string GetJournalPage() {
			return Language.GetTextValue(
				loc_key + "Journal", //translation key
				progress,
				target,
				StageTagOption(Stage >= 2) //used in a quest stage tag to show the stage as completed
			);
		}
		public override void SetStaticDefaults() {
			NameKey = loc_key + "Name";
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
					HasNotification = true;
				}
				ShouldSync = true;
			}
		}
    }
}
