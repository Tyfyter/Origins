using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class Rasterize : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Still Image");
            Tooltip.SetDefault("Attacks may inflict \"Rasterize\"");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.accessory = true;
            Item.width = 22;
            Item.height = 20;
            Item.rare = ItemRarityID.Blue;
        }
        public override void UpdateEquip(Player player) {
            player.GetModPlayer<OriginPlayer>().rasterize = true;
        }
    }
}
