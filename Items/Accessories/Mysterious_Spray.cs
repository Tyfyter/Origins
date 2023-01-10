using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class Mysterious_Spray : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Mysterious Spray");
            Tooltip.SetDefault("Increases life regeneration at low health\n'The perfect ailment to attack your senses'");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.accessory = true;
            Item.width = 21;
            Item.height = 20;
            Item.rare = ItemRarityID.Master;
            Item.master = true;
            Item.value = Item.buyPrice(gold: 30);
        }
		public override void UpdateAccessory(Player player, bool hideVisual) {
            int factor = (int)(30 / ((player.statLife / (float)player.statLifeMax2) * 3.5f + 0.5f));
            player.lifeRegen += factor;
			if (!hideVisual) {
                player.GetModPlayer<OriginPlayer>().mysteriousSprayMult *= player.statLife / (float)player.statLifeMax2;
            }
        }
		public override void UpdateVanity(Player player) {
            player.GetModPlayer<OriginPlayer>().mysteriousSprayMult *= player.statLife / (float)player.statLifeMax2;
        }
	}
}
