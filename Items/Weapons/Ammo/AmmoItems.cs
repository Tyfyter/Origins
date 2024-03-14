using Microsoft.Xna.Framework;
using Origins.Dusts;
using Origins.Items.Weapons.Demolitionist;
using Origins.Projectiles;
using Origins.World.BiomeData;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ammo {
    public class Thermite_Canister : ModItem, ICanisterAmmo {
        static short glowmask;
		public CanisterData GetCanisterData => new(new(211, 194, 182), new(255, 163, 68));
		public override void SetStaticDefaults() {
            glowmask = Origins.AddGlowMask(this);
            Item.ResearchUnlockCount = 199;
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.RocketI);
            Item.damage = 26;
            Item.shoot = ModContent.ProjectileType<Thermite_Canister_P>();
            Item.ammo = Item.type;
            Item.glowMask = glowmask;
            Item.value = Item.sellPrice(silver: 3, copper: 2);
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type, 5);
            recipe.AddIngredient(ItemID.Fireblossom);
            recipe.AddRecipeGroup(RecipeGroupID.IronBar, 5);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
    public class Metal_Slug : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 199;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.MusketBall);
			Item.damage = 25;
			Item.shoot = Metal_Slug_P.ID;
			Item.ammo = Item.type;
			Item.value = Item.sellPrice(silver: 3);
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 5);
            recipe.AddIngredient(ItemID.ExplosivePowder);
            recipe.AddRecipeGroup(RecipeGroupID.IronBar);
            recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
	}
	public class Metal_Slug_P : ModProjectile {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
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
	public class Gray_Solution : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.GreenSolution);
			Item.shoot = ModContent.ProjectileType<Gray_Solution_P>() - ProjectileID.PureSpray;
			Item.value = Item.sellPrice(silver: 3);
		}
	}
	public class Gray_Solution_P : ModProjectile {
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
			Item.ResearchUnlockCount = 99;
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
