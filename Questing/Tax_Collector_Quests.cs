using Origins.NPCs.MiscE.Quests;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Origins.Questing {
	public class Tax_Collector_Ghosts_Quest : Quest {
		public const string loc_key = "Mods.Origins.Quests.TaxCollector.Tax_Collector_Ghosts_Quest.";
		public override bool SaveToWorld => true;
		//backing field for Stage property
		int stage = 0;
		//Stage property so changing quest stage also updates its event handlers
		public override int Stage {
			get => stage;
			set {
				stage = value;
				// clear event handlers so they only run if the quest is active
				KillEnemyEvent = null;
				switch (stage) {
					case 1:
					KillEnemyEvent = OnKill<Jacob_Marley>(2);
					break;
					case 2:
					KillEnemyEvent = OnKill<Spirit_Of_Christmas_Past>(3);
					break;
					case 3:
					KillEnemyEvent = OnKill<Spirit_Of_Christmas_Present>(4);
					break;
					case 4:
					KillEnemyEvent = OnKill<Spirit_Of_Christmas_Future>(5);
					break;
				}
			}
		}
		Action<NPC> OnKill<T>(int stage) where T : ModNPC {
			int type = ModContent.NPCType<T>();
			return npc => {
				 if (Stage < stage && npc.type == type) {
					Stage = stage;
					ShouldSync = true;
				}
			};
		}
		public override bool Started => Stage > 0;
		public override bool Completed => Stage > 5;
		public override bool CanStart(NPC npc) => Questing.QuestListSelected && Questing.fromQuest is Tax_Collector_Quests && npc.type == NPCID.TaxCollector && !ShowInJournal();
		public override string GetInquireText(NPC npc) => Language.GetOrRegister(loc_key + "Inquire").Value;
		public override void OnAccept(NPC npc) {
			Stage = 1;// - set stage to 1 (kill ghosts)
			Main.npcChatText = Language.GetTextValue(loc_key + "Start");
			LocalPlayerStarted = true;
		}
		public override bool CanComplete(NPC npc) {
			return npc.type == NPCID.TaxCollector && Stage == 5;
		}
		public override string ReadyToCompleteText(NPC npc) => Language.GetOrRegister(loc_key + "ReadyToComplete").Value;
		public override void OnComplete(NPC npc) {
			Main.npcChatText = Language.GetTextValue(loc_key + "Complete");
			Stage = 6;
			ShouldSync = true;
		}
		public override bool ShowInJournal() => Completed || (base.ShowInJournal() && LocalPlayerStarted);
		public override string GetJournalPage() {
			return Language.GetTextValue(
				loc_key + "Journal", //translation key
				Math.Min(Stage - 1, 4),
				StageTagOption(Stage > 4), //used in a quest stage tag to show the stage as completed
				StageTagOption(Stage > 5) //used in a quest stage tag to show the stage as completed
			);
		}
		public override void SetStaticDefaults() {
			NameKey = loc_key + "Name";
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
		public override void SendSync(BinaryWriter writer) {
			writer.Write(Stage);
		}
		public override void ReceiveSync(BinaryReader reader) {
			Stage = reader.ReadInt32();
		}
		public static void GetTime(out int hour, out int minute) {
			double time = Main.time;
			if (!Main.dayTime) {
				time += 54000.0;
			}
			time = time / 86400.0 * 24.0;
			time = time - 7.5 - 12.0;
			if (time < 0) {
				time += 24.0;
			}
			int intTime = (int)time;
			double deltaTime = time - intTime;
			hour = intTime;
			minute = (int)(deltaTime * 60.0);
		}
	}
}