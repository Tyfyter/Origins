using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
    public class Defiled_Spirit : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Defiled Spirit");
			Tooltip.SetDefault("'Throw it like you wanna'");
			SacrificeTotal = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Snowball);
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Throwing];
			Item.damage = 28;
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
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.SnowBallFriendly);
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Throwing];
			Projectile.penetrate = 1;
		}
		public override void Kill(int timeLeft) {
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 48;
			Projectile.height = 48;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.Damage();
			SoundEngine.PlaySound(SoundID.Item46.WithVolume(0.66f), Projectile.Center);
			Gore.NewGoreDirect(Projectile.GetSource_FromThis(), new Vector2(Projectile.Center.X, Projectile.Center.Y), default, Main.rand.Next(61, 64)).velocity += Vector2.One;
			Gore.NewGoreDirect(Projectile.GetSource_FromThis(), new Vector2(Projectile.Center.X, Projectile.Center.Y), default, Main.rand.Next(61, 64)).velocity += Vector2.One;
		}
	} //Probably could use RTS' retaliation effect for explosion
}
