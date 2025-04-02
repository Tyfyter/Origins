using Origins.Items.Other;
using Origins.Items.Other.Consumables;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Origins.Questing {
	public class Quest_Quest : Quest {
		public const string loc_key = "Mods.Origins.Quests.Guide.Quest_Quest.";
		public override bool SaveToWorld => false;
		public override bool Started => Main.LocalPlayer?.OriginPlayer()?.journalUnlocked ?? false;
		bool completed = false;
		public override bool Completed {
			get {
				if (!Started) return false;
				if (completed) return true;
				for (int i = 0; i < Quest_Registry.Quests.Count; i++) {
					if (Quest_Registry.Quests[i] == this) continue;
					if (Quest_Registry.Quests[i].Completed) {
						HasNotification = false;
						return completed = true;
					}
				}
				return false;
			}
		}
		public override bool CanStart(NPC npc) => npc.type == NPCID.Guide && !Started;
		public override string GetInquireText(NPC npc) => Language.GetTextValue(loc_key + "Inquire", Main.worldName);
		public override void OnAccept(NPC npc) {
			Main.npcChatText = Language.GetTextValue(loc_key + "Start");
			Main.LocalPlayer.OriginPlayer().journalUnlocked = true;
			SoundEngine.PlaySound(SoundID.Grab);
			PopupText.NewText(
				new AdvancedPopupRequest() {
					Text = ModContent.GetInstance<Journal_Item>().DisplayName.Value,
					DurationInFrames = 60,
					Velocity = Vector2.UnitY * -7,
					Color = Colors.RarityAmber
				},
				Main.LocalPlayer.Top
			);
			HasNotification = true;
		}
		public override string ReadyToCompleteText(NPC npc) => "";
		public override string GetJournalPage() {
			return Language.GetTextValue(
				loc_key + "Journal",
				StageTagOption(Completed)
			);
		}
		public override void SetStaticDefaults() {
			NameKey = loc_key + "Name";
		}
		public override void OnComplete(NPC npc) { }
		public override bool ShowInJournal() => true;
		public override void SaveData(TagCompound tag) {
			//save stage and kills
			tag.Add("Completed", completed);
		}
		public override void LoadData(TagCompound tag) {
			//load stage and kills, note that it uses the Stage property so that it sets the event handlers
			//SafeGet returns the default value (0 for ints) if the tag doesn't have the data
			completed = tag.SafeGet<bool>("Completed");
		}
	}
	public class Assimilation_Quest : Quest {
		public const string loc_key = "Mods.Origins.Quests.Guide.Assimilation_Quest.";
		public override bool SaveToWorld => false;
		public override bool Started => Stage > 0;
		public override bool Completed => Stage > 0;
		public override bool CanStart(NPC npc) => npc.type == NPCID.Guide && !Started && (Main.LocalPlayer?.OriginPlayer()?.IterateAssimilation()?.Any(a => a.Percent > 0) ?? false);
		public override string GetInquireText(NPC npc) => Language.GetTextValue(loc_key + "Inquire", Main.worldName);
		public override void OnAccept(NPC npc) {
			Main.npcChatText = Language.GetTextValue(loc_key + "Start");
			Main.LocalPlayer.QuickSpawnItem(npc.GetSource_GiftOrReward(NameKey), ItemID.SpelunkerPotion, 5);
			Main.LocalPlayer.QuickSpawnItem(npc.GetSource_GiftOrReward(NameKey), ModContent.ItemType<Eye_Of_Aether>(), 5);
			Stage = 1;
		}
		public override string ReadyToCompleteText(NPC npc) => "";
		public override string GetJournalPage() {
			return Language.GetTextValue(
				loc_key + "Journal",
				StageTagOption(Completed)
			);
		}
		public override void SetStaticDefaults() {
			NameKey = loc_key + "Name";
		}
		public override void OnComplete(NPC npc) {
			
		}
		public override void SaveData(TagCompound tag) {
			//save stage
			tag.Add("Stage", Stage);
		}
		public override void LoadData(TagCompound tag) {
			//load stage, note that it uses the Stage property so that it sets the event handlers
			//SafeGet returns the default value (0 for ints) if the tag doesn't have the data
			Stage = tag.SafeGet<int>("Stage");
		}
	}
}
