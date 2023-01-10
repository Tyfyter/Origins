using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class Rasterize : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Rasterized Fish");
            Tooltip.SetDefault("Chance to stun enemies");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.accessory = true;
            Item.width = 22;
            Item.height = 20;
            Item.value = Item.sellPrice(gold: 3);
            Item.rare = ItemRarityID.LightRed;
        }
        public override void UpdateEquip(Player player) {
            player.GetModPlayer<OriginPlayer>().rasterize = true;
        }
    }
}
