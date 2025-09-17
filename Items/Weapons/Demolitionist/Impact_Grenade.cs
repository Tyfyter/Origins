using Origins.Dev;
using Origins.Tiles.Brine;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Demolitionist {
	public class Impact_Grenade : ModItem, ICustomWikiStat {
        public string[] Categories => [
            "ThrownExplosive",
			"IsGrenade",
            "ExpendableWeapon"
        ];
        public override void SetStaticDefaults() {
			ItemID.Sets.ItemsThatCountAsBombsForDemolitionistToSpawn[Type] = true;
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Grenade);
			Item.damage = 43;
			/*Item.useTime = (int)(Item.useTime * 0.75);
			Item.useAnimation = (int)(Item.useAnimation * 0.75);*/
			Item.shoot = ModContent.ProjectileType<Impact_Grenade_P>();
			Item.shootSpeed *= 1.75f;
			Item.ammo = ItemID.Grenade;
			Item.value = Item.sellPrice(copper: 35);
			Item.rare = ItemRarityID.Green;
            //Item.ArmorPenetration += 3;
        }
		public override void AddRecipes() {
			Recipe.Create(Type, 8)
			.AddIngredient(ItemID.Grenade, 8)
			.AddIngredient(ModContent.ItemType<Peat_Moss_Item>())
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	public class Impact_Grenade_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Impact_Grenade";
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 32;
			Projectile.appliesImmunityTimeOnSingleHits = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Grenade);
			Projectile.timeLeft = 135;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Projectile.Kill();
			return false;
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
		}
	}
}
