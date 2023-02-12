using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ammo {
	public class Resizable_Mine : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Resizable Mine");
			Tooltip.SetDefault("'Compatible with your garden-variety mine launchers!'");
			SacrificeTotal = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Grenade);
			Item.maxStack = 999;
			Item.damage = 20;
			Item.shootSpeed = 4f;
            Item.knockBack = 3f;
			Item.value = Item.sellPrice(silver: 4, copper: 65);
			Item.rare = ItemRarityID.Pink;
		}
	}
	public class Resizable_Mine_P : ModProjectile {
        //public override string Texture => "Origins/Items/Weapons/Ammo/Resizable_Mine_P";
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.Landmine);
			Projectile.timeLeft = 420;
        }
    }
}
