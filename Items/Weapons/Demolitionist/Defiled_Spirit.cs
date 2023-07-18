using Origins.Items.Accessories;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
    public class Defiled_Spirit : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Defiled Spirit");
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Snowball);
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Throwing];
			Item.damage = 13;
			Item.useTime = 28;
			Item.useAnimation = 28;
			Item.shoot = ModContent.ProjectileType<Defiled_Spirit_P>();
			Item.shootSpeed = 17;
			Item.knockBack -= 3;
			Item.value = Item.sellPrice(copper: 80);
			Item.rare = ItemRarityID.Blue;
		}
	}
	public class Defiled_Spirit_P : ModProjectile {
		public override string Texture => "Origins/Projectiles/Weapons/Defiled_Spirit_P";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Defiled Spirit");
			Main.projFrames[Type] = 3;
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.SnowBallFriendly);
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Throwing];
			Projectile.penetrate = 1;
		}
		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation();
			if (++Projectile.frameCounter > 3) {
				Projectile.frame = (Projectile.frame + 1) % 3;
				Projectile.frameCounter = 0;
			}
			if (Main.rand.NextBool(3)) Dust.NewDust(Projectile.Center, Projectile.width, Projectile.height, DustID.Asphalt);
		}
		public override void Kill(int timeLeft) {
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 48;
			Projectile.height = 48;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			int t = ModContent.ProjectileType<Return_To_Sender_Thorns>();
			Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, (Main.rand.NextVector2Unit() * 4) + (Projectile.velocity / 8), t, Projectile.damage / 8, 6, Projectile.owner, ai1: -0.5f).scale = 1f;
			Projectile.Damage();
			SoundEngine.PlaySound(SoundID.Item46.WithVolume(0.66f), Projectile.Center);
		}
	}
}
