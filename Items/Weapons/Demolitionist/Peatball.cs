using Microsoft.Xna.Framework;
using Origins.Tiles.Brine;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
using Terraria.Localization;

namespace Origins.Items.Weapons.Demolitionist {
    public class Peatball : ModItem, ICustomWikiStat {
        public string[] Categories => [
            "ThrownExplosive",
            "ExpendableWeapon"
        ];
        public override void SetStaticDefaults() {
            Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Snowball);
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Throwing];
			Item.damage = 12;
			Item.useTime = (int)(Item.useTime * 0.75);
			Item.useAnimation = (int)(Item.useAnimation * 0.75);
			Item.shoot = ModContent.ProjectileType<Peatball_P>();
			Item.shootSpeed = 7;
			Item.knockBack *= 2;
			Item.value = Item.sellPrice(copper: 75);
			Item.rare = ItemRarityID.Blue;
		}
		public override void AddRecipes() {
			const int coalNumber = 4;

			Recipe recipe = Recipe.Create(Type, 4);
			recipe.AddIngredient(ModContent.ItemType<Peat_Moss_Item>());
			recipe.DisableDecraft();
			recipe.Register();

            Recipe.Create(Type)
            .AddIngredient(ItemID.Coal, coalNumber)
            .AddCondition(Language.GetOrRegister("Mods.Origins.Conditions.ShimmerDecrafting"), () => false)
            .AddDecraftCondition(Condition.Hardmode)
            .Register();

            Recipe.Create(ItemID.Coal, coalNumber)
            .AddIngredient(Type)
            .AddCondition(Language.GetOrRegister("Mods.Origins.Conditions.ShimmerDecrafting"), () => false)
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
