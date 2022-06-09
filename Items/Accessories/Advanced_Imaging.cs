using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Advanced_Imaging : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Advanced Imaging");
            Tooltip.SetDefault("Increased projectile speed\nImmunity to Confusion\n\"The future is now.\"");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.YoYoGlove);
            Item.handOffSlot = -1;
            Item.handOnSlot = -1;
        }
        public override void UpdateEquip(Player player) {
            player.GetModPlayer<OriginPlayer>().advancedImaging = true;
            player.buffImmune[BuffID.Confused] = true;
        }
    }
}
