using Microsoft.Xna.Framework;
using Origins.Buffs;
using Origins.Items.Materials;
using Origins.Dev;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Newtonsoft.Json.Linq;
using Origins.UI;
using Terraria.Localization;
namespace Origins.Items.Weapons.Ammo {
	public class Bile_Bullet : ModItem, ICustomWikiStat {
        public string[] Categories => [
            WikiCategories.Bullet,
            WikiCategories.RasterSource
        ];
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.CursedBullet);
			Item.damage = 11;
			Item.shoot = ModContent.ProjectileType<Bile_Bullet_P>();
			Item.shootSpeed = 5f;
			Item.knockBack = 4f;
			Item.value = Item.sellPrice(copper: 8);
			Item.rare = ItemRarityID.Orange;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 150)
			.AddIngredient(ItemID.MusketBall, 150)
			.AddIngredient(ModContent.ItemType<Black_Bile>())
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
		public void ModifyWikiStats(JObject data) {
			data.Add("Debuffs", new JArray() {
				new JObject() {
					["src"] = "Rasterized",// wiki stat file name for the buff
					["Duration"] = Time_Radices.BuffTime.FormatTime(15)
				}
			});
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
			Projectile.BulletShimmer();
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
			target.AddBuff(ModContent.BuffType<Rasterized_Debuff>(), 15);
		}
	}
}
