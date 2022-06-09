using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Blast_Resistant_Plate : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Blast Resistant Plate");
            Tooltip.SetDefault("Reduces explosive self-damage by 20%");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
        public override void SetDefaults() {
            Item.accessory = true;
        }
        public override void UpdateEquip(Player player) {
            player.GetModPlayer<OriginPlayer>().explosiveSelfDamage-=0.2f;
        }
    }
}
