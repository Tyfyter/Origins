using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    [AutoloadEquip(EquipType.Face)]
    public class Comb : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Comb");
            Tooltip.SetDefault("3% increased damage\n'Every hero needs an iconic haircut'");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            sbyte faceSlot = Item.faceSlot;
            Item.CloneDefaults(ItemID.Shackle);
            Item.faceSlot = faceSlot;
            Item.handOffSlot = -1;
            Item.handOnSlot = -1;
            Item.defense = 0;
            Item.value = Item.sellPrice(silver: 30);
            Item.rare = ItemRarityID.Green;
        }
        public override void UpdateEquip(Player player) {
            player.GetDamage(DamageClass.Generic) *= 1.03f;
        }
    }
}
