using Microsoft.Xna.Framework;
using Origins.Dusts;
using Origins.Items.Materials;
using Origins.Items.Weapons.Demolitionist;
using Origins.Projectiles;
using Origins.World.BiomeData;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ammo {
	public class Giant_Metal_Slug : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Giant Metal Slug");
			SacrificeTotal = 199;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.MusketBall);
			Item.damage = 25;
			Item.shoot = Giant_Metal_Slug_P.ID;
			Item.ammo = Item.type;
			Item.value = Item.sellPrice(silver: 3);
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 10);
			recipe.AddIngredient(ItemID.IronBar);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
			recipe = Recipe.Create(Type, 10);
			recipe.AddIngredient(ItemID.LeadBar);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
	}
	public class Giant_Metal_Slug_P : ModProjectile {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Giant Metal Slug");
			ID = Type;
		}
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

	public class Thermite_Canister : ModItem {
		static short glowmask;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Thermite Canister");
			glowmask = Origins.AddGlowMask(this);
			SacrificeTotal = 199;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.RocketI);
			Item.damage = 20;
			Item.shoot = ModContent.ProjectileType<Thermite_Canister_P>();
			Item.ammo = Item.type;
			Item.glowMask = glowmask;
			Item.value = Item.sellPrice(silver: 3, copper: 2);
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 4);
			recipe.AddIngredient(ItemID.Fireblossom, 2);
			recipe.AddIngredient(ItemID.IronBar, 2);
			recipe.AddIngredient(ModContent.ItemType<Silicon>());
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
			recipe = Recipe.Create(Type, 4);
			recipe.AddIngredient(ItemID.Fireblossom, 2);
			recipe.AddIngredient(ItemID.LeadBar, 2);
			recipe.AddIngredient(ModContent.ItemType<Silicon>());
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
	}
	public class White_Solution : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Gray Solution");
			Tooltip.SetDefault("Spreads the Defiled");
			SacrificeTotal = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.GreenSolution);
			Item.shoot = ModContent.ProjectileType<White_Solution_P>() - ProjectileID.PureSpray;
			Item.value = Item.sellPrice(silver: 3);
		}
	}
	public class White_Solution_P : ModProjectile {
		public override string Texture => "Origins/Projectiles/Pixel";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.PureSpray);
			Projectile.aiStyle = 0;
		}
		public override void AI() {
			OriginGlobalProj.ClentaminatorAI<Defiled_Wastelands_Alt_Biome>(Projectile, ModContent.DustType<Solution_D>(), Color.GhostWhite);
		}
	}
	public class Teal_Solution : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Teal Solution");
			Tooltip.SetDefault("Spreads the Riven");
			SacrificeTotal = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.GreenSolution);
			Item.shoot = ModContent.ProjectileType<Teal_Solution_P>() - ProjectileID.PureSpray;
			Item.value = Item.sellPrice(silver: 3);
		}
	}
	public class Teal_Solution_P : ModProjectile {
		public override string Texture => "Origins/Projectiles/Pixel";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.PureSpray);
			Projectile.aiStyle = 0;
		}
		public override void AI() {
			OriginGlobalProj.ClentaminatorAI<Riven_Hive_Alt_Biome>(Projectile, ModContent.DustType<Solution_D>(), Color.Teal);
		}
	}
}
