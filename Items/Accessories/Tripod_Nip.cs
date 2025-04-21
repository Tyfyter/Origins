using Origins.Dev;
using Origins.Journal;
using Origins.NPCs.Defiled;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Tripod_Nip : ModItem, IJournalEntrySource, ICustomWikiStat {
		public string[] Categories => [
			"Misc",
			"LoreItem"
		];
		public string EntryName => "Origins/" + typeof(Tripod_Nip_Entry).Name;
		
		public override void SetDefaults() {
			Item.DefaultToAccessory(30, 24);
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void UpdateEquip(Player player) {
			player.aggro -= 400;
			player.calmed = true;
			player.npcTypeNoAggro[ModContent.NPCType<Defiled_Tripod>()] = true;
		}
	}
	public class Tripod_Nip_Entry : JournalEntry {
		public override string TextKey => "Tripod_Nip";
		public override JournalSortIndex SortIndex => new("The_Defiled", 17);
	}
}
