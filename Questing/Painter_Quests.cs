using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.IO;
using static Origins.OriginsSets.Items;

namespace Origins.Questing {
	public class Spray_N_Pray_Quest : Quest {
		public override bool SaveToWorld => true;
		//backing field for Stage property
		int stage = 0;

		int paintings = 0;
		const int paintingTarget = 10;
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
						paintings = 0;
					};
					UpdateInventoryEvent = (item) => {
						if (PaintingsNotFromVendor[item.type]) {
							paintings += item.stack;
						}
						if (!hasNotified && HasRequiredItems) {
							hasNotified = true;
							HasNotification = true;
						}
					};
					break;
					case 2:
					paintings = paintingTarget;
					break;
				}
			}
		}
		bool hasNotified = false;
		bool HasRequiredItems => paintings >= paintingTarget;
		public override bool Started => Stage > 0;
		public override bool Completed => Stage > 1;
		public override bool CanStart(NPC npc) {
			return npc.type == NPCID.Painter && Stage == 0 && Main.hardMode;
		}
		public override string GetInquireText(NPC npc) => Language.GetTextValue("Mods.Origins.Quests.Painter.Spray_N_Pray_Quest.Inquire");
		public override void OnAccept(NPC npc) {
			Stage = 1;
			Main.npcChatText = Language.GetTextValue("Mods.Origins.Quests.Painter.Spray_N_Pray_Quest.Start");
			ShouldSync = true;
		}
		public override bool CanComplete(NPC npc) => npc.type == NPCID.Painter && HasRequiredItems;
		public override string ReadyToCompleteText(NPC npc) => Language.GetOrRegister("Mods.Origins.Quests.Painter.Spray_N_Pray_Quest.ReadyToComplete").Value;
		public override void OnComplete(NPC npc) {
			Item[] inventory = Main.LocalPlayer.inventory;
			ConsumeItems(
				inventory,
				((i) => PaintingsNotFromVendor[i.type], paintingTarget)
			);
			Main.npcChatText = Language.GetTextValue("Mods.Origins.Quests.Painter.Spray_N_Pray_Quest.Complete", paintingTarget);
			Stage = 2;
			ShouldSync = true;
		}
		public override string GetJournalPage() {
			return Language.GetTextValue(
				"Mods.Origins.Quests.Painter.Spray_N_Pray_Quest.Journal", //translation key

				paintings,
				paintingTarget,
				StageTagOption(paintings >= paintingTarget),
				StageTagOption(Completed)
			);
		}
		public override void SetStaticDefaults() {
			NameKey = "Mods.Origins.Quests.Painter.Spray_N_Pray_Quest.Name";
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
