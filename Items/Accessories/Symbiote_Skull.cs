using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class Symbiote_Skull : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Symbiote Skull");
            Tooltip.SetDefault("Attacks tenderize targets");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.Aglet);
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.Blue;
        }
        public override void UpdateEquip(Player player) {
            player.GetModPlayer<OriginPlayer>().symbioteSkull = true;
        }
    }
}
