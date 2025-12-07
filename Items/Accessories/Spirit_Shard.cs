using Origins.Dev;
using Origins.Journal;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Spirit_Shard : ModItem, IJournalEntrySource, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Combat,
			WikiCategories.SummonBoostAcc
		];
		public string EntryName => "Origins/" + typeof(Eccentric_Stone_Entry).Name;
		
		public override void SetDefaults() {
			Item.DefaultToAccessory(16, 16);
			Item.rare = ItemRarityID.Green;
			Item.value = Item.sellPrice(silver: 250);
		}
		public override void UpdateEquip(Player player) {
			player.maxMinions += 1;
			player.GetDamage(DamageClass.Summon) += 0.1f;
			player.GetModPlayer<OriginPlayer>().spiritShard = true;
		}
	}
}
