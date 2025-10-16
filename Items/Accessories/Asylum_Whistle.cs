using Origins.Dev;
using Origins.Journal;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Neck)]
	public class Asylum_Whistle : ModItem, IJournalEntrySource, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Combat,
			WikiCategories.SummonBoostAcc,
			WikiCategories.LoreItem
		];
		public string EntryName => "Origins/" + typeof(Asylum_Whistle_Entry).Name;
		public override void SetStaticDefaults() {
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Bomb_Handling_Device>()] = ModContent.ItemType<Asylum_Whistle>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Asylum_Whistle>()] = ModContent.ItemType<Bomb_Handling_Device>();
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(30, 28);
			Item.accessory = true;
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.sellPrice(gold: 1);
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().asylumWhistle = true;
			player.GetDamage(DamageClass.Summon) += 0.1f;
		}
	}
	public class Asylum_Whistle_Entry : JournalEntry {
		public override string TextKey => "Asylum_Whistle";
		public override JournalSortIndex SortIndex => new("The_Dungeon", 1);
	}
}
