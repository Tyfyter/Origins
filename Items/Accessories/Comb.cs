using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class Comb : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Comb");
            Tooltip.SetDefault("'Every hero needs an iconic haircut'");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.Shackle);
            Item.value = Item.sellPrice(silver: 30);
            Item.rare = ItemRarityID.White;
        }
        public override void UpdateEquip(Player player) {
            player.GetDamage(DamageClass.Generic) *= 1.03f;
        }
    }
}
