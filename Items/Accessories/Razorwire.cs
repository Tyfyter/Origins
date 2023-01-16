using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class Razorwire : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Razorwire");
            Tooltip.SetDefault("67% of damage recieved is split across 3 enemies\nEnemies are less likely to target you");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.accessory = true;
            Item.rare = ItemRarityID.Green;
            Item.value = Item.sellPrice(gold: 1);
        }
        public override void UpdateEquip(Player player) {
            player.aggro -= 400;
            //player.GetModPlayer<OriginPlayer>().razorwire = true;
        }
    }
}
