using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Back)]
	public class Dysfunctional_Endless_Explosives_Bag : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat",
			"ExplosiveBoostAcc"
		];
		public override void SetStaticDefaults() {
			ItemID.Sets.ShimmerTransformToItem[ItemID.MagicQuiver] = ModContent.ItemType<Dysfunctional_Endless_Explosives_Bag>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Dysfunctional_Endless_Explosives_Bag>()] = ItemID.MagicQuiver;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 34);
			Item.rare = ItemRarityID.Gray;
			Item.value = Item.sellPrice(gold: 3);
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().endlessExplosives = true;
		}
	}
}
