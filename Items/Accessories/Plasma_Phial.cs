using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Balloon)]
	public class Plasma_Phial : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Plasma Phial");
            Tooltip.SetDefault("Debuff durations are halved\nSlightly increased regeneration\nRapidly lose life when inflicted with Bleeding");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaultsKeepSlots(ItemID.Aglet);
            Item.value = Item.sellPrice(silver:60);
            Item.rare = ItemRarityID.Blue;
        }
        public override void UpdateEquip(Player player) {
			player.lifeRegen += 1;
			player.GetModPlayer<OriginPlayer>().plasmaPhial = true;
        }
    }
}
