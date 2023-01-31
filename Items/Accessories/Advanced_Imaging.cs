using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Face)]
    public class Advanced_Imaging : ModItem {
        static short glowmask;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Advanced Imaging");
            Tooltip.SetDefault("Increased projectile speed\nImmunity to Confusion\n\"The future is now\"");
            glowmask = Origins.AddGlowMask(this);
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaultsKeepSlots(ItemID.YoYoGlove);
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Yellow;
            Item.glowMask = glowmask;
        }
        public override void UpdateEquip(Player player) {
            player.GetModPlayer<OriginPlayer>().advancedImaging = true;
            player.buffImmune[BuffID.Confused] = true;
        }
    }
}
