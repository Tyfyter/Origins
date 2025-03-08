using Origins.Dev;
using Origins.Journal;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Spirit_Shard : ModItem, IJournalEntrySource, ICustomWikiStat {
		public string[] Categories => [
			"Combat",
			"LoreItem",
			"SummonBoostAcc"
		];
		public string EntryName => "Origins/" + typeof(Eccentric_Stone_Entry).Name;
		
		public override void SetDefaults() {
			Item.DefaultToAccessory(16, 16);
			Item.rare = ItemRarityID.Green;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().spiritShard = true;
		}
	}
}
