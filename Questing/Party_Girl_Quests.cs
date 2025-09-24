using Origins.Tiles;
using PegasusLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Events;
using Terraria.GameContent.Personalities;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Origins.Questing {
	[Autoload(false)]
	file class Happy_Grenade_Quest_Chat_Button(Quest quest) : QuestChatButton(quest) {
		public override string Name => $"{base.Name}_{Quest.Name}";
		public override double Priority => 101;
		Happy_Grenade_Quest HGQuest => Quest as Happy_Grenade_Quest;
		public override void OnClick(NPC npc, Player player) {
			Quest.ConsumeItems(player.inventory, ((i) => i.type == ItemID.Confetti, Happy_Grenade_Quest.confettiToGive));
			SoundEngine.PlaySound(SoundID.Grab, npc.Center);
			HGQuest.npcsGivenConfetti.Add(npc.type);
			string textKey = $"Mods.Origins.Quests.{npc.ModNPC?.Name ?? NPCID.Search.GetName(npc.type)}.GivenConfetti";
			if (!Language.Exists(textKey)) textKey = $"Mods.Origins.Quests.Common.GivenConfetti.{Main.rand.Next(3)}";
			Main.npcChatText = Language.GetOrRegister(textKey).Value;
		}
		public override string Text(NPC npc, Player player) => Language.GetOrRegister("Mods.Origins.Quests.PartyGirl.Happy_Grenade.RequestConfettiGiving").Format(Happy_Grenade_Quest.confettiToGive);
		public override bool IsActive(NPC npc, Player player) {
			if (!Questing.QuestListSelected) return false;
			return Quest.HasQuestButton(npc, player);
		}
	}
	public class Happy_Grenade_Quest : Quest {
		public const string loc_key = "Mods.Origins.Quests.PartyGirl.Happy_Grenade.";
		public QuestChatButton Button { get; protected set; }
		public override void Load() {
			Mod.AddContent(Button = new Happy_Grenade_Quest_Chat_Button(this));
		}
		//backing field for Stage property
		int stage = 0;

		public HashSet<int> npcsGivenConfetti = [];
		List<string> unloadedConfettiNPCs = [];
		public const int confettiToGive = 10;
		public const int npcsToGiveConfetti = 10;
		//Stage property so changing quest stage also updates its event handlers
		public override int Stage {
			get => stage;
			set {
				stage = value;
			}
		}
		public bool HasGivenAllConfetti => (npcsGivenConfetti ?? []).Count + (unloadedConfettiNPCs ?? []).Count >= npcsToGiveConfetti;
		public override bool Started => Stage > 0;
		public override bool Completed => Stage > 1;
		public override bool HasQuestButton(NPC npc, Player player) {
			int confettiAmount = 0;
			foreach (Item item in player.inventory) {
				if (item.active && item.type == ItemID.Confetti) confettiAmount += item.stack;
			}
			if (npc.type == NPCID.PartyGirl || NPCID.Sets.IsTownPet[npc.type] || NPCID.Sets.IsTownSlime[npc.type] || OriginsSets.NPCs.TargetDummies[npc.type]) return false;
			return !npcsGivenConfetti.Contains(npc.type) && Stage == 1 && confettiAmount >= confettiToGive && !HasGivenAllConfetti;
		}
		public override bool CanStart(NPC npc) {
			return npc.type == NPCID.PartyGirl && Stage == 0;
		}
		public override string GetInquireText(NPC npc) => Language.GetTextValue(loc_key + "Inquire");
		public override void OnAccept(NPC npc) {
			Stage = 1;
			Main.npcChatText = Language.GetTextValue(loc_key + "Start");
			ShouldSync = true;
		}
		public override bool CanComplete(NPC npc) => npc.type == NPCID.PartyGirl && HasGivenAllConfetti;
		public override string ReadyToCompleteText(NPC npc) => Language.GetOrRegister(loc_key + "ReadyToComplete").Value;
		public override void OnComplete(NPC npc) {
			Main.npcChatText = Language.GetTextValue(loc_key + "Complete");
			Stage = 2;
			ShouldSync = true;
		}
		public override string GetJournalPage() {
			return Language.GetTextValue(
				loc_key + "Journal", //translation key

				Completed ? npcsToGiveConfetti : npcsGivenConfetti.Count,
				npcsToGiveConfetti,
				confettiToGive,
				StageTagOption(HasGivenAllConfetti),
				StageTagOption(Completed)
			);
		}
		public override void SetStaticDefaults() {
			NameKey = loc_key + "Name";
		}
		public override void SaveData(TagCompound tag) {
			//save stage and kills
			tag.Add("Stage", Stage);
			tag.Add("NPCsGivenConfetti", npcsGivenConfetti.Select(NPCID.Search.GetName).Concat(unloadedConfettiNPCs ?? []).ToList() ?? []);
		}
		public override void LoadData(TagCompound tag) {
			//load stage and kills, note that it uses the Stage property so that it sets the event handlers
			//SafeGet returns the default value (0 for ints) if the tag doesn't have the data
			Stage = tag.SafeGet<int>("Stage");
			npcsGivenConfetti = [];
			unloadedConfettiNPCs = [];
			List<string> givenConfetti = tag.SafeGet<List<string>>("NPCsGivenConfetti") ?? [];
			foreach (string value in givenConfetti) {
				if (NPCID.Search.TryGetId(value, out int id)) npcsGivenConfetti.Add(id);
				else unloadedConfettiNPCs.Add(value);
			}
		}
	}
	[Autoload(false)]
	file class Tax_Collector_Hat_Chat_Button(Quest quest) : QuestChatButton(quest) {
		public override string Name => $"{base.Name}_{Quest.Name}";
		public override double Priority => 101;
		public override void OnClick(NPC npc, Player player) {
			Questing.QuestListSelected = true;
			Questing.fromQuest = Quest;
			Main.npcChatText = Language.GetTextValue(Tax_Collector_Ghosts_Quest.loc_key + "Request");
		}
		public override string Text(NPC npc, Player player) => Language.GetOrRegister("Mods.Origins.Quests.PartyGirl.Tax_Collector_Hat.RequestHatWearing").Value;
		public override bool IsActive(NPC npc, Player player) {
			if (Questing.QuestListSelected) return false;
			return npc.type == NPCID.TaxCollector && Quest.Stage == 1 && !ModContent.GetInstance<Tax_Collector_Ghosts_Quest>().ShowInJournal();
		}
	}
	public class Tax_Collector_Quests : Quest {
		public const string loc_key = "Mods.Origins.Quests.PartyGirl.Tax_Collector_Hat.";
		public override void Load() {
			On_NPC.UsesPartyHat += (orig, self) => {
				if (self.type == NPCID.TaxCollector) {
					if (self.ForcePartyHatOn) {
						return true;
					}
					if (self.IsABestiaryIconDummy) {
						return false;
					}
					return BirthdayParty.PartyIsUp && (ModContent.GetInstance<Tax_Collector_Ghosts_Quest>()?.Completed ?? false);
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
			}
		}
		public override bool Started => Stage > 0;
		public override bool Completed => Stage > 1;
		public override bool CanStart(NPC npc) {
			if ((Started && LocalPlayerStarted) || Completed) return false;
			return npc.type == NPCID.PartyGirl
				&& NPC.downedPlantBoss
				&& ModContent.GetInstance<Happy_Grenade_Quest>().Completed
				&& NPC.AnyNPCs(NPCID.TaxCollector);
		}
		public override string GetInquireText(NPC npc) => Language.GetTextValue(loc_key + "Inquire", Main.LocalPlayer.Get2ndPersonReference("casual"));
		public override void OnAccept(NPC npc) {
			Stage = 1;// - set stage to 1 (kill harpies)
			LocalPlayerStarted = true;
			Main.npcChatText = Language.GetTextValue(loc_key + "Start");
		}
		public override bool CanComplete(NPC npc) => npc.type == NPCID.PartyGirl && (ModContent.GetInstance<Tax_Collector_Ghosts_Quest>()?.Completed ?? false);
		public override string ReadyToCompleteText(NPC npc) => Language.GetOrRegister(loc_key + "ReadyToComplete").Value;
		public override void OnComplete(NPC npc) {
			Main.npcChatText = Language.GetTextValue(loc_key + "Complete");
			Stage = 2;
		}
		public override string GetJournalPage() {
			return Language.GetTextValue(
				loc_key + "Journal", //translation key
				StageTagOption(ModContent.GetInstance<Tax_Collector_Ghosts_Quest>()?.Completed ?? false),
				StageTagOption(Completed)
			);
		}
		public override void SetStaticDefaults() {
			NameKey = loc_key + "Name";
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
}