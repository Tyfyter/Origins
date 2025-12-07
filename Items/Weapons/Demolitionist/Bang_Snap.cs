using Origins.Tiles.Defiled;
using Origins.Tiles.Riven;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.Dev;
namespace Origins.Items.Weapons.Demolitionist {
	public class Bang_Snap : ModItem {
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Snowball);
			Item.damage = 10;
			Item.crit = 0;
			Item.DamageType = DamageClasses.ThrownExplosive;
			Item.shoot = ModContent.ProjectileType<Bang_Snap_P>();
			Item.shootSpeed = 11;
            Item.knockBack = 0;
			Item.value = Item.sellPrice(copper: 1);
            Item.ArmorPenetration += 12;
        }
		public override void AddRecipes() {
			Recipe.Create(Type, 60)
			.AddIngredient(ItemID.SilverOre)
            .AddIngredient(ItemID.SandBlock)
            .Register();

            Recipe.Create(Type, 60)
            .AddIngredient(ItemID.SilverOre)
            .AddIngredient(ItemID.EbonsandBlock)
            .Register();

            Recipe.Create(Type, 60)
            .AddIngredient(ItemID.SilverOre)
            .AddIngredient(ItemID.CrimsandBlock)
            .Register();

			Recipe.Create(Type, 60)
			.AddIngredient(ItemID.SilverOre)
			.AddIngredient(ItemID.PearlsandBlock)
			.Register();

			Recipe.Create(Type, 60)
            .AddIngredient(ItemID.SilverOre)
            .AddIngredient(ModContent.ItemType<Defiled_Sand_Item>())
            .Register();

            Recipe.Create(Type, 60)
            .AddIngredient(ItemID.SilverOre)
            .AddIngredient(ModContent.ItemType<Silica_Item>())
            .Register();

            /*Recipe.Create(Type, 60)
            .AddIngredient(ItemID.SilverOre)
            .AddIngredient(ModContent.ItemType<Ashen_Sand_Item>())
            .Register();*/
        }
	}
	public class Bang_Snap_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Bang_Snap_P";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.SnowBallFriendly);
			Projectile.DamageType = DamageClasses.ThrownExplosive;
			Projectile.penetrate = 1;
			Projectile.appliesImmunityTimeOnSingleHits = true;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 10;
		}
		public override void OnKill(int timeLeft) {
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 32;
			Projectile.height = 32;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.Damage();
			SoundEngine.PlaySound(SoundID.Item40.WithPitch(1f).WithVolume(1f), Projectile.Center);
		}
	}
}
