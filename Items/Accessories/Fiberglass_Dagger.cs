using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class Fiberglass_Dagger : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Fiberglass Dagger");
            Tooltip.SetDefault("Increases weapon damage by 4, but reduces defense by 4");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.WormScarf);
            Item.neckSlot = -1;
            Item.rare = ItemRarityID.Expert;
        }
        public override void UpdateEquip(Player player) {
            player.statDefense -= 4;
            player.GetDamage(DamageClass.Default).Flat += 4;
            player.GetDamage(DamageClass.Generic).Flat += 4;
            //player.GetModPlayer<OriginPlayer>().fiberglassDagger = true;
        }
    }
}
