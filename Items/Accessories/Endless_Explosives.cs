using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    [AutoloadEquip(EquipType.Back)]
	public class Endless_Explosives : ModItem {
		public static int BackSlot { get; private set; }
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Dysfunctional Endless Explosives Bag");
			// Tooltip.SetDefault("15% chance not to consume thrown explosives or explosive ammo");
			BackSlot = Item.backSlot;
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 34);
			Item.rare = ItemRarityID.Gray;
			Item.value = Item.sellPrice(gold: 3);
			ItemID.Sets.ShimmerTransformToItem[ItemID.MagicQuiver] = ModContent.ItemType<Endless_Explosives>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Endless_Explosives>()] = ItemID.MagicQuiver;
		}
		public override void UpdateEquip(Player player) {
			//player.GetModPlayer<OriginPlayer>().endlessExplosives = true;
		}
	}
}
