using Origins.Items.Accessories;
using Origins.Items.Materials;
using Origins.Items.Weapons.Melee;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Origins.Questing {
	public class Journal_Quest : Quest {
		public const string loc_key = "Mods.Origins.Quests.Guide.Journal.";
		public override bool SaveToWorld => false;
		public override bool Started => Completed;
		public override bool Completed => Main.LocalPlayer?.OriginPlayer()?.journalUnlocked ?? false;
		public override bool CanStart(NPC npc) => npc.type == NPCID.Guide && !Completed && Main.npcChatText != Language.GetTextValue(loc_key + "Start");
		public override string GetInquireText(NPC npc) => Language.GetTextValue(loc_key + "Inquire", Main.worldName);
		public override void OnAccept(NPC npc) {
			Main.npcChatText = Language.GetTextValue(loc_key + "Start");
		}
		public override string ReadyToCompleteText(NPC npc) => Language.GetOrRegister(loc_key + "ReadyToComplete").Value;
		public override string GetJournalPage() => Language.GetTextValue(loc_key + "Journal");
		public override void SetStaticDefaults() {
			NameKey = loc_key + "Name";
		}
		public override void OnComplete(NPC npc) {}
		public override bool ShowInJournal() => true;
		public override bool GrayInJournal() => true;
	}
}
