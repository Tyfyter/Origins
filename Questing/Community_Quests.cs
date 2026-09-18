using Origins.Items.Accessories;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Origins.Questing; 
public class SPE_Suggestions : Quest {
	const string base_key = "Mods.Origins.Quests.Community.SPE_Suggestions.";
	public override bool SaveToWorld => false;
	public override int Stage {
		get => field;
		set => field = value;
	}
	public override bool Started => Stage > 0;
	public override bool Completed => Stage > 1;
	public override bool CanStart(NPC npc) => npc.type == NPCID.Steampunker && Stage == 0 && Main.LocalPlayer.OriginPlayer().itemCounts[ModContent.ItemType<Space_Pirates_Eye>()] > 0;
	public override string GetInquireText(NPC npc) => Language.GetTextValue(base_key + "Inquire");
	public override void OnAccept(NPC npc) {
		Stage = 1;
		Main.npcChatText = Language.GetTextValue(base_key + "Start");
	}
	public override string ReadyToCompleteText(NPC npc) => Language.GetOrRegister(base_key + "ReadyToComplete").Value;
	public override bool CanComplete(NPC npc) {
		if (npc.type != NPCID.Steampunker) return false; // NPCs other than the merchant won't have any dialogue related to this quest
		return false;
	}
	public override void OnComplete(NPC npc) {
		Main.npcChatText = Language.GetTextValue(base_key + "Complete");
		Stage = 2;
		ShouldSync = true;
	}
	public override string GetJournalPage() {
		return Language.GetTextValue(
			base_key + "Journal"
		);
	}
	public override void SetStaticDefaults() {
		NameKey = base_key + "Name";
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
