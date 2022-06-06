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
            Item.CloneDefaults(ItemID.TitaniumWaraxe);
			Item.damage = 31;
			Item.DamageType = DamageClass.Melee;
            Item.pick = 0;
            Item.hammer = 65;
            Item.axe = 22;
			Item.width = 34;
			Item.height = 34;
			Item.useTime = 7;
			Item.useAnimation = 22;
			Item.knockBack = 4f;
			Item.value = 3600;
			Item.rare = ItemRarityID.LightRed;
			Item.UseSound = SoundID.Item1;
		}
        public override float UseTimeMultiplier(Player player) {
            return player.wet?1.5f:1;
        }
	}
}
