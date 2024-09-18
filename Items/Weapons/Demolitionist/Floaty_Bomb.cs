using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
namespace Origins.Items.Weapons.Demolitionist {
    public class Floaty_Bomb : ModItem, ICustomWikiStat {
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
			Item.damage = 70;
			Item.shoot = ModContent.ProjectileType<Floaty_Bomb_P>();
			Item.shootSpeed *= 1.4f;
			Item.value *= 6;
			Item.rare = ItemRarityID.Green;
            Item.ArmorPenetration += 2;
        }
		public override void AddRecipes() {
			Recipe.Create(Type, 35)
			.AddIngredient(ItemID.Bomb, 35)
			.AddIngredient(ItemID.Bone, 35)
			.AddIngredient(ItemID.Cloud)
			.AddIngredient(ItemID.Feather)
            .AddTile(TileID.Anvils)
            .Register();
		}
	}
	public class Floaty_Bomb_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Floaty_Bomb";
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 32;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bomb);
			Projectile.penetrate = 1;
			Projectile.timeLeft = 135;
		}
		public override void AI() {
			Projectile.velocity.Y -= 0.1f;
		}
		public override bool PreKill(int timeLeft) {
			Projectile.type = ProjectileID.Bomb;
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
		}
	}
}
