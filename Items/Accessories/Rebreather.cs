using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Face)]
	public class Rebreather : ModItem {
		public static sbyte FaceSlot { get; private set; }
		public override void SetStaticDefaults() {
            DisplayName.SetDefault("Rebreather");
            Tooltip.SetDefault("Gain more breath as you move in water");
			FaceSlot = Item.faceSlot;
			SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaultsKeepSlots(ItemID.YoYoGlove);
            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(gold: 1);
        }
        public override void UpdateEquip(Player player) {
            player.GetModPlayer<OriginPlayer>().rebreather = true;
        }
    }
}
