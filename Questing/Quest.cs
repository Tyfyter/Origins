using BetterDialogue.UI;
using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.GameContent.Personalities;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Origins.Reflection;

namespace Origins.Questing {
	public abstract class Quest : ModType {
		public virtual bool SaveToWorld => true;
		public virtual bool Started => false;
		public virtual bool Completed => false;
		public virtual string CompleteButtonText => Language.GetOrRegister("Mods.Origins.Interface.CompleteQuestGeneric").Value;
		public string NameKey { get; protected set; }
		public string NameValue => Language.GetOrRegister(NameKey).Value;
		public virtual int Stage { get; set; }
		public bool ShouldSync { get; protected set; }
		public bool HasNotification { get; set; }
		public bool ActiveForLocalPlayer => Started && !Completed && LocalPlayerStarted;
		public bool LocalPlayerStarted {
			get => Main.LocalPlayer?.GetModPlayer<OriginPlayer>()?.startedQuests?.Contains(NameKey) ?? Started;
			set {
				if (value) {
					Main.LocalPlayer.GetModPlayer<OriginPlayer>().startedQuests.Add(NameKey);
				} else {
					Main.LocalPlayer.GetModPlayer<OriginPlayer>().startedQuests.Remove(NameKey);
				}
			}
		}

		public abstract string GetInquireText(NPC npc);
		public abstract bool CanStart(NPC npc);
		public virtual bool CanComplete(NPC npc) => false;
		public abstract string ReadyToCompleteText(NPC npc);
		public abstract void OnComplete(NPC npc);
		public abstract void OnAccept(NPC npc);
		public virtual bool ShowInJournal() {
			return Started;
		}
		public virtual bool GrayInJournal() {
			return Completed;
		}
		public virtual string GetJournalPage() {
			return "";
		}
		public virtual bool HasQuestButton(NPC npc, Player player) => false;
		public virtual void SaveData(TagCompound tag) { }
		public virtual void LoadData(TagCompound tag) { }
		#region events
		public Action<NPC> KillEnemyEvent { get; protected set; }
		public Action PreUpdateInventoryEvent { get; protected set; }
		public Action<Item> UpdateInventoryEvent { get; protected set; }
		#endregion events
		public sealed override void SetupContent() {
			SetStaticDefaults();
			_ = NameValue;
		}
		protected sealed override void Register() {
			if (SaveToWorld && Mod.Side != ModSide.Both) {
				throw new Exception($"The Quest \"{NameValue}\" is saved to the world but not in a \"Both\" side mod, this is not allowed as it would break networking");
			}
			ModTypeLookup<Quest>.Register(this);
			NameKey ??= $"Mods.{FullName}";
			Quest_Registry.RegisterQuest(this);
			Mod.AddContent(new QuestInquireChatButton(this));
			Mod.AddContent(new QuestAcceptChatButton(this));
			Mod.AddContent(new QuestCompleteChatButton(this));
		}
		public int Type { get; internal set; }
		public int NetID { get; internal set; } = -1;
		public static string StageTagOption(bool completed) => completed ? "/completed" : "";
		public static int[] ConsumeItems(Item[] inventory, params (Predicate<Item> match, int count)[] items) {
			int[] counts = new int[items.Length];
			for (int j = 0; j < inventory.Length; j++) {
				Item item = inventory[j];
				for (int i = 0; i < items.Length; i++) {
					(Predicate<Item> match, int count) current = items[i];
					if (current.count <= 0) continue;
					if (current.match(item)) {
						if (current.count >= item.stack) {
							counts[i] += item.stack;
							current.count -= item.stack;
							item.TurnToAir();
						} else {
							counts[i] += current.count;
							item.stack -= current.count;
							current.count = 0;
						}
						items[i] = current;
					}
				}
			}
			return counts;
		}
		public void CheckSync() {
			if (ShouldSync) {
				Sync();
			}
		}
		public void Sync(int toClient = -1, int ignoreClient = -1) {
			if (Main.netMode == NetmodeID.SinglePlayer) return;
			TagCompound dataTag = [];
			SaveData(dataTag);
			Mod.Logger.Info($"Syncing {NameValue}, to {(toClient == -1 ? "everyone" : Main.player[toClient].name)} with data: {dataTag}");
			ModPacket packet = Origins.instance.GetPacket();
			packet.Write(Origins.NetMessageType.sync_quest);
			packet.Write(Type);
			SendSync(packet);
			packet.Send(toClient, ignoreClient);
			ShouldSync = false;
		}
		public virtual void SendSync(BinaryWriter writer) { }
		public virtual void ReceiveSync(BinaryReader reader) { }
		public static Condition QuestCondition<T>() where T : Quest => ModContent.GetInstance<T>().CompletedCondition;
		private Condition completedCondition;
		public Condition CompletedCondition => completedCondition ??= new Condition(
			Language.GetOrRegister("Mods.Origins.Conditions.QuestCompleted").WithFormatArgs(Language.GetOrRegister(NameKey)),
			() => Completed
		);
	}
	public static class Questing {
		static bool questListSelected = false;
		public static bool QuestListSelected {
			get => questListSelected;
			set {
				questListSelected = value;
				fromQuest = null;
			}
		}
		public static Quest fromQuest = null;
		public static Quest selectedQuest = null;
		public static void ExitChat() {
			QuestListSelected = false;
			selectedQuest = null;
		}
		public static void EnterQuestList(NPC npc) {
			if (CanEnterQuestList(npc)) {
				QuestListSelected = true;
				string textKey = $"Mods.Origins.Quests.{npc.ModNPC?.Name ?? NPCID.Search.GetName(npc.type)}.Quest_Menu";
				if (!Language.Exists(textKey)) textKey = $"Mods.Origins.Quests.Common.Quest_Menu.{Main.rand.Next(3)}";
				Main.npcChatText = Language.GetOrRegister(textKey).Value;
			} else {
				QuestListSelected = false;
				Main.npcChatText = npc.GetChat();
			}
		}
		public static bool CanEnterQuestList(NPC npc) => Quest_Registry.Quests.Any(q => q.CanStart(npc) || (!q.Completed && q.CanComplete(npc)) || q.HasQuestButton(npc, Main.LocalPlayer));
	}
	public class ConditionalNPCPreferenceTrait : NPCPreferenceTrait, IShopPersonalityTrait {
		public Condition condition;
		public new void ModifyShopPrice(HelperInfo info, ShopHelper shopHelperInstance) {
			if (info.nearbyNPCsByType[NpcId] && Level != 0 && Enum.IsDefined(Level) && condition.IsMet()) {
				ShopMethods.AddHappinessReportText(shopHelperInstance, $"{Level}NPC", new {
					NPCName = NPC.GetFullnameByID(NpcId)
				});
				ShopMethods._currentPriceAdjustment.SetValue(shopHelperInstance, ShopMethods._currentPriceAdjustment.GetValue(shopHelperInstance) * NPCHappiness.AffectionLevelToPriceMultiplier[Level]);
			}
		}
	}
}
