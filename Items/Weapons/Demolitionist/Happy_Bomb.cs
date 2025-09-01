using Origins.Dev;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Demolitionist {
	public class Happy_Bomb : ModItem, ICustomWikiStat {
        public string[] Categories => [
            "ThrownExplosive",
			"IsBomb",
            "ExpendableWeapon"
        ];
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Bomb);
			Item.damage = 100;
			Item.shootSpeed *= 1.5f;
			Item.shoot = ModContent.ProjectileType<Happy_Bomb_P>();
			Item.value *= 13;
			Item.rare = ItemRarityID.Pink;
		}
	}
	public class Happy_Bomb_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Happy_Bomb";
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 32;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bomb);
			Projectile.penetrate = 1;
			Projectile.timeLeft = 135;
			Projectile.friendly = true;
			Projectile.appliesImmunityTimeOnSingleHits = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}
		public override void AI() {
			if (Projectile.timeLeft < 60 && Main.rand.Next(0, Projectile.timeLeft) == 0) Projectile.Kill();
		}
		public override bool PreKill(int timeLeft) {
			Projectile.type = ProjectileID.Grenade;
			return true;
		}
		public override void OnKill(int timeLeft) {
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 128;
			Projectile.height = 128;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.Damage();
			SoundEngine.PlaySound(Main.rand.NextFromList(SoundID.Zombie121, SoundID.Zombie122, SoundID.Zombie123).WithPitchRange(1.1f, 1.25f).WithVolume(0.5f), Projectile.Center);
		}
	}
}
