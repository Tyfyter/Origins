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
			Item.shoot = ModContent.ProjectileType<Resizable_Mine_P>();
			Item.ammo = Type;
			Item.shootSpeed = 4f;
            Item.knockBack = 3f;
			Item.value = Item.sellPrice(silver: 4, copper: 65);
			Item.rare = ItemRarityID.Pink;
		}
	}
	public class Resizable_Mine_P : ModProjectile {
		//public override string Texture => "Origins/Items/Weapons/Ammo/Resizable_Mine_P";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ProximityMineI);
			Projectile.timeLeft = 420;
			Projectile.scale = 0.5f;
			Projectile.penetrate = 1;
			Origins.MagicTripwireRange[Type] = 96;
			//AIType = ProjectileID.ProximityMineI;
		}
		public override bool PreKill(int timeLeft) {
			Projectile.type = ProjectileID.Grenade;
			return true;
		}
		public override void Kill(int timeLeft) {
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 96;
			Projectile.height = 96;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.Damage();
		}
	}
}
