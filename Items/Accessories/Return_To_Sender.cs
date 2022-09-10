using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class Return_To_Sender : ModItem {
        static short glowmask;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Return To Sender");
            Tooltip.SetDefault("Attackers take 75% of their damage");
            glowmask = Origins.AddGlowMask(this);
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.accessory = true;
            Item.width = 30;
            Item.height = 30;
            Item.rare = ItemRarityID.Expert;
            Item.glowMask = glowmask;
        }
        public override void UpdateEquip(Player player) {
            player.thorns += 0.75f;
        }
    }
}
