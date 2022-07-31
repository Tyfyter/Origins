using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class Queen_of_Hearts : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Queen of Hearts");
            Tooltip.SetDefault("+2 minion slots\nIncreases max health by 30 for every active minion\nExpert");
            SacrificeTotal = 1;
            Main.RegisterItemAnimation(Type, new DrawAnimationVertical(8, 5, true));//uses "ping-pong" animation to reduce file size
        }
        public override void SetDefaults() {
            Item.accessory = true;
            Item.width = 16;
            Item.height = 18;
            Item.rare = ItemRarityID.Expert;
            Item.expert = true;
        }
        public override void UpdateEquip(Player player) {
            player.maxMinions += 2;
            player.statLifeMax2 += 30 * player.numMinions;
        }
    }
}
