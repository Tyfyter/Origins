using Microsoft.Xna.Framework;
using Origins.Buffs;
using Origins.Items.Materials;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ammo {
	public class Bile_Bullet : ModItem {
        public string[] Categories => new string[] {
            "Bullet",
            "RasterSource"
        };
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.CursedBullet);
			Item.maxStack = 999;
			Item.damage = 11;
			Item.shoot = ModContent.ProjectileType<Bile_Bullet_P>();
			Item.shootSpeed = 5f;
			Item.knockBack = 4f;
			Item.value = Item.sellPrice(silver: 9, copper: 1);
			Item.rare = ItemRarityID.Orange;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 150);
			recipe.AddIngredient(ItemID.EmptyBullet, 150);
			recipe.AddIngredient(ModContent.ItemType<Black_Bile>());
			recipe.Register();
		}
	}
	public class Bile_Bullet_P : ModProjectile {
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.CursedBullet);
			Projectile.aiStyle = 0;
		}
		public override void AI() {
            Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Asphalt, 0, 0, 65, new Color(30, 0, 30), 0.75f);
            Projectile.rotation = Projectile.velocity.ToRotation();
			if (Projectile.alpha > 0)
				Projectile.alpha -= 15;
			if (Projectile.alpha < 0)
				Projectile.alpha = 0;
		}
		public override Color? GetAlpha(Color lightColor) {
			if (Projectile.alpha < 200) {
				return new Color(255 - Projectile.alpha, 185 - Projectile.alpha, 185 - Projectile.alpha, 185);
			}
			return Color.Transparent;
		}
		public override void OnKill(int timeLeft) {
			Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
			SoundEngine.PlaySound(SoundID.NPCHit22.WithVolume(0.5f), Projectile.position);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(ModContent.BuffType<Rasterized_Debuff>(), 20);
		}
	}
}
