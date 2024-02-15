using Microsoft.Xna.Framework;
using Origins.Buffs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
    public class Outbreak_Bomb : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Bomb);
			Item.damage = 74;
			Item.shoot = ModContent.ProjectileType<Outbreak_Bomb_P>();
			Item.value *= 2;
			Item.rare = ItemRarityID.Blue;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 15);
			recipe.AddIngredient(ItemID.Bomb, 15);
			recipe.AddIngredient(ItemID.DemoniteOre);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
		}
	}
	public class Outbreak_Bomb_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Outbreak_Bomb";
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 32;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bomb);
			Projectile.penetrate = 1;
			Projectile.timeLeft = 135;
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
			int center = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<Impeding_Shrapnel_Shard>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
			Vector2 v;
			for (int i = 4; i-- > 0;) {
				v = Main.rand.NextVector2Unit() * 6;
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + v * 8, v, ModContent.ProjectileType<Impeding_Shrapnel_Shard>(), Projectile.damage / 2, Projectile.knockBack / 4, Projectile.owner, center, 4);
			}
		}
	}
}
