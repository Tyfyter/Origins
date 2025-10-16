using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Face)]
	public class Comb : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Combat,
			WikiCategories.GenericBoostAcc
		];
		public override void SetStaticDefaults() {
			ItemID.Sets.ShimmerTransformToItem[ItemID.Shackle] = ModContent.ItemType<Comb>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Comb>()] = ItemID.Shackle;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(28, 28);
			Item.value = Item.sellPrice(silver: 20);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.GetDamage(DamageClass.Generic) *= 1.05f;
		}
	}
}
