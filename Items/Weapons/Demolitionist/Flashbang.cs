using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
    public class Flashbang : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Flashbang");
			Tooltip.SetDefault("Dazes enemies upon detonation");
			SacrificeTotal = 99;

		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Grenade);
			Item.damage = 30;
			Item.knockBack = 0;
			Item.crit += 16;
			Item.shootSpeed *= 2;
			Item.shoot = ModContent.ProjectileType<Flashbang_P>();
			Item.ammo = ItemID.Grenade;
			Item.value = Item.sellPrice(copper: 15);
			Item.maxStack = 999;
		}
	}
	public class Flashbang_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Flashbang";
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Flashbang");
			Origins.MagicTripwireRange[Type] = 32;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Grenade);
			Projectile.timeLeft = 135;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}
		public override bool PreKill(int timeLeft) {
			Projectile.type = ProjectileID.Grenade;
			return true;
		}
		public override void Kill(int timeLeft) {
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 128;
			Projectile.height = 128;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.Damage();
		}
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			target.AddBuff(BuffID.Confused, 145);
			target.AddBuff(BuffID.Slow, 300);
		}
	}
}
