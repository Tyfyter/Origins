using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.HandsOn)]
	public class Bomb_Charm : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Vitality,
			WikiCategories.ExplosiveBoostAcc,
			WikiCategories.SelfDamageProtek
		];
		public override void SetStaticDefaults() {
			ItemID.Sets.ShimmerTransformToItem[ItemID.BandofRegeneration] = ModContent.ItemType<Bomb_Charm>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Bomb_Charm>()] = ItemID.BandofRegeneration;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 26);
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().explosiveSelfDamage -= 0.2f;
		}
	}
}
