using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    [AutoloadEquip(EquipType.Back)]
	public class Endless_Explosives : ModItem {
		public override void SetStaticDefaults() {
			ItemID.Sets.ShimmerTransformToItem[ItemID.MagicQuiver] = ModContent.ItemType<Endless_Explosives>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Endless_Explosives>()] = ItemID.MagicQuiver;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 34);
			Item.rare = ItemRarityID.Gray;
			Item.value = Item.sellPrice(gold: 3);
		}
		public override void UpdateEquip(Player player) {
			//player.GetModPlayer<OriginPlayer>().endlessExplosives = true;
		}
	}
}
