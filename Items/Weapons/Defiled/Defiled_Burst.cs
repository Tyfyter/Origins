using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Defiled {
	public class Defiled_Burst : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("?");
			Tooltip.SetDefault("Very pointy");
		}
		public override void SetDefaults() {
			item.damage = 17;
			item.ranged = true;
            item.noMelee = true;
			item.width = 56;
			item.height = 18;
			item.useTime = 43;
			item.useAnimation = 43;
			item.useStyle = 5;
			item.knockBack = 5;
            item.shootSpeed = 4.75f;
            item.shoot = ProjectileID.Bullet;
            item.useAmmo = AmmoID.Bullet;
			item.value = 5000;
            item.useTurn = true;
			item.rare = ItemRarityID.Blue;
			item.UseSound = SoundID.Item1;
		}
	}
}
