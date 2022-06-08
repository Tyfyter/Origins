using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.HandsOn)]
    public class Bomb_Yeeter : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Bomb Handling Device");
            Tooltip.SetDefault("Also commonly referred to as the 'Bomb Yeeter'");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
        public override void SetDefaults() {
            sbyte handOnSlot = Item.handOnSlot;
            sbyte handOffSlot = Item.handOffSlot;
            Item.CloneDefaults(ItemID.YoYoGlove);
            Item.handOffSlot = handOffSlot;
            Item.handOnSlot = handOnSlot;
        }
        public override void UpdateEquip(Player player) {
            player.GetModPlayer<OriginPlayer>().bombHandlingDevice = true;
            player.GetModPlayer<OriginPlayer>().explosiveThrowSpeed+=0.5f;
        }
    }
}
