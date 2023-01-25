using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class IWTPA_Standard : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("I-WTPA Standard");
            Tooltip.SetDefault("Explosive fuse time reduced");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.Aglet);
            Item.value = Item.sellPrice(silver: 20);
            Item.rare = ItemRarityID.Blue;
        }
        public override void UpdateEquip(Player player) {
            //player.GetModPlayer<OriginPlayer>().iwtpaStandard = true;
        }
    }
}
