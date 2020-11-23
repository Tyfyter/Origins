using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Defiled {
	public class Spiker_Sword : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Spiker Sword");
			Tooltip.SetDefault("Very pointy");
		}
		public override void SetDefaults() {
			item.damage = 30;
			item.melee = true;
			item.width = 42;
			item.height = 50;
			item.useTime = 28;
			item.useAnimation = 28;
			item.useStyle = 1;
			item.knockBack = 7.5f;
			item.value = 5000;
            item.useTurn = true;
			item.rare = ItemRarityID.Blue;
			item.UseSound = SoundID.Item1;
		}
	}
}
