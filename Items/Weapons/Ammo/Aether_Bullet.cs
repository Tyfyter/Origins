using Origins.Items.Materials;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ammo {
	public class Aether_Bullet : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.CursedBullet);
			Item.damage = 11;
			Item.shoot = ModContent.ProjectileType<Aether_Bullet_P>();
			Item.shootSpeed = 5f;
			Item.knockBack = 4f;
			Item.value = Item.sellPrice(copper: 14);
			Item.rare = ItemRarityID.Green;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 90)
			.AddIngredient(ItemID.MusketBall, 90)
			.AddIngredient(ModContent.ItemType<Aetherite_Bar>())
			.Register();
		}
	}
	public class Aether_Bullet_P : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.VenomBullet;
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.width = 4;
			Projectile.height = 4;
			Projectile.friendly = true;
			Projectile.penetrate = 1;
			Projectile.light = 0.5f;
			Projectile.alpha = 255;
			Projectile.scale = 1.2f;
			Projectile.timeLeft = 600;
			Projectile.extraUpdates = 2;
			Projectile.aiStyle = 0;
		}
		public override void OnSpawn(IEntitySource source) {
			Projectile.NewProjectile(
				source,
				Projectile.Center,
				-Projectile.velocity,
				ModContent.ProjectileType<Aether_Bullet_Duplicate_P>(),
				Projectile.damage,
				Projectile.knockBack,
				Projectile.owner
			);
		}
		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			if (Projectile.alpha > 0)
				Projectile.alpha -= 15;
			if (Projectile.alpha < 0)
				Projectile.alpha = 0;
			Projectile.BulletShimmer();
		}
		public override Color? GetAlpha(Color lightColor) {
			if (Projectile.alpha < 200) {
				return new Color(255 - Projectile.alpha, 255 - Projectile.alpha, 255 - Projectile.alpha, 0);
			}
			return Color.Transparent;
		}
		public override void OnKill(int timeLeft) {
			Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
		}
	}
	public class Aether_Bullet_Duplicate_P : Aether_Bullet_P {
		public override void OnSpawn(IEntitySource source) { }
	}
}
