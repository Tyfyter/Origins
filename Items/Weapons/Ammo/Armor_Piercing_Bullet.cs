using Origins.Dev;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ammo {
	public class Armor_Piercing_Bullet : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Bullet
		];
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.CursedBullet);
			Item.damage = 6;
			Item.shoot = ModContent.ProjectileType<Armor_Piercing_Bullet_P>();
			Item.shootSpeed = 5f;
			Item.knockBack = 4f;
			Item.value = Item.sellPrice(copper: 15);
			Item.rare = ItemRarityID.Orange;
			Item.ArmorPenetration = 15;
		}
	}
	public class Armor_Piercing_Bullet_P : ModProjectile {
		public override string Texture => "Origins/Projectiles/Ammo/Generic_Bullet";
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.width = 4;
			Projectile.height = 4;
			Projectile.friendly = true;
			Projectile.penetrate = 2;
			Projectile.light = 0.5f;
			Projectile.alpha = 255;
			Projectile.scale = 1.2f;
			Projectile.timeLeft = 600;
			Projectile.extraUpdates = 2;
			Projectile.aiStyle = 0;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}
		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation();
			if (Projectile.alpha > 0)
				Projectile.alpha -= 15;
			if (Projectile.alpha < 0)
				Projectile.alpha = 0;
			Projectile.BulletShimmer();
		}
		public override Color? GetAlpha(Color lightColor) {
			if (Projectile.alpha < 200) {
				return new Color(255, 128, 0) * Projectile.Opacity;
			}
			return Color.Transparent;
		}
		public override void OnKill(int timeLeft) {
			Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
		}
	}
}
