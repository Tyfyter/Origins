using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class Nitro_Crate : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Nitro Crate");
            Tooltip.SetDefault("Increases explosive blast radius by 25%");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.YoYoGlove);
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Green;
        }
        public override void UpdateEquip(Player player) {
            player.GetModPlayer<OriginPlayer>().explosiveBlastRadius += 0.25f;
        }
    }
}
