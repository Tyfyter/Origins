using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class Return_To_Sender : ModItem {
        static short glowmask;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Return To Sender");
            Tooltip.SetDefault("Attackers recieve all damage inflicted to the user");
            glowmask = Origins.AddGlowMask(this);
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.accessory = true;
            Item.width = 30;
            Item.height = 30;
            Item.rare = ItemRarityID.Blue;
            Item.glowMask = glowmask;
            Item.value = Item.sellPrice(gold: 1);
        }
        public override void UpdateEquip(Player player) {
            player.thorns += 1f;
        }
    }
}
