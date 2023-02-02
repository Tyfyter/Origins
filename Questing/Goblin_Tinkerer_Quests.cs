using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Origins.Questing {
	public class Turbo_Reel_Quest : Quest {
		//backing field for Stage property
		int stage = 0;

		int ironBars = 0;
		const int ironBarTarget = 5;

		int chains = 0;
		const int chainTarget = 2;

		int adhesiveWraps = 0;
		const int adhesiveWrapTarget = 15;

		bool hasWatch = false;
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
						ironBars = 0;
						chains = 0;
						adhesiveWraps = 0;
						hasWatch = false;
					};
					UpdateInventoryEvent = (item) => {
						if (item.type == ItemID.GoldWatch || item.type == ItemID.PlatinumWatch) {
							hasWatch = true;
						} else if (item.type == ModContent.ItemType<Adhesive_Wrap>()) {
							adhesiveWraps += item.stack;
						} else if (item.type == ItemID.Chain) {
							chains += item.stack;
						} else if (RecipeGroup.recipeGroups[RecipeGroupID.IronBar].ContainsItem(item.type)) {
							ironBars += item.stack;
						}
					};
					break;
					case 2:
					ironBars = ironBarTarget;
					chains = chainTarget;
					adhesiveWraps = adhesiveWrapTarget;
					hasWatch = true;
					break;
				}
			}
		}
		public override bool Started => Stage > 0;
		public override bool Completed => Stage > 1;
		public override bool HasStartDialogue(NPC npc) {
			return npc.type == NPCID.GoblinTinkerer && Stage == 0;
		}
		public override bool HasDialogue(NPC npc) {
			if (npc.type != NPCID.GoblinTinkerer) return false; // NPCs other than the merchant won't have any dialogue related to this quest
			switch (Stage) {
				case 1:
				return
					ironBars >= ironBarTarget &&
					chains >= chainTarget &&
					adhesiveWraps >= adhesiveWrapTarget &&
					hasWatch;
			}
			return false;
		}
		public override string GetDialogue() {
			switch (Stage) {
				case 1:
				return "Complete Quest";

				default:
				if (Origins.npcChatQuestSelected) {
					return "Accept";
				}
				return Language.GetTextValue(NameKey);
			}
		}
		public override void OnDialogue() {
			switch (stage) {
				case 0: {
					if (Origins.npcChatQuestSelected) {
						Stage = 1;
					} else {
						Main.npcChatText = Language.GetTextValue("Mods.Origins.Quests.Goblin_Tinkerer.Turbo_Reel.Start");
						Origins.npcChatQuestSelected = true;// (npcChatQuestSelected is reset to false when the player closes the dialogue box)
					}
					break;
				}
				case 1: {
					Item[] inventory = Main.LocalPlayer.inventory;
					RecipeGroup ironBarGroup = RecipeGroup.recipeGroups[RecipeGroupID.IronBar];
					ConsumeItems(
						inventory,
						((i) => i.type == ItemID.GoldWatch || i.type == ItemID.PlatinumWatch, 1),
						((i) => i.type == ModContent.ItemType<Adhesive_Wrap>(), adhesiveWrapTarget),
						((i) => i.type == ItemID.Chain, chainTarget),
						((i) => ironBarGroup.ContainsItem(i.type), ironBarTarget)
					);
					Main.npcChatText = Language.GetTextValue("Mods.Origins.Quests.Goblin_Tinkerer.Turbo_Reel.Complete");
					Stage = 2;
					break;
				}
			}
		}
		public override string GetJournalPage() {
			return Language.GetTextValue(
				"Mods.Origins.Quests.Goblin_Tinkerer.Turbo_Reel.Journal", //translation key

				ironBars,
				ironBarTarget,
				StageTagOption(ironBars >= ironBarTarget),

				chains,
				chainTarget,
				StageTagOption(chains >= chainTarget),

				adhesiveWraps,
				adhesiveWrapTarget,
				StageTagOption(adhesiveWraps >= adhesiveWrapTarget),

				StageTagOption(hasWatch)
			);
		}
		public override void SetStaticDefaults() {
			NameKey = "Mods.Origins.Quests.Goblin_Tinkerer.Turbo_Reel.Name";
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
	public class Gun_Glove_Quest : Quest {
		//backing field for Stage property
		int stage = 0;

		int leather = 0;
		const int leatherTarget = 2;

		bool hasPistol = false;

		bool hasArm = false;
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
						leather = 0;
						hasPistol = false;
						hasArm = false;
					};
					UpdateInventoryEvent = (item) => {
						switch (item.type) {
							case ItemID.Leather:
							leather += item.stack;
							break;

							case ItemID.FlintlockPistol:
							hasPistol = true;
							break;

							case ItemID.ZombieArm:
							hasArm = true;
							break;
						}
					};
					break;
					case 2:
					leather = leatherTarget;
					hasPistol = true;
					hasArm = true;
					break;
				}
			}
		}
		public override bool Started => Stage > 0;
		public override bool Completed => Stage > 1;
		public override bool HasStartDialogue(NPC npc) {
			return npc.type == NPCID.GoblinTinkerer && Stage == 0 && ModContent.GetInstance<Turbo_Reel_Quest>().Completed;
		}
		public override bool HasDialogue(NPC npc) {
			if (npc.type != NPCID.GoblinTinkerer) return false; // NPCs other than the merchant won't have any dialogue related to this quest
			switch (Stage) {
				case 1:
				return leather >= leatherTarget && hasPistol && hasArm;
			}
			return false;
		}
		public override string GetDialogue() {
			switch (Stage) {
				case 1:
				return "Complete Quest";

				default:
				if (Origins.npcChatQuestSelected) {
					return "Accept";
				}
				return Language.GetTextValue(NameKey);
			}
		}
		public override void OnDialogue() {
			switch (stage) {
				case 0: {
					if (Origins.npcChatQuestSelected) {
						Stage = 1;
					} else {
						Main.npcChatText = Language.GetTextValue(
							"Mods.Origins.Quests.Goblin_Tinkerer.Gun_Glove.Start",
							NPC.GetFirstNPCNameOrNull(NPCID.ArmsDealer) ?? Language.GetTextValue("Mods.Origins.Generic.Arms_Dealer")
						);
						Origins.npcChatQuestSelected = true;// (npcChatQuestSelected is reset to false when the player closes the dialogue box)
					}
					break;
				}
				case 1: {
					Item[] inventory = Main.LocalPlayer.inventory;
					RecipeGroup ironBarGroup = RecipeGroup.recipeGroups[RecipeGroupID.IronBar];
					ConsumeItems(
						inventory,
						((i) => i.type == ItemID.Leather, leatherTarget),
						((i) => i.type == ItemID.FlintlockPistol, 1),
						((i) => i.type == ItemID.ZombieArm, 1)
					);
					Main.npcChatText = Language.GetTextValue("Mods.Origins.Quests.Goblin_Tinkerer.Gun_Glove.Complete");
					Stage = 2;
					break;
				}
			}
		}
		public override string GetJournalPage() {
			return Language.GetTextValue(
				"Mods.Origins.Quests.Goblin_Tinkerer.Gun_Glove.Journal", //translation key
				NPC.GetFirstNPCNameOrNull(NPCID.ArmsDealer) ?? Language.GetTextValue("Mods.Origins.Generic.Arms_Dealer"),

				leather,
				leatherTarget,
				StageTagOption(leather >= leatherTarget),

				StageTagOption(hasPistol),

				StageTagOption(hasArm)
			);
		}
		public override void SetStaticDefaults() {
			NameKey = "Mods.Origins.Quests.Goblin_Tinkerer.Gun_Glove.Name";
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
	public class Rocket_Boosted_Minecart_Quest : Quest {
		//backing field for Stage property
		int stage = 0;

		bool hasMinecart = false;

		int rockets = 0;
		const int rocketTarget = 50;

		int adhesiveWraps = 0;
		const int adhesiveWrapTarget = 80;

		int adamantiteBars = 0;
		const int adamantiteBarTarget = 15;
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
						rockets = 0;
						adamantiteBars = 0;
						adhesiveWraps = 0;
						hasMinecart = false;
					};
					UpdateInventoryEvent = (item) => {
						if (item.type == ItemID.Minecart) {
							hasMinecart = true;
						} else if (item.type == ItemID.RocketI) {
							rockets += item.stack;
						} else if (item.type == ModContent.ItemType<Adhesive_Wrap>()) {
							adhesiveWraps += item.stack;
						} else if (item.type == ItemID.AdamantiteBar || item.type == ItemID.TitaniumBar) {
							adamantiteBars += item.stack;
						}
					};
					break;
					case 2:
					rockets = rocketTarget;
					adamantiteBars = adamantiteBarTarget;
					adhesiveWraps = adhesiveWrapTarget;
					hasMinecart = true;
					break;
				}
			}
		}
		public override bool Started => Stage > 0;
		public override bool Completed => Stage > 1;
		public override bool HasStartDialogue(NPC npc) {
			return npc.type == NPCID.GoblinTinkerer && Stage == 0 && Main.hardMode && ModContent.GetInstance<Gun_Glove_Quest>().Completed;
		}
		public override bool HasDialogue(NPC npc) {
			if (npc.type != NPCID.GoblinTinkerer) return false; // NPCs other than the merchant won't have any dialogue related to this quest
			switch (Stage) {
				case 1:
				return
					rockets >= rocketTarget &&
					adamantiteBars >= adamantiteBarTarget &&
					adhesiveWraps >= adhesiveWrapTarget &&
					hasMinecart;
			}
			return false;
		}
		public override string GetDialogue() {
			switch (Stage) {
				case 1:
				return "Complete Quest";

				default:
				if (Origins.npcChatQuestSelected) {
					return "Accept";
				}
				return Language.GetTextValue(NameKey);
			}
		}
		public override void OnDialogue() {
			switch (stage) {
				case 0: {
					if (Origins.npcChatQuestSelected) {
						Stage = 1;
					} else {
						Main.npcChatText = Language.GetTextValue(
							"Mods.Origins.Quests.Goblin_Tinkerer.Rocket_Boosted_Minecart.Start",
							NPC.GetFirstNPCNameOrNull(NPCID.Mechanic) ?? Language.GetTextValue("Mods.Origins.Quests.Goblin_Tinkerer.Generic.Mechanic")
						);
						Origins.npcChatQuestSelected = true;// (npcChatQuestSelected is reset to false when the player closes the dialogue box)
					}
					break;
				}
				case 1: {
					Item[] inventory = Main.LocalPlayer.inventory;
					ConsumeItems(
						inventory,
						((i) => i.type == ItemID.Minecart, 1),
						((i) => i.type == ItemID.RocketI, rocketTarget),
						((i) => i.type == ModContent.ItemType<Adhesive_Wrap>(), adhesiveWrapTarget),
						((i) => i.type == ItemID.AdamantiteBar || i.type == ItemID.TitaniumBar, adamantiteBarTarget)
					);
					Main.npcChatText = Language.GetTextValue("Mods.Origins.Quests.Goblin_Tinkerer.Rocket_Boosted_Minecart.Complete");
					Stage = 2;
					break;
				}
			}
		}
		public override string GetJournalPage() {
			return Language.GetTextValue(
				"Mods.Origins.Quests.Goblin_Tinkerer.Rocket_Boosted_Minecart.Journal", //translation key
				NPC.GetFirstNPCNameOrNull(NPCID.Mechanic) ?? Language.GetTextValue("Mods.Origins.Quests.Goblin_Tinkerer.Generic.Mechanic"),

				StageTagOption(hasMinecart),

				rockets,
				rocketTarget,
				StageTagOption(rockets >= rocketTarget),

				adhesiveWraps,
				adhesiveWrapTarget,
				StageTagOption(adhesiveWraps >= adhesiveWrapTarget),

				adamantiteBars,
				adamantiteBarTarget,
				StageTagOption(adamantiteBars >= adamantiteBarTarget)
			);
		}
		public override void SetStaticDefaults() {
			NameKey = "Mods.Origins.Quests.Goblin_Tinkerer.Rocket_Boosted_Minecart.Name";
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
