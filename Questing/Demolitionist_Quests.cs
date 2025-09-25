using AltLibrary.Common.Conditions;
using Origins.Items.Accessories;
using Origins.Items.Other.Consumables;
using Origins.Items.Tools;
using Origins.Items.Weapons.Demolitionist;
using Origins.Items.Weapons.Summoner;
using Origins.NPCs;
using Origins.Tiles.Brine;
using Origins.UI;
using Origins.World;
using Origins.World.BiomeData;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Origins.Questing {
	[Autoload(false)]
	file class Peat_Moss_Quest_Chat_Button(Quest quest) : QuestChatButton(quest) {
		public override string Name => $"{base.Name}_{Quest.Name}";
		public override double Priority => 101;
		public override void OnClick(NPC npc, Player player) {
			int rewardsCount = Peat_Moss_Quest.GetItems().Count(i => i.AllConditionsMet);
			int mossGiven = Quest.ConsumeItems(player.inventory, ((i) => i.ModItem is Peat_Moss_Item, Peat_Moss_Quest.Rewards[^1].PeatAmount - ModContent.GetInstance<OriginSystem>().peatSold))[0];
			ModContent.GetInstance<OriginSystem>().peatSold += mossGiven;
			if (mossGiven > 0) {
				if (!NetmodeActive.SinglePlayer) {
					ModPacket packet = Mod.GetPacket();
					packet.Write(Origins.NetMessageType.sync_peat);
					packet.Write((short)OriginSystem.Instance.peatSold);
					packet.Send(-1, player.whoAmI);
				}
				SoundEngine.PlaySound(SoundID.Grab, npc.Center);
				Main.npcChatText = Language.GetOrRegister(Peat_Moss_Quest.lockey + (Peat_Moss_Quest.GetItems().Count(i => i.AllConditionsMet) > rewardsCount ? "GaveMossUnlocked" : "GaveMoss")).Value;
				int value = 35 * mossGiven;
				void SpawnCoins(int type, int count) {
					if (count > 0) Main.LocalPlayer.QuickSpawnItem(npc.GetSource_GiftOrReward(Quest.FullName), type, count);
				}
				SpawnCoins(ItemID.PlatinumCoin, (value / 1000000) % 100);
				SpawnCoins(ItemID.GoldCoin, (value / 10000) % 100);
				SpawnCoins(ItemID.SilverCoin, (value / 100) % 100);
				SpawnCoins(ItemID.CopperCoin, value % 100);
			}
		}
		public override string Text(NPC npc, Player player) => Language.GetOrRegister(Peat_Moss_Quest.lockey + "GiveMoss").Value;
		public override bool IsActive(NPC npc, Player player) {
			if (!Questing.QuestListSelected) return false;
			return Quest.HasQuestButton(npc, player);
		}
	}
	public class Peat_Moss_Quest : Quest {
		public const string lockey = "Mods.Origins.Quests.Demolitionist.Peat_Moss.";
		public static IEnumerable<ShopItem> GetItems() {
			yield return new ShopItem<Peatball>(15);
			yield return new ShopItem<Flashbang>(25);
			yield return new ShopItem<IWTPA_Standard>(35);
			yield return new ShopItem<Impact_Grenade>(40);
			yield return new ShopItem<Defiled_Spirit>(50, OriginGlobalNPC.WorldEvilBossCondition<Defiled_Wastelands_Alt_Biome>("Mods.Origins.Conditions.DownedDefiledAmalgamation"));
			yield return new ShopItem<Ameballoon>(60, OriginGlobalNPC.WorldEvilBossCondition<Riven_Hive_Alt_Biome>("Mods.Origins.Conditions.DownedWorldCracker"));
			yield return new ShopItem<Impact_Bomb>(70);
			yield return new ShopItem<Brainade>(81, Condition.DownedBrainOfCthulhu);
			yield return new ShopItem<Link_Grenade>(85, ShopConditions.GetWorldEvilCondition<Ashen_Alt_Biome>());
			yield return new ShopItem<Nitro_Crate>(100);
			yield return new ShopItem<Shrapnel_Bomb>(125, OriginGlobalNPC.WorldEvilBossCondition<Ashen_Alt_Biome>("Mods.Origins.Conditions.DownedScrapper"));
			yield return new ShopItem<Magic_Tripwire>(135);
			yield return new ShopItem<Bomb_Artifact>(145);
			yield return new ShopItem<Trash_Lid>(160);
			yield return new ShopItem(ItemID.Beenade, 170, [OriginGlobalNPC.PeatSoldCondition(170), Condition.NotTheBeesWorld]);
			yield return new ShopItem<Impact_Dynamite>(180, Condition.Hardmode);
			yield return new ShopItem<Alkaline_Grenade>(200, Boss_Tracker.Conditions[nameof(Boss_Tracker.downedLostDiver)]);
			yield return new ShopItem<Alkaline_Bomb>(230, Boss_Tracker.Conditions[nameof(Boss_Tracker.downedLostDiver)]);
			yield return new ShopItem<Sonar_Dynamite>(230, Boss_Tracker.Conditions[nameof(Boss_Tracker.downedLostDiver)]);
			yield return new ShopItem<Indestructible_Saddle>(250, Condition.DownedMechBossAny);
			yield return new ShopItem<Absorption_Potion>(350);
			yield return new ShopItem<Caustica>(999, Condition.DownedGolem);
		}
		public override bool Started => LocalPlayerStarted || ModContent.GetInstance<OriginSystem>().peatSold > 0;
		public override bool Completed => ModContent.GetInstance<OriginSystem>().peatSold >= Rewards[^1].PeatAmount;
		public static ShopItem[] Rewards { get; private set; }
		public override void Load() {
			Mod.AddContent(new Peat_Moss_Quest_Chat_Button(this));
		}
		public override bool HasQuestButton(NPC npc, Player player) {
			if (Completed) return false;
			return npc.type == NPCID.Demolitionist && player.HasItem(ModContent.ItemType<Peat_Moss_Item>());
		}
		public override bool CanStart(NPC npc) {
			return npc.type == NPCID.Demolitionist && !ShowInJournal();
		}
		public override string GetInquireText(NPC npc) => Language.GetTextValue(lockey + "Inquire");
		public override void OnAccept(NPC npc) {
			Main.npcChatText = Language.GetTextValue(lockey + "Start");
			LocalPlayerStarted = true;
		}
		public override bool CanComplete(NPC npc) => false;
		public override string ReadyToCompleteText(NPC npc) => "";
		public override void OnComplete(NPC npc) { }
		public override bool ShowInJournal() => Completed || (base.ShowInJournal() && LocalPlayerStarted);
		public override string GetJournalPage() {
			int peatSold = ModContent.GetInstance<OriginSystem>().peatSold;
			return Language.GetTextValue(
				lockey + "Journal", //translation key
				StageTagOption(Completed),
				peatSold,
				Rewards[^1].PeatAmount,
				string.Join("\n", Rewards.Select(RewardSnippet))
			);
		}
		static string RewardSnippet(ShopItem item) {
			return $"[qian/{(item.Conditions.All(c => c.IsMet()) ? "u" : "")}text{string.Join(",", item.Conditions.Select(DeparseCondition))}:{item.ItemID}]";
		}
		static string DeparseCondition(Condition condition) {
			StringBuilder builder = new(condition.Description.Key);
			for (int i = 0; i < (condition.Description.BoundArgs?.Length ?? 0); i++) {
				builder.Append(';');
				builder.Append(condition.Description.BoundArgs[i].ToString());
			}
			string fullKey = builder.ToString();
			Quest_Reward_Item_List_Handler.conditions.TryAdd(fullKey, condition);
			return fullKey;
		}
		public override void SetStaticDefaults() {
			NameKey = lockey + "Name";
			SetupItems();
		}
		public static void SetupItems() {
			Rewards = GetItems().ToArray();
		}
		public record class ShopItem(int ItemID, int PeatAmount, params Condition[] Conditions) {
			public bool AllConditionsMet {
				get {
					for (int i = 0; i < Conditions.Length; i++) {
						if (!Conditions[i].IsMet()) return false;
					}
					return true;
				}
			}
		}
		public record class ShopItem<TItem>(int PeatAmount, params Condition[] Conditions) : ShopItem(ModContent.ItemType<TItem>(), PeatAmount, [OriginGlobalNPC.PeatSoldCondition(PeatAmount), .. (Conditions ?? [])]) where TItem : ModItem;
	}
}
