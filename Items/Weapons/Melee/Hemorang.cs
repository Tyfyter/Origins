using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Melee {
	public class Hemorang : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Boomerang
		];
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ThornChakram);
			Item.DamageType = DamageClass.MeleeNoSpeed;
			Item.damage = 25;
			Item.width = 34;
			Item.height = 34;
			Item.useTime = 14;
			Item.useAnimation = 14;
			Item.shoot = ModContent.ProjectileType<Hemorang_P>();
			Item.shootSpeed = 9.75f;
			Item.knockBack = 5f;
			Item.value = Item.sellPrice(silver: 50);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.CrimtaneBar, 9)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override bool CanUseItem(Player player) {
			return player.ownedProjectileCounts[Item.shoot] < 1;
		}
	}
	public class Hemorang_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Melee/Hemorang";
		
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ThornChakram);
			Projectile.DamageType = DamageClass.MeleeNoSpeed;
			Projectile.penetrate = -1;
			Projectile.width = 34;
			Projectile.height = 34;
			AIType = ProjectileID.FruitcakeChakram;
		}
		public override bool PreAI() {
			Projectile.aiStyle = 3;
			return true;
		}
		public override void AI() {
			if (++Projectile.ai[2] > 8) {
				Projectile.ai[2] = 0;
				if (Projectile.owner == Main.myPlayer) Projectile.NewProjectile(
					Projectile.GetSource_FromAI(),
					Projectile.Center,
					Main.rand.NextVector2CircularEdge(2, 2),
					ModContent.ProjectileType<Hemorang_Blood_P>(),
					Projectile.damage / 2,
					Projectile.knockBack / 3,
					Projectile.owner
				);
			}
		}
		public override bool? CanHitNPC(NPC target) {
			Projectile.aiStyle = 0;
			return null;
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			width = 27;
			height = 27;
			return true;
		}
	}
	public class Hemorang_Blood_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Melee/Hemorang";
		
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ThornChakram);
			Projectile.DamageType = DamageClass.MeleeNoSpeed;
			Projectile.aiStyle = 1;
			Projectile.penetrate = 1;
			Projectile.extraUpdates = 1;
			Projectile.width = 6;
			Projectile.height = 6;
			Projectile.ignoreWater = false;
			Projectile.hide = true;
		}
		public override void AI() {
			if (Projectile.numUpdates == -1) Dust.NewDustDirect(Projectile.position, 0, 0, DustID.Blood).velocity *= 0.25f;
		}
		public override void OnKill(int timeLeft) {
			for (int i = 0; i < 2; i++) {
				Dust.NewDustDirect(Projectile.position, 0, 0, DustID.Blood);
			}
		}
	}
}
