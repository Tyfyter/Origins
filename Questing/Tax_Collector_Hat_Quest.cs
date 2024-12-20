using PegasusLib;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Events;
using Terraria.GameContent.Personalities;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Origins.Questing {
	public class Tax_Collector_Hat_Quest : Quest {
		public override void Load() {
			On_NPC.UsesPartyHat += (orig, self) => {
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
			Mod.AddContent(new Tax_Collector_Hat_Chat_Button(this));
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
					if (OriginSystem.Instance?.taxCollectorWearsPartyhat ?? false) { 
						Stage = 2;
					}
					break;
				}
			}
		}
		public override bool Started => Stage > 0;
		public override bool Completed => Stage > 2;
		public override bool CanStart(NPC npc) {
			return false;
			if (LocalPlayerStarted || Completed) return false;
			return npc.type == NPCID.PartyGirl &&
				true && // replace with condition for prerequisite quests
				NPC.AnyNPCs(NPCID.TaxCollector);
		}
		public override string GetInquireText(NPC npc) => Language.GetTextValue("Mods.Origins.Quests.Party_Girl.Tax_Collector_Hat.Inquire", Main.LocalPlayer.Get2ndPersonReference("casual"));
		public override void OnAccept(NPC npc) {
			Stage = 1;// - set stage to 1 (kill harpies)
			LocalPlayerStarted = true;
			Main.npcChatText = Language.GetTextValue("Mods.Origins.Quests.Party_Girl.Tax_Collector_Hat.Start");
		}
		public override bool CanComplete(NPC npc) => npc.type == NPCID.PartyGirl && Stage == 2;
		public override string ReadyToCompleteText(NPC npc) => Language.GetOrRegister("Mods.Origins.Quests.Party_Girl.Tax_Collector_Hat.ReadyToComplete").Value;
		public override void OnComplete(NPC npc) {
			Main.npcChatText = Language.GetTextValue("Mods.Origins.Quests.Party_Girl.Tax_Collector_Hat.Complete");
			Stage = 3;
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
			FastFieldInfo<ShopHelper, PersonalityDatabase> _database = new("_database", BindingFlags.Public | BindingFlags.NonPublic);
			List<IShopPersonalityTrait> shopModifiers = _database.GetValue(Main.ShopHelper).GetOrCreateProfileByNPCID(NPCID.PartyGirl).ShopModifiers;
			for (int i = 0; i < shopModifiers.Count; i++) {
				if (shopModifiers[i] is NPCPreferenceTrait nPCPreferenceTrait && nPCPreferenceTrait.NpcId == NPCID.TaxCollector) {
					shopModifiers[i] = new ConditionalNPCPreferenceTrait() {
						NpcId = nPCPreferenceTrait.NpcId,
						Level = nPCPreferenceTrait.Level,
						condition = CompletedCondition.Not()
					};
					break;
				}
			}
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
	[Autoload(false)]
	file class Tax_Collector_Hat_Chat_Button(Quest quest) : QuestChatButton(quest) {
		public override string Name => $"{base.Name}_{Quest.Name}";
		public override double Priority => 101;
		public override void OnClick(NPC npc, Player player) {
			Quest.Stage++;
		}
		public override string Text(NPC npc, Player player) => "missingno";
		public override bool IsActive(NPC npc, Player player) {
			if (Questing.questListSelected) return false;
			return npc.type == NPCID.TaxCollector && Quest.Stage == 1;
		}
	}
}