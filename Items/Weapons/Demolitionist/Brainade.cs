using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
    public class Brainade : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Brainade");
			Tooltip.SetDefault("Explodes into a bloody mess");
			SacrificeTotal = 99;

		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Grenade);
			Item.damage = 50;
			Item.value *= 5;
			Item.shoot = ModContent.ProjectileType<Brainade_P>();
			Item.ammo = ItemID.Grenade;
			Item.rare = ItemRarityID.Blue;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 50);
			recipe.AddIngredient(ItemID.Grenade, 50);
			recipe.AddIngredient(ItemID.CrimtaneOre, 2);
			recipe.AddIngredient(ItemID.ViciousPowder, 25);
			recipe.Register();
		}
	}
	public class Brainade_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Brainade";
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Brainade");
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
	}
}
