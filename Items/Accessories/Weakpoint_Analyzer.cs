using Origins.Dev;
using Origins.Journal;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Weakpoint_Analyzer : ModItem, ICustomWikiStat, IJournalEntrySource {
		public string[] Categories => [
			"Combat",
			"RangedBoostAcc"
		];
		public string EntryName => "Origins/" + typeof(Weakpoint_Analyzer_Entry).Name;
		public class Weakpoint_Analyzer_Entry : JournalEntry {
			public override string TextKey => "Weakpoint_Analyzer";
			public override JournalSortIndex SortIndex => new("The_Crimson", 11);
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(14, 28);
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Orange;
			Item.master = true;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().weakpointAnalyzer = true;
		}
	}
}
