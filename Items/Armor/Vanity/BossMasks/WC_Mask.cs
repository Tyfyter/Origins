using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Vanity.BossMasks {
	[AutoloadEquip(EquipType.Head)]
    public class WC_Mask : ModItem {
        public override LocalizedText Tooltip => LocalizedText.Empty;
        public override void SetStaticDefaults() {
            if (Main.netMode != NetmodeID.Server) {
                Origins.AddHelmetGlowmask(Item.headSlot, "Items/Armor/Vanity/BossMasks/WC_Mask_Head_Glow");
            }
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults() {
            Item.rare = ItemRarityID.Blue;
            Item.vanity = true;
        }
    }
}
