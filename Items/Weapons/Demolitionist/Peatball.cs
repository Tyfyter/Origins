using Origins.Dev;
using Origins.Tiles.Brine;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Peatball : ModItem, ICustomWikiStat {
        public string[] Categories => [
            WikiCategories.ThrownExplosive,
            WikiCategories.ExpendableWeapon
        ];
        public override void SetStaticDefaults() {
            Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Snowball);
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Throwing];
			Item.damage = 16;
			Item.shoot = ModContent.ProjectileType<Peatball_P>();
			Item.shootSpeed = 8;
			Item.knockBack = 0;
			Item.value = Item.sellPrice(copper: 75);
			Item.rare = ItemRarityID.Green;
			Item.ArmorPenetration += 8;
		}
		public override void AddRecipes() {
			const int coalNumber = 4;

			Recipe.Create(Type, 4)
			.AddIngredient(ModContent.ItemType<Peat_Moss_Item>())
			.DisableDecraft()
			.Register();

            Recipe.Create(Type, coalNumber)
            .AddIngredient(ItemID.Coal)
            .AddCondition(RecipeConditions.ShimmerTransmutation)
            .AddDecraftCondition(Condition.Hardmode)
            .Register();

            Recipe.Create(ItemID.Coal)
            .AddIngredient(Type, coalNumber)
            .AddCondition(RecipeConditions.ShimmerTransmutation)
            .Register();
        }
	}
	public class Peatball_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Peatball";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.SnowBallFriendly);
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Throwing];
			Projectile.penetrate = 1;
			Projectile.width = 12;
			Projectile.height = 12;
			Projectile.scale = 0.85f;
			Projectile.appliesImmunityTimeOnSingleHits = true;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 10;
		}
		public override void OnKill(int timeLeft) {
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 48;
			Projectile.height = 48;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.Damage();
			SoundEngine.PlaySound(SoundID.Item14.WithVolume(0.66f), Projectile.Center);
			Gore.NewGoreDirect(Projectile.GetSource_FromThis(), new Vector2(Projectile.Center.X, Projectile.Center.Y), default, Main.rand.Next(61, 64)).velocity += Vector2.One;
			Gore.NewGoreDirect(Projectile.GetSource_FromThis(), new Vector2(Projectile.Center.X, Projectile.Center.Y), default, Main.rand.Next(61, 64)).velocity += Vector2.One;
		}
	}
}
