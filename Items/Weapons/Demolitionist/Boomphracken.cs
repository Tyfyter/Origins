using Microsoft.Xna.Framework;
using Origins.Items.Weapons.Ammo;
using Origins.Items.Weapons.Ranged;
using Origins.Tiles.Dusk;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
    public class Boomphracken : ModItem {
		static short glowmask;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Boomphracken");
			// Tooltip.SetDefault("Chance to throw an explosive when used\n'He works his work, I work mine'");
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Musket);
			Item.damage = 68;
			Item.width = 56;
			Item.height = 28;
			Item.useTime = 57;
			Item.useAnimation = 57;
			Item.shoot = ModContent.ProjectileType<Boomphracken_P>();
			Item.useAmmo = ModContent.ItemType<Metal_Slug>();
			Item.knockBack = 10f;
			Item.shootSpeed = 24f;
			Item.value = Item.sellPrice(gold: 20);
			Item.rare = ItemRarityID.Pink;
			Item.UseSound = Origins.Sounds.Krunch.WithPitch(-0.25f);
			Item.autoReuse = true;

		}
		public override Vector2? HoldoutOffset() {
			return Vector2.Zero;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.IllegalGunParts, 2);
			recipe.AddIngredient(ModContent.ItemType<Bleeding_Obsidian_Item>(), 8);
			recipe.AddIngredient(ModContent.ItemType<Hallowed_Cleaver>());
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (type == Metal_Slug_P.ID) type = Item.shoot;
		}
	}
	public class Boomphracken_P : ModProjectile {
		public override string Texture => "Origins/Projectiles/Ammo/Boomphracken_P";
		
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ExplosiveBullet);
			Projectile.width = 10;
			Projectile.height = 8;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 900;
			Projectile.alpha = 0;
		}
		public override void AI() {

		}
	}
}
