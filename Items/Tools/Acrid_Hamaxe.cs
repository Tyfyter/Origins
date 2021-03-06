using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Tools {
	public class Acrid_Hamaxe : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Acrid Hamaxe");
			Tooltip.SetDefault("");
		}
		public override void SetDefaults() {
            item.CloneDefaults(ItemID.TitaniumWaraxe);
			item.damage = 31;
			item.melee = true;
            item.pick = 0;
            item.hammer = 65;
            item.axe = 22;
			item.width = 34;
			item.height = 34;
			item.useTime = 7;
			item.useAnimation = 22;
			item.knockBack = 4f;
			item.value = 3600;
			item.rare = ItemRarityID.LightRed;
			item.UseSound = SoundID.Item1;
		}
        public override float UseTimeMultiplier(Player player) {
            return player.wet?1.5f:1;
        }
	}
}
