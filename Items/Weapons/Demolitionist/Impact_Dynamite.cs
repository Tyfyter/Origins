using Origins.Dev;
using Origins.Tiles.Brine;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Demolitionist {
	public class Impact_Dynamite : ModItem {
        public override void SetStaticDefaults() {
			ItemID.Sets.ItemsThatCountAsBombsForDemolitionistToSpawn[Type] = true;
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Dynamite);
			Item.damage = 100;
			/*Item.useTime = (int)(Item.useTime * 0.75);
			Item.useAnimation = (int)(Item.useAnimation * 0.75);*/
			Item.shoot = ModContent.ProjectileType<Impact_Dynamite_P>();
			Item.shootSpeed *= 1.75f;
			Item.value = Item.sellPrice(silver: 8);
			Item.rare = ItemRarityID.Green;
            //Item.ArmorPenetration += 3;
        }
		public override void AddRecipes() {
			Recipe.Create(Type, 3)
			.AddIngredient(ItemID.Dynamite, 3)
			.AddIngredient(ModContent.ItemType<Peat_Moss_Item>())
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	public class Impact_Dynamite_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Impact_Dynamite";
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 32;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Dynamite);
			Projectile.penetrate = 1;
			Projectile.timeLeft = 225;
			Projectile.appliesImmunityTimeOnSingleHits = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}
		public override void AI() {
			if (Projectile.timeLeft < 150 && Main.rand.Next(0, Projectile.timeLeft) <= 1) Projectile.Kill();
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Projectile.Kill();
			return false;
		}
		public override bool PreKill(int timeLeft) {
			Projectile.type = ProjectileID.Dynamite;
			return true;
		}
		public override void OnKill(int timeLeft) {
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 200;
			Projectile.height = 200;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.Damage();
		}
	}
}
