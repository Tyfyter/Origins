using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class Trap_Charm : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Trap Charm");
            Tooltip.SetDefault("Reduces damage recieved from traps");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.Aglet);
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.Green;
        }
        public override void UpdateEquip(Player player) {
            //player.GetModPlayer<OriginPlayer>().trapCharm = true;
        }
    }
}
