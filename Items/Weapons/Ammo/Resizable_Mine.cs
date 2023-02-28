using Origins.Projectiles;
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
	public class Resizable_Mine_P : ModProjectile, IIsExplodingProjectile {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Resizable Mine");
			Origins.MagicTripwireRange[Type] = 40;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ProximityMineI);
			Projectile.timeLeft = 420;
			Projectile.scale = 0.5f;
			Projectile.penetrate = 1;
		}
		public override bool PreKill(int timeLeft) {
			Projectile.penetrate = -1;
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 96;
			Projectile.height = 96;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.Damage();
			ExplosiveGlobalProjectile.ExplosionVisual(Projectile, true, sound:SoundID.Item62);
			return false;
		}
		public bool IsExploding() => Projectile.penetrate == -1;
	}
}
