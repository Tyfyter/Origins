using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class Advanced_Imaging : ModItem {
        static short glowmask;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Advanced Imaging");
            Tooltip.SetDefault("Increased projectile speed\nImmunity to Confusion\n\"The future is now\"");
            glowmask = Origins.AddGlowMask(this);
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.YoYoGlove);
            Item.handOffSlot = -1;
            Item.handOnSlot = -1;
            Item.value = Item.buyPrice(gold: 25);
            Item.rare = ItemRarityID.Yellow;
            Item.glowMask = glowmask;
        }
        public override void UpdateEquip(Player player) {
            player.GetModPlayer<OriginPlayer>().advancedImaging = true;
            player.buffImmune[BuffID.Confused] = true;
        }
    }
}
