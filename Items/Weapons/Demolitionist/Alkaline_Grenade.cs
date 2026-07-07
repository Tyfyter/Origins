using Origins.Dev;
using Origins.Items.Materials;
using Origins.Projectiles.Weapons;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Demolitionist {
	public class Alkaline_Grenade : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.ToxicSource
		];
		public override void SetStaticDefaults() {
			ItemID.Sets.ItemsThatCountAsBombsForDemolitionistToSpawn[Type] = true;
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Grenade);
			Item.damage = 68;
			Item.value = 500;
			Item.shootSpeed *= 1.5f;
			Item.shoot = ModContent.ProjectileType<Acid_Grenade_P>();
			Item.ammo = ItemID.Grenade;
			Item.rare = ItemRarityID.LightRed;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 2)
			.AddIngredient(ItemID.Grenade, 2)
			.AddIngredient(ModContent.ItemType<Alkaliphiliac_Tissue>())
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
	public class Acid_Grenade_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Alkaline_Grenade";
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 32;
			Hand_Grenade_Launcher.AltFireAction[Type] = (player, source, position, velocity, type, damage, knockback) => {
				position += velocity.SafeNormalize(Vector2.Zero) * 40;
				type = ModContent.ProjectileType<Brine_Droplet>();
				damage -= 20;
				for (int i = Main.rand.Next(2); ++i < 5;) {
					Projectile.NewProjectileDirect(source, position, velocity.RotatedByRandom(0.1 * i) * 0.6f, type, damage / 2, knockback, player.whoAmI).scale = 0.85f;
				}
			};
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Grenade);
			Projectile.timeLeft = 135;
			Projectile.penetrate = 1;
			Projectile.appliesImmunityTimeOnSingleHits = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
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
			//Main.PlaySound(2, (int)projectile.Center.X, (int)projectile.Center.Y, 122, 2f, 1f);
			int t = ModContent.ProjectileType<Brine_Droplet>();
			for (int i = Main.rand.Next(3); i < 5; i++) Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, (Main.rand.NextVector2Unit() * 4) + (Projectile.velocity / 6), t, Projectile.damage / 8, 6, Projectile.owner, ai1: -0.5f).scale = 0.85f;
		}
	}
}
