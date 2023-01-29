using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class Turbo_Reel : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Turbo Reel");
            Tooltip.SetDefault("Increased return speed for harpoon guns");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Green;
        }
		public override void UpdateEquip(Player player) {
            player.GetModPlayer<OriginPlayer>().turboReel = true;
        }
    }
}
