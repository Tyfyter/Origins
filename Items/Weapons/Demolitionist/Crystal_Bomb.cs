using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Microsoft.Xna.Framework.MathHelper;
using static Origins.OriginExtensions;

namespace Origins.Items.Weapons.Demolitionist {
    public class Crystal_Bomb : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Crystal Bomb");
			Tooltip.SetDefault("Explodes into several crystal shards");
			SacrificeTotal = 99;

		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Bomb);
			Item.damage = 75;
			Item.value *= 14;
			Item.shoot = ModContent.ProjectileType<Crystal_Bomb_P>();
			Item.shootSpeed *= 1.5f;
			Item.knockBack = 5f;
			Item.ammo = ItemID.Grenade;
			Item.value = Item.sellPrice(silver: 5);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 5);
			recipe.AddIngredient(ItemID.CrystalShard);
			recipe.AddIngredient(ItemID.Grenade, 5);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
	}
	public class Crystal_Bomb_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Crystal_Bomb";
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Crystal Bomb");
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bomb);
			Projectile.timeLeft = 135;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}
		public override bool PreKill(int timeLeft) {
			Projectile.type = ProjectileID.Bomb;
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
			int t = ModContent.ProjectileType<Crystal_Grenade_Shard>();
			int count = 14 - Main.rand.Next(3);
			float rot = TwoPi / count;
			for (int i = count; i > 0; i--) {
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, (Vec2FromPolar(rot * i, 6) + Main.rand.NextVector2Unit()) + (Projectile.velocity / 12), t, Projectile.damage / 4, 6, Projectile.owner);
			}
		}
	}
}