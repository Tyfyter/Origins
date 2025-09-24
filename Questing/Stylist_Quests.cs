using Origins.Items.Accessories;
using Origins.Items.Materials;
using Origins.Items.Weapons.Melee;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Origins.Questing {
	public class Magic_Hair_Spray_Quest : Quest {
		public const string loc_key = "Mods.Origins.Quests.Stylist.Magic_Hair_Spray.";
		//backing field for Stage property
		int stage = 0;

		int worms = 0;
		const int wormTarget = 30;
		//Stage property so changing quest stage also updates its event handlers
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
						worms = 0;
					};
					UpdateInventoryEvent = (item) => {
						if (item.type == ModContent.ItemType<Magic_Hair_Spray>()) {
							worms += item.stack;
						}
					};
					break;
					case 2:
					worms = wormTarget;
					break;
				}
			}
		}
		public override bool Started => Stage > 0;
		public override bool Completed => Stage > 1;
		public override bool CanStart(NPC npc) => npc.type == NPCID.Stylist && Stage == 0;
		public override string GetInquireText(NPC npc) => Language.GetTextValue(loc_key + "Inquire");
		public override void OnAccept(NPC npc) {
			Stage = 1;
			Main.npcChatText = Language.GetTextValue(loc_key + "Start");
		}
		public override string ReadyToCompleteText(NPC npc) => Language.GetOrRegister(loc_key + "ReadyToComplete").Value;
		public override bool CanComplete(NPC npc) {
			if (npc.type != NPCID.Stylist) return false; // NPCs other than the merchant won't have any dialogue related to this quest
			switch (Stage) {
				case 1:
				return worms >= wormTarget;
			}
			return false;
		}
		public override void OnComplete(NPC npc) {
			ConsumeItems(Main.LocalPlayer.inventory, ((i) => i.type == ModContent.ItemType<Magic_Hair_Spray>(), wormTarget));
			Main.npcChatText = Language.GetTextValue(loc_key + "Complete");
			Stage = 2;
			ShouldSync = true;
		}
		public override string GetJournalPage() {
			return Language.GetTextValue(
				loc_key + "Journal", //translation key

				worms,
				wormTarget,
				StageTagOption(worms >= wormTarget),
				StageTagOption(Completed)
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
	}
	public class Comb_Quest : Quest {
		public const string loc_key = "Mods.Origins.Quests.Stylist.Comb.";
		//backing field for Stage property
		int stage = 0;

		bool hasComb = false;
		//Stage property so changing quest stage also updates its event handlers
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
						hasComb = false;
					};
					UpdateInventoryEvent = (item) => {
						if (item.type == ModContent.ItemType<Comb>()) {
							hasComb = true;
						}
					};
					break;
					case 2:
					hasComb = true;
					break;
				}
			}
		}
		public override bool Started => Stage > 0;
		public override bool Completed => Stage > 1;
		public override bool CanStart(NPC npc) => npc.type == NPCID.Stylist && !Started && ModContent.GetInstance<Magic_Hair_Spray_Quest>().Completed;
		public override string GetInquireText(NPC npc) => Language.GetTextValue(loc_key + "Inquire", Main.LocalPlayer.name);
		public override void OnAccept(NPC npc) {
			Stage = 1;
			Main.npcChatText = Language.GetTextValue(loc_key + "Start");
		}
		public override string ReadyToCompleteText(NPC npc) => Language.GetOrRegister(loc_key + "ReadyToComplete").Value;
		public override bool CanComplete(NPC npc) {
			if (npc.type != NPCID.Stylist) return false; // NPCs other than the merchant won't have any dialogue related to this quest
			switch (Stage) {
				case 1:
				return hasComb;
			}
			return false;
		}
		public override void OnComplete(NPC npc) {
			ConsumeItems(Main.LocalPlayer.inventory, ((i) => i.type == ModContent.ItemType<Comb>(), 1));
			Main.npcChatText = Language.GetTextValue(loc_key + "Complete");
			Stage = 2;
			ShouldSync = true;
		}
		public override string GetJournalPage() {
			return Language.GetTextValue(
				loc_key + "Journal", //translation key

				StageTagOption(hasComb),
				StageTagOption(Completed)
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
	}
	public class Holiday_Hair_Dye_Quest : Quest {
		public const string loc_key = "Mods.Origins.Quests.Stylist.Holiday_Hair_Dye.";
		//backing field for Stage property
		int stage = 0;

		bool hasComb = false;
		//Stage property so changing quest stage also updates its event handlers
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
						hasComb = false;
					};
					UpdateInventoryEvent = (item) => {
						if (item.type == ModContent.ItemType<Crystal_Cutters>()) {
							hasComb = true;
						}
					};
					break;
					case 2:
					hasComb = true;
					break;
				}
			}
		}
		public override bool Started => Stage > 0;
		public override bool Completed => Stage > 1;
		public override bool CanStart(NPC npc) => npc.type == NPCID.Stylist && Main.hardMode && !Started && NPC.savedWizard && ModContent.GetInstance<Comb_Quest>().Completed;
		public override string GetInquireText(NPC npc) => Language.GetTextValue(loc_key + "Inquire", Main.LocalPlayer.name);
		public override void OnAccept(NPC npc) {
			Stage = 1;
			Main.npcChatText = Language.GetTextValue(loc_key + "Start");
		}
		public override string ReadyToCompleteText(NPC npc) => Language.GetOrRegister(loc_key + "ReadyToComplete").Value;
		public override bool CanComplete(NPC npc) {
			if (npc.type != NPCID.Stylist || !Main.hardMode) return false; // NPCs other than the merchant won't have any dialogue related to this quest
			switch (Stage) {
				case 1:
				return hasComb;
			}
			return false;
		}
		public override void OnComplete(NPC npc) {
			ConsumeItems(Main.LocalPlayer.inventory, ((i) => i.type == ModContent.ItemType<Crystal_Cutters>(), 1));
			Main.npcChatText = Language.GetTextValue(loc_key + "Complete");
			Stage = 2;
			ShouldSync = true;
		}
		public override string GetJournalPage() {
			return Language.GetTextValue(
				loc_key + "Journal", //translation key

				StageTagOption(hasComb),
				StageTagOption(Completed)
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
	}
}
