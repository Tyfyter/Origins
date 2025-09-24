using Origins.Items.Accessories;
using Origins.Items.Materials;
using Origins.Items.Weapons.Melee;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Origins.Questing {
	public class Advanced_Imaging_Quest : Quest {
		public const string loc_key = "Mods.Origins.Quests.Cyborg.Advanced_Imaging.";
		//backing field for Stage property
		int stage = 0;
		static bool SawThing => Main.BestiaryDB.FindEntryByNPCID(NPCID.MartianProbe).UIInfoProvider.GetEntryUICollectionInfo().UnlockState != BestiaryEntryUnlockState.NotKnownAtAll_0;
		//Stage property so changing quest stage also updates its event handlers
		bool hasShownNotification = false;
		public override int Stage {
			get => stage;
			set {
				stage = value;
				// clear kill event handlers so they only run if the quest is active
				PreUpdateInventoryEvent = null;
				UpdateInventoryEvent = null;
				switch (stage) {
					case 1:
					PreUpdateInventoryEvent = () => {
						if (!hasShownNotification && SawThing) {
							HasNotification = true;
							hasShownNotification = true;
						}
					};
					break;
				}
			}
		}
		public override bool Started => Stage > 0;
		public override bool Completed => Stage > 1;
		public override bool CanStart(NPC npc) => npc.type == NPCID.Cyborg && !Started;
		public override string GetInquireText(NPC npc) => Language.GetTextValue(loc_key + "Inquire", Main.worldName);
		public override void OnAccept(NPC npc) {
			Stage = 1;
			Main.npcChatText = Language.GetTextValue(loc_key + "Start");
		}
		public override string ReadyToCompleteText(NPC npc) => Language.GetOrRegister(loc_key + "ReadyToComplete").Value;
		public override bool CanComplete(NPC npc) {
			if (npc.type != NPCID.Cyborg) return false;
			switch (Stage) {
				case 1:
				return SawThing;
			}
			return false;
		}
		public override void OnComplete(NPC npc) {
			Main.npcChatText = Language.GetTextValue(loc_key + "Complete");
			Stage = 2;
			ShouldSync = true;
		}
		public override string GetJournalPage() {
			return Language.GetTextValue(
				loc_key + "Journal", //translation key

				Main.worldName,
				StageTagOption(SawThing),
				StageTagOption(Completed)
			);
		}
		public override void SetStaticDefaults() {
			NameKey = loc_key + "Name";
		}
		public override void SaveData(TagCompound tag) {
			//save stage
			tag.Add("Stage", Stage);
		}
		public override void LoadData(TagCompound tag) {
			//load stage, note that it uses the Stage property so that it sets the event handlers
			//SafeGet returns the default value (0 for ints) if the tag doesn't have the data
			Stage = tag.SafeGet<int>("Stage");
		}
		public override void SendSync(BinaryWriter writer) {
			writer.Write(Stage);
		}
		public override void ReceiveSync(BinaryReader reader) {
			Stage = reader.ReadInt32();
		}
	}
}
