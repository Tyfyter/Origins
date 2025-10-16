using Microsoft.Xna.Framework;
using Origins.Dev;
using PegasusLib;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Melee {
	public class Dark_Spiral : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Boomerang
		];
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ThornChakram);
			Item.DamageType = DamageClass.MeleeNoSpeed;
			Item.damage = 30;
			Item.width = 34;
			Item.height = 34;
			Item.useTime = 17;
			Item.useAnimation = 17;
			Item.shoot = ModContent.ProjectileType<Dark_Spiral_Thrown>();
			Item.shootSpeed = 11.75f;
			Item.knockBack = 4f;
			Item.value = Item.sellPrice(silver: 50);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.DemoniteBar, 9)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override bool CanUseItem(Player player) {
			return player.ownedProjectileCounts[Item.shoot] < 1;
		}
	}
	public class Dark_Spiral_Thrown : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Melee/Dark_Spiral";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ThornChakram);
			Projectile.DamageType = DamageClass.MeleeNoSpeed;
			Projectile.penetrate = -1;
			Projectile.width = 34;
			Projectile.height = 34;
			AIType = ProjectileID.FruitcakeChakram;
		}
		public override void AI() {
			if (++Projectile.ai[2] > 12) {
				Main.player[Projectile.owner].DoHoming((target) => {
					if (Projectile.ai[2] == 0) return false;
					if (Projectile.Center.Clamp(target.Hitbox).IsWithin(Projectile.Center, 5 * 16)) {
						Projectile.ai[2] = 0;
						Projectile.NewProjectile(
							Projectile.GetSource_FromAI(),
							Projectile.Center,
							Projectile.Center.DirectionTo(target.Center) * 8,
							ModContent.ProjectileType<Dark_Spiral_P>(),
							Projectile.damage / 2,
							Projectile.knockBack,
							Projectile.owner
						);
						return true;
					}
					return false;
				});
			}
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			width = 27;
			height = 27;
			return true;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (Projectile.velocity.X != oldVelocity.X) {
				Projectile.velocity.X = -oldVelocity.X;
			}
			if (Projectile.velocity.Y != oldVelocity.Y) {
				Projectile.velocity.Y = -oldVelocity.Y;
			}
			SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
			return false;
		}
	}
	public class Dark_Spiral_P : ModProjectile {
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ThornChakram);
			Projectile.DamageType = DamageClass.MeleeNoSpeed;
			Projectile.aiStyle = 1;
			Projectile.penetrate = 1;
			Projectile.extraUpdates = 0;
			Projectile.width = 6;
			Projectile.height = 6;
			Projectile.ignoreWater = false;
			Projectile.hide = false;
		}
		public override void PostAI() {
			base.PostAI();
		}
		public override void OnKill(int timeLeft) {
			Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
			for (int i = 0; i < 2; i++) {
				Dust.NewDustDirect(Projectile.position, 0, 0, DustID.Demonite);
			}
		}
	}
}
