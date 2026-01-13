using BetterDialogue.UI;
using Origins.Achievements;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Questing {
	public class QuestListChatButton : ChatButton {
		public override double Priority => 101.0;
		public override void OnClick(NPC npc, Player player) {
			Questing.EnterQuestList(npc);
		}
		public override string Text(NPC npc, Player player) => Language.GetOrRegister("Mods.Origins.Interface.Quests").Value;
		public override bool IsActive(NPC npc, Player player) {
			if (Questing.QuestListSelected) return false;
			if (Questing.selectedQuest is not null) return false;
			return Questing.CanEnterQuestList(npc);
		}
	}
	public abstract class QuestChatButton(Quest quest) : ChatButton {
		public Quest Quest => quest;
	}
	[Autoload(false)]
	public class QuestInquireChatButton(Quest quest) : QuestChatButton(quest) {
		public override string Name => $"{base.Name}_{Quest.Name}";
		public override double Priority => 0;
		public override void OnClick(NPC npc, Player player) {
			Questing.selectedQuest = Quest;
			Main.npcChatText = Quest.CanComplete(npc) ? Quest.ReadyToCompleteText(npc) : Quest.GetInquireText(npc);
		}
		public override string Text(NPC npc, Player player) => Quest.NameValue;
		public override bool IsActive(NPC npc, Player player) {
			if (!Questing.QuestListSelected) return false;
			if (Questing.selectedQuest is not null) return false;
			return Quest.CanStart(npc) || (!Quest.Completed && Quest.CanComplete(npc));
		}
	}
	[Autoload(false)]
	public class QuestAcceptChatButton(Quest quest) : QuestChatButton(quest) {
		public override string Name => $"{base.Name}_{Quest.Name}";
		public override double Priority => 0;
		public override void OnClick(NPC npc, Player player) {
			Quest.OnAccept(npc);
		}
		public override string Text(NPC npc, Player player) => Language.GetOrRegister("Mods.Origins.Interface.AcceptQuest").Value;
		public override bool IsActive(NPC npc, Player player) {
			if (Questing.selectedQuest != Quest) return false;
			return Quest.CanStart(npc);
		}
	}
	[Autoload(false)]
	public class QuestCompleteChatButton(Quest quest) : QuestChatButton(quest) {
		public override string Name => $"{base.Name}_{Quest.Name}";
		public override double Priority => 0;
		public override void OnClick(NPC npc, Player player) {
			Quest.OnComplete(npc);
			player.OriginPlayer().goingPlacesTracker.CompleteQuest(Quest);
		}
		public override string Text(NPC npc, Player player) => Quest.CompleteButtonText;
		public override bool IsActive(NPC npc, Player player) {
			if (Questing.selectedQuest != Quest) return false;
			return !Quest.Completed && Quest.CanComplete(npc);
		}
	}
	public class QuestBackChatButton : ChatButton {
		public override double Priority => 99.9;
		public override void OnClick(NPC npc, Player player) {
			if (Questing.selectedQuest == null) {
				Questing.QuestListSelected = false;
				Main.npcChatText = Main.npc[player.talkNPC].GetChat();
			} else {
				Questing.selectedQuest = null;
				Questing.EnterQuestList(npc);
			}
		}
		public override string Text(NPC npc, Player player) => Language.GetTextValue("UI.Back");
		public override bool IsActive(NPC npc, Player player) {
			if (!Questing.QuestListSelected && Questing.selectedQuest == null) return false;
			return true;
		}
	}
	public class QuestChatGlobalButton : GlobalChatButton {
		public override bool? IsActive(ChatButton chatButton, NPC npc, Player player) {
			if (Questing.QuestListSelected) return chatButton is QuestChatButton or QuestBackChatButton;
			return null;
		}
	}
}
