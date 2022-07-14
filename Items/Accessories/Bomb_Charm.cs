using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Bomb_Charm : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Bomb Charm");
            Tooltip.SetDefault("Reduces explosive self-damage by 20%");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
        public override void SetDefaults() {
            Item.accessory = true;
            Item.rare = ItemRarityID.Blue;
        }
        public override void UpdateEquip(Player player) {
            player.GetModPlayer<OriginPlayer>().explosiveSelfDamage-=0.2f;
        }
    }
}
