using Origins.Items.Other.Consumables;
using Origins.NPCs.TownNPCs;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Origins.Questing {
	public class Alkaliegis_Quest : Quest {
		public const string loc_key = "Mods.Origins.Quests.Brine_Fiend.Alkaliegis.";
		//backing field for Stage property
		int stage = 0;

		bool hasSummonPotion = false;
		bool hasTeleportPotion = false;
		bool hasTitanPotion = false;
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
						hasSummonPotion = false;
						hasTeleportPotion = false;
						hasTitanPotion = false;
					};
					UpdateInventoryEvent = (item) => {
						if (item?.ModItem is Greater_Summoning_Potion) {
							hasSummonPotion = true;
						} else if (item.type == ItemID.TeleportationPotion) {
							hasTeleportPotion = true;
						} else if (item.type == ItemID.TitanPotion) {
							hasTitanPotion = true;
						}
						if (!hasNotified && HasRequiredItems) {
							hasNotified = true;
							HasNotification = true;
						}
					};
					break;
					case 2:
					hasSummonPotion = true;
					hasTeleportPotion = true;
					hasTitanPotion = true;
					break;
				}
			}
		}
		bool hasNotified = false;
		bool HasRequiredItems => hasSummonPotion && hasTeleportPotion && hasTitanPotion;
		public override bool Started => Stage > 0;
		public override bool Completed => Stage > 1;
		public override bool CanStart(NPC npc) {
			return npc.type == ModContent.NPCType<Brine_Fiend>() && Stage == 0;
		}
		public override string GetInquireText(NPC npc) => Language.GetTextValue(loc_key + "Inquire");
		public override void OnAccept(NPC npc) {
			Stage = 1;
			Main.npcChatText = Language.GetTextValue(loc_key + "Start");
			ShouldSync = true;
		}
		public override bool CanComplete(NPC npc) => npc.type == ModContent.NPCType<Brine_Fiend>() && HasRequiredItems;
		public override string ReadyToCompleteText(NPC npc) => Language.GetOrRegister(loc_key + "ReadyToComplete").Value;
		public override void OnComplete(NPC npc) {
			Item[] inventory = Main.LocalPlayer.inventory;
			ConsumeItems(
				inventory,
				((i) => i?.ModItem is Greater_Summoning_Potion, 1),
				((i) => i.type == ItemID.TeleportationPotion, 1),
				((i) => i.type == ItemID.TitanPotion, 1)
			);
			Main.npcChatText = Language.GetTextValue(loc_key + "Complete");
			Stage = 2;
			ShouldSync = true;
		}
		public override string GetJournalPage() {
			return Language.GetTextValue(
				loc_key + "Journal", //translation key

				StageTagOption(hasSummonPotion),
				StageTagOption(hasTeleportPotion),
				StageTagOption(hasTitanPotion),
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
		public override void SendSync(BinaryWriter writer) {
			writer.Write(Stage);
		}
		public override void ReceiveSync(BinaryReader reader) {
			Stage = reader.ReadInt32();
		}
	}
	public class Old_Brine_Music_Box_Quest : Quest {
		public const string loc_key = "Mods.Origins.Quests.Brine_Fiend.Ancient_Brine_Music_Box.";
		//backing field for Stage property
		int stage = 0;

		bool hasFervorPotion = false;
		bool hasLovePotion = false;
		bool hasMagicPotion = false;
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
						hasFervorPotion = false;
						hasLovePotion = false;
						hasMagicPotion = false;
					};
					UpdateInventoryEvent = (item) => {
						if (item.type == ModContent.ItemType<Fervor_Potion>()) {
							hasFervorPotion = true;
						} else if (item.type == ItemID.LovePotion) {
							hasLovePotion = true;
						} else if (item.type == ItemID.MagicPowerPotion) {
							hasMagicPotion = true;
						}
						if (!hasNotified && HasRequiredItems) {
							hasNotified = true;
							HasNotification = true;
						}
					};
					break;
					case 2:
					hasFervorPotion = true;
					hasLovePotion = true;
					hasMagicPotion = true;
					break;
				}
			}
		}
		bool hasNotified = false;
		bool HasRequiredItems => hasFervorPotion && hasLovePotion && hasMagicPotion;
		public override bool Started => Stage > 0;
		public override bool Completed => Stage > 1;
		public override bool CanStart(NPC npc) {
			return npc.type == ModContent.NPCType<Brine_Fiend>() && Stage == 0 && ModContent.GetInstance<Alkaliegis_Quest>().Completed;
		}
		public override string GetInquireText(NPC npc) => Language.GetTextValue(loc_key + "Inquire");
		public override void OnAccept(NPC npc) {
			Stage = 1;
			Main.npcChatText = Language.GetTextValue(loc_key + "Start");
			ShouldSync = true;
		}
		public override bool CanComplete(NPC npc) => npc.type == ModContent.NPCType<Brine_Fiend>() && HasRequiredItems;
		public override string ReadyToCompleteText(NPC npc) => Language.GetOrRegister(loc_key + "ReadyToComplete").Value;
		public override void OnComplete(NPC npc) {
			Item[] inventory = Main.LocalPlayer.inventory;
			ConsumeItems(
				inventory,
				((i) => i.type == ModContent.ItemType<Fervor_Potion>(), 1),
				((i) => i.type == ItemID.LovePotion, 1),
				((i) => i.type == ItemID.MagicPowerPotion, 1)
			);
			Main.npcChatText = Language.GetTextValue(loc_key + "Complete");
			Stage = 2;
			ShouldSync = true;
		}
		public override string GetJournalPage() {
			return Language.GetTextValue(
				loc_key + "Journal", //translation key

				StageTagOption(hasFervorPotion),
				StageTagOption(hasLovePotion),
				StageTagOption(hasMagicPotion),
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
		public override void SendSync(BinaryWriter writer) {
			writer.Write(Stage);
		}
		public override void ReceiveSync(BinaryReader reader) {
			Stage = reader.ReadInt32();
		}
	}
}
