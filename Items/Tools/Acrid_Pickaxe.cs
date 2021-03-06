using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Tools {
	public class Acrid_Pickaxe : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Acrid Pickaxe");
			Tooltip.SetDefault("");
		}
		public override void SetDefaults() {
            item.CloneDefaults(ItemID.TitaniumPickaxe);
			item.damage = 28;
			item.melee = true;
            item.pick = 195;
			item.width = 34;
			item.height = 32;
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
