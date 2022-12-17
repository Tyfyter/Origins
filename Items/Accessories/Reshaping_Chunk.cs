using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class Reshaping_Chunk : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Refactoring Pieces");
            Tooltip.SetDefault("Strengthens the set bonus of Defiled Armor\nReduces damage taken by 5% if Defiled Armor is not equipped");
			Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(11, 6));
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.accessory = true;
            Item.width = 30;
            Item.height = 30;
            Item.rare = ItemRarityID.Expert;
            Item.expert = true;
            Item.value = Item.buyPrice(gold: 2);
        }
        public override void UpdateEquip(Player player) {
            player.GetModPlayer<OriginPlayer>().reshapingChunk = true;
        }
    }
}
