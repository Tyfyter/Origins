using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class Extremophile_Charm : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Extremophile Charm");
            Tooltip.SetDefault("Grants immunity to 'On Fire!', 'Acid Venom' and 'Toxic Shock'\nReduces damage recieved from debuffs by 30%");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.Aglet);
            Item.defense = 4;
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.LightRed;
        }
        public override void UpdateEquip(Player player) {
            //player.GetModPlayer<OriginPlayer>().extremeCharm = true;
        }
    }
}
