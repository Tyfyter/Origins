using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    [AutoloadEquip(EquipType.HandsOn)]
    public class Destructive_Claws : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Destructive Claws");
            Tooltip.SetDefault("30% increased explosive throwing velocity\nIncreases attack speed of thrown explosives\nEnables autouse for all explosive weapons");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            sbyte handOnSlot = Item.handOnSlot;
            sbyte handOffSlot = Item.handOffSlot;
            Item.CloneDefaults(ItemID.YoYoGlove);
            Item.handOffSlot = handOffSlot;
            Item.handOnSlot = handOnSlot;
            Item.value = Item.sellPrice(gold: 3);
            Item.rare = ItemRarityID.Orange;
        }
        public override void UpdateEquip(Player player) {
            player.GetModPlayer<OriginPlayer>().destructiveClaws = true; //Needs autouse and usetime boosts
            player.GetModPlayer<OriginPlayer>().explosiveThrowSpeed += 0.3f;
        }
    }
}
