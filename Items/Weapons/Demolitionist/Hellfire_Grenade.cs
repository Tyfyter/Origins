using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
    public class Hellfire_Grenade : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Hellfire Grenade");
			Tooltip.SetDefault("Explodes into a lingering fire");
			SacrificeTotal = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Grenade);
			Item.damage = 67;
			Item.shoot = ModContent.ProjectileType<Hellfire_Grenade_P>();
			Item.value *= 9;
			Item.rare = ItemRarityID.Orange;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 40);
			recipe.AddIngredient(ItemID.Grenade, 40);
			recipe.AddIngredient(ItemID.Fireblossom, 5);
			recipe.AddIngredient(ItemID.Hellstone, 2);
			recipe.Register();
		}
	}
	public class Hellfire_Grenade_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Hellfire_Grenade";
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Hellfire Grenade");
			Origins.MagicTripwireRange[Type] = 32;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Grenade);
			Projectile.penetrate = 1;
			Projectile.timeLeft = 135;
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
			int center = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<Ace_Shrapnel_P>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
			Vector2 v;
			for (int i = 4; i-- > 0;) {
				v = Main.rand.NextVector2Unit() * 6;
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + v * 8, v, ModContent.ProjectileType<Ace_Shrapnel_Shard>(), Projectile.damage / 2, Projectile.knockBack / 4, Projectile.owner, center, 4);
			}
		}
	}
}
