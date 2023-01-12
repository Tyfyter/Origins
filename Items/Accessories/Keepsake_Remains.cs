using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class Keepsake_Remains : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Keepsake Remains");
            Tooltip.SetDefault("Increases armor penetration by 5 and attacks tenderize targets");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.Aglet);
            Item.value = Item.sellPrice(gold: 3);
            Item.rare = ItemRarityID.Green;
        }
        public override void UpdateEquip(Player player) {
            player.GetModPlayer<OriginPlayer>().symbioteSkull = true;
            player.GetArmorPenetration(DamageClass.Generic) += 5;
        }
    }
}
