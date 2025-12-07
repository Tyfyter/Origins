using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Magic {
	public class Startillery : ModItem, ICustomWikiStat {
        public string[] Categories => [
            "SpellBook"
        ];
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.GoldenShower);
			Item.damage = 48;
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Magic];
			Item.mana = 12;
			Item.noUseGraphic = false;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.knockBack = 6;
			Item.shoot = ModContent.ProjectileType<Starshot>();
			Item.shootSpeed = 16f;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.LightRed;
			Item.UseSound = Origins.Sounds.DeepBoom.WithPitchRange(1f, 1.3f);
		}
	}
	public class Starshot : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_288";
		
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ExplosiveBullet);
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Magic];
			Projectile.friendly = true;
			Projectile.timeLeft = 180;
			Projectile.aiStyle = 2;
			Projectile.hide = false;
			Projectile.alpha = 0;
		}
		public override void AI() {
			Lighting.AddLight(Projectile.Center, 0f, 0.2f, 0.85f);
			Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.MagicMirror);
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Projectile.Kill();
			return false;
		}
		public override bool PreKill(int timeLeft) {
			Projectile.type = ProjectileID.StarCannonStar;
			return true;
		}
		public override void OnKill(int timeLeft) {
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 64;
			Projectile.height = 128;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.Damage();
		}
	}
}
