using Origins.Items;
using Origins.Items.Weapons.Ranged;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Origins.Questing {
	public class Shardcannon_Quest : Quest {
		public const string loc_key = "Mods.Origins.Quests.ArmsDealer.Shardcannon.";
		public QuestChatButton Button { get; protected set; }
		//backing field for Stage property
		int stage = 0;
		int progress = 0;
		const int target = 50;
		public void UpdateKillCount() {
			if (stage != 1) return;
			if (++progress >= target) {
				HasNotification = true;
				Stage = 2;
			}
			ShouldSync = true;
		}
		//Stage property so changing quest stage also updates its event handlers
		public override int Stage {
			get => stage;
			set => stage = value;
		}
		public override bool Started => Stage > 0;
		public override bool Completed => Stage > 2;
		public override void Load() {
			Mod.AddContent(Button = new Lost_Shardcannon_Chat_Button(this));
		}
		public override bool HasQuestButton(NPC npc, Player player) {
			static bool IsShardcannon(Item item) {
				return item.type == Shardcannon.ID && item.prefix == Imperfect_Prefix.ID && item.stack > 0;
			}
			static bool HasShardcannon(Item[] inventory) {
				for (int i = 0; i < inventory.Length; i++) {
					Item item = inventory[i];
					if (IsShardcannon(item)) {
						return true;
					}
				}
				return false;
			}
			if (HasShardcannon(player.inventory)) {
				return false;
			}
			if (IsShardcannon(player.inventory[Main.InventorySlotsTotal]) || IsShardcannon(player.trashItem)) return false;
			if (HasShardcannon(player.bank.item)) {
				return false;
			}
			if (HasShardcannon(player.bank2.item)) {
				return false;
			}
			if (HasShardcannon(player.bank3.item)) {
				return false;
			}
			if (HasShardcannon(player.bank4.item)) {
				return false;
			}
			return Stage == 1;
		}
		public override bool CanStart(NPC npc) {
			return npc.type == NPCID.ArmsDealer && !ShowInJournal();
		}
		public override void OnAccept(NPC npc) {
			if (Stage < 1) Stage = 1;// - set stage to 1 (kill harpies)
			LocalPlayerStarted = true;
			GiveGun();
			Main.npcChatText = Language.GetTextValue(loc_key + "Start");
		}
		public override string GetInquireText(NPC npc) => Language.GetTextValue(loc_key + "Inquire");
		public override bool CanComplete(NPC npc) => npc.type == NPCID.ArmsDealer && Stage == 2;
		public override string ReadyToCompleteText(NPC npc) => Language.GetOrRegister(loc_key + "ReadyToComplete").Value;
		public override void OnComplete(NPC npc) {
			for (int i = 0; i < Main.InventoryItemSlotsCount; i++) {
				Item item = Main.LocalPlayer.inventory[i];
				if (item.type == ModContent.ItemType<Shardcannon>() && item.prefix == ModContent.PrefixType<Imperfect_Prefix>()) {
					item.TurnToAir();
					Main.npcChatText = Language.GetTextValue(loc_key + "Complete");
					Stage = 3;
					ShouldSync = true;
					return;
				}
			}
			Main.npcChatText = Language.GetTextValue(loc_key + "WhereGun");
			Stage = 3;
			ShouldSync = true;
		}
		public static void GiveGun() {
			int index = Item.NewItem(
				Main.LocalPlayer.GetSource_GiftOrReward(),
				Main.LocalPlayer.position,
				Main.LocalPlayer.Size,
				ModContent.ItemType<Shardcannon>(),
				1,
				prefixGiven: ModContent.PrefixType<Imperfect_Prefix>()
			);
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				NetMessage.SendData(MessageID.SyncItem, -1, -1, null, index, 1f);
			}
		}
		public override bool ShowInJournal() => Completed || (base.ShowInJournal() && LocalPlayerStarted);
		public override string GetJournalPage() {
			return Language.GetTextValue(
				loc_key + "Journal", //translation key
				progress,
				target,
				StageTagOption(progress >= target) //used in a quest stage tag to show the stage as completed
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
	}
	[Autoload(false)]
	file class Lost_Shardcannon_Chat_Button(Quest quest) : QuestChatButton(quest) {
		public override string Name => $"{base.Name}_{Quest.Name}";
		public override double Priority => 101;
		public override void OnClick(NPC npc, Player player) {
			Main.npcChatText = Language.GetTextValue("Mods.Origins.Quests.ArmsDealer.Shardcannon.YouLostThatGunIGaveYou");
			Shardcannon_Quest.GiveGun();
		}
		public override string Text(NPC npc, Player player) => Language.GetTextValue("Mods.Origins.Quests.ArmsDealer.Shardcannon.ILostThatGunYouGaveMe");
		public override bool IsActive(NPC npc, Player player) {
			if (!Questing.QuestListSelected || npc.type != NPCID.ArmsDealer) return false;
			return Quest.HasQuestButton(npc, player);
		}
	}
	public class Harpoon_Burst_Rifle_Quest : Quest {
		public string loc_key = "Mods.Origins.Quests.ArmsDealer.Harpoon_Burst_Rifle.";
		public QuestChatButton Button { get; protected set; }
		//backing field for Stage property
		int stage = 0;
		int progress = 0;
		const int target = 50;
		public void UpdateKillCount() {
			if (stage != 1) return;
			if (++progress >= target) {
				HasNotification = true;
				Stage = 2;
			}
			ShouldSync = true;
		}
		//Stage property so changing quest stage also updates its event handlers
		public override int Stage {
			get => stage;
			set => stage = value;
		}
		public override bool Started => Stage > 0;
		public override bool Completed => Stage > 2;
		public override void Load() {
			Mod.AddContent(Button = new Lost_Harpoon_Burst_Rifle_Chat_Button(this));
		}
		public override bool HasQuestButton(NPC npc, Player player) {
			static bool IsShardcannon(Item item) {
				return item.type == Harpoon_Burst_Rifle.ID && item.prefix == Imperfect_Prefix.ID && item.stack > 0;
			}
			static bool HasShardcannon(Item[] inventory) {
				for (int i = 0; i < inventory.Length; i++) {
					Item item = inventory[i];
					if (IsShardcannon(item)) {
						return true;
					}
				}
				return false;
			}
			if (HasShardcannon(player.inventory)) {
				return false;
			}
			if (IsShardcannon(player.inventory[Main.InventorySlotsTotal]) || IsShardcannon(player.trashItem)) return false;
			if (HasShardcannon(player.bank.item)) {
				return false;
			}
			if (HasShardcannon(player.bank2.item)) {
				return false;
			}
			if (HasShardcannon(player.bank3.item)) {
				return false;
			}
			if (HasShardcannon(player.bank4.item)) {
				return false;
			}
			return Stage == 1;
		}
		public override bool CanStart(NPC npc) {
			return npc.type == NPCID.ArmsDealer && NPC.downedMechBossAny && !ShowInJournal() && ModContent.GetInstance<Shardcannon_Quest>().Completed;
		}
		public override void OnAccept(NPC npc) {
			if (Stage < 1) Stage = 1;// - set stage to 1 (kill harpies)
			LocalPlayerStarted = true;
			GiveGun();
			Main.npcChatText = Language.GetTextValue(loc_key + "Start");
		}
		public override string GetInquireText(NPC npc) => Language.GetTextValue(loc_key + "Inquire");
		public override bool CanComplete(NPC npc) => npc.type == NPCID.ArmsDealer && Stage == 2;
		public override string ReadyToCompleteText(NPC npc) => Language.GetOrRegister(loc_key + "ReadyToComplete").Value;
		public override void OnComplete(NPC npc) {
			for (int i = 0; i < Main.InventoryItemSlotsCount; i++) {
				Item item = Main.LocalPlayer.inventory[i];
				if (item.type == ModContent.ItemType<Harpoon_Burst_Rifle>() && item.prefix == ModContent.PrefixType<Imperfect_Prefix>()) {
					item.TurnToAir();
					Main.npcChatText = Language.GetTextValue(loc_key + "Complete");
					Stage = 3;
					ShouldSync = true;
					return;
				}
			}
			Main.npcChatText = Language.GetTextValue(loc_key + "WhereGun");
			Stage = 3;
			ShouldSync = true;
		}
		public static void GiveGun() {
			int index = Item.NewItem(
				Main.LocalPlayer.GetSource_GiftOrReward(),
				Main.LocalPlayer.position,
				Main.LocalPlayer.Size,
				ModContent.ItemType<Harpoon_Burst_Rifle>(),
				1,
				prefixGiven: ModContent.PrefixType<Imperfect_Prefix>()
			);
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				NetMessage.SendData(MessageID.SyncItem, -1, -1, null, index, 1f);
			}
		}
		public override bool ShowInJournal() => Completed || (base.ShowInJournal() && LocalPlayerStarted);
		public override string GetJournalPage() {
			return Language.GetTextValue(
				loc_key + "Journal", //translation key
				progress,
				target,
				StageTagOption(progress >= target) //used in a quest stage tag to show the stage as completed
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
	}
	[Autoload(false)]
	file class Lost_Harpoon_Burst_Rifle_Chat_Button(Quest quest) : QuestChatButton(quest) {
		public override string Name => $"{base.Name}_{Quest.Name}";
		public override double Priority => 101;
		public override void OnClick(NPC npc, Player player) {
			Main.npcChatText = Language.GetTextValue("Mods.Origins.Quests.ArmsDealer.Harpoon_Burst_Rifle.YouLostThatGunIGaveYou");
			Harpoon_Burst_Rifle_Quest.GiveGun();
		}
		public override string Text(NPC npc, Player player) => Language.GetTextValue("Mods.Origins.Quests.ArmsDealer.Harpoon_Burst_Rifle.ILostThatGunYouGaveMe");
		public override bool IsActive(NPC npc, Player player) {
			if (Questing.QuestListSelected || npc.type != NPCID.ArmsDealer) return false;
			return Quest.HasQuestButton(npc, player);
		}
	}
}
