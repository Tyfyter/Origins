using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class Headstomperz : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Headstomperz");
            Tooltip.SetDefault("Double tap movement keys to launch yourself forward damaging and piercing any enemies in the way");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.HermesBoots);
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.LightRed;
        }
        public override void UpdateEquip(Player player) {
            //player.GetModPlayer<OriginPlayer>().headStomperz = true;
        }
    }
}
