using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class Spirit_Shard : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Spirit Shard");
            Tooltip.SetDefault("Artifact minions turn into ghosts of their former selves upon death");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.Aglet);
            Item.rare = ItemRarityID.Green;
        }
        public override void UpdateEquip(Player player) {
            //player.GetModPlayer<OriginPlayer>().spiritShard = true;
        }
    }
}
