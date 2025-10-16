using Origins.Dev;
using Origins.Items.Materials;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Melee {
	public class Crystal_Cutters : ModItem, ICustomWikiStat {
        public string[] Categories => [
            WikiCategories.Sword
        ];
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.TerraBlade);
			Item.noMelee = false;
			Item.damage = 44;
			Item.DamageType = DamageClass.Melee;
			Item.width = 34;
			Item.height = 34;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 39;
			Item.useAnimation = 13;
			Item.knockBack = 4f;
			Item.shoot = ModContent.ProjectileType<Crystal_Cutters_P>();
			Item.shootSpeed = 7;
			Item.rare = ItemRarityID.LightRed;
			Item.autoReuse = true;
			Item.value = Item.sellPrice(silver: 20);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.CrystalShard, 14)
			.AddIngredient(ModContent.ItemType<Magic_Hair_Spray>(), 5)
			.AddIngredient(ModContent.ItemType<Rubber>(), 4)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	public class Crystal_Cutters_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Melee/Crystal_Cutters_P";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.CrystalDart);
			Projectile.DamageType = DamageClass.Melee;
			Projectile.aiStyle = 0;
			Projectile.alpha = 0;
			Projectile.timeLeft = 300;
		}
		public override Color? GetAlpha(Color lightColor) => new Color(255, 255, 255, 200) * Math.Min(Projectile.timeLeft / 30f, 1f);
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) => fallThrough = true; // only works because we happen to want to set fallThrough to the same value we want to return
		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (Projectile.velocity.X != oldVelocity.X) {
				Projectile.velocity.X = 0f - oldVelocity.X;
			}
			if (Projectile.velocity.Y != oldVelocity.Y) {
				Projectile.velocity.Y = 0f - oldVelocity.Y;
			}
			Projectile.timeLeft -= 3;
			return false;
		}
	}
}
