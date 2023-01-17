using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class Stealth_Ravel : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Stealth Ravel");
            Tooltip.SetDefault("Double tap down to transform into a small, rolling ball\nEnemies are less likely to target you when ravelled");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.accessory = true;
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(gold: 8);
        }
        public override void UpdateEquip(Player player) {
            //player.GetModPlayer<OriginPlayer>().ravelStealth = true;
        }
    }
}
