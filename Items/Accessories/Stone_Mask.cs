using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    [AutoloadEquip(EquipType.Face)]
    public class Stone_Mask : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Stone Mask");
            Tooltip.SetDefault("Increases defense by 8, but your movement is hindered\nYou hear closeby whispers...");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.WormScarf);
            Item.faceSlot = -1;
            Item.rare = ItemRarityID.White;
        }
        public override void UpdateEquip(Player player) {
            player.statDefense += 8;
            player.moveSpeed = 0.9f;
            player.jumpSpeedBoost -= 1.8f;
        }
    }
}
