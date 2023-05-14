using Origins.Projectiles;
using Origins.Tiles.Dusk;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ammo {
	public class Resizable_Mine_One : Resizable_Mine_Two {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Resizable Mine Mk. I");
			Tooltip.SetDefault("'Compatible with your garden-variety mine launchers!'");
			SacrificeTotal = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Grenade);
			Item.maxStack = 999;
			Item.damage = 14;
			Item.shoot = ModContent.ProjectileType<Resizable_Mine_P>();
			Item.ammo = Type;
			Item.shootSpeed = 3.7f;
			Item.knockBack = 2f;
			Item.value = Item.sellPrice(silver: 2, copper: 33);
			Item.rare = ItemRarityID.White;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 2);
			recipe.AddIngredient(ItemID.ExplosivePowder);
			recipe.AddIngredient(ItemID.Wood, 2);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
	}
	public class Resizable_Mine_Two : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Resizable Mine Mk. II");
			Tooltip.SetDefault("'Compatible with your garden-variety mine launchers!'");
			SacrificeTotal = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Grenade);
			Item.maxStack = 999;
			Item.damage = 22;
			Item.shoot = ModContent.ProjectileType<Resizable_Mine_P>();
			Item.ammo = ModContent.ItemType<Resizable_Mine_One>();
			Item.shootSpeed = 4f;
            Item.knockBack = 3f;
			Item.value = Item.sellPrice(silver: 4, copper: 65);
			Item.rare = ItemRarityID.Green;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 2);
			recipe.AddIngredient(ItemID.ExplosivePowder);
			recipe.AddIngredient(ItemID.IronOre, 2);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
	public class Resizable_Mine_Three : Resizable_Mine_Two {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Resizable Mine Mk. III");
			Tooltip.SetDefault("'Compatible with your garden-variety mine launchers!'");
			SacrificeTotal = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Grenade);
			Item.maxStack = 999;
			Item.damage = 31;
			Item.shoot = ModContent.ProjectileType<Resizable_Mine_P>();
			Item.ammo = ModContent.ItemType<Resizable_Mine_One>();
			Item.shootSpeed = 4.4f;
			Item.knockBack = 3.6f;
			Item.value = Item.sellPrice(silver: 8, copper: 80);
			Item.rare = ItemRarityID.Pink;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 2);
			recipe.AddIngredient(ItemID.ExplosivePowder);
			recipe.AddIngredient(ModContent.ItemType<Bleeding_Obsidian_Item>(), 2);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
	}
	public class Resizable_Mine_Four : Resizable_Mine_Two {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Resizable Mine Mk. IV");
			Tooltip.SetDefault("'Compatible with your garden-variety mine launchers!'");
			SacrificeTotal = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Grenade);
			Item.maxStack = 999;
			Item.damage = 45;
			Item.shoot = ModContent.ProjectileType<Resizable_Mine_P>();
			Item.ammo = ModContent.ItemType<Resizable_Mine_One>();
			Item.shootSpeed = 4.8f;
			Item.knockBack = 4.3f;
			Item.value = Item.sellPrice(silver: 13);
			Item.rare = ItemRarityID.Yellow;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 2);
			recipe.AddIngredient(ItemID.ExplosivePowder);
			recipe.AddIngredient(ItemID.ChlorophyteOre, 2);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
	}
	public class Resizable_Mine_Five : Resizable_Mine_Two {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Resizable Mine Mk. V");
			Tooltip.SetDefault("'Compatible with your garden-variety mine launchers!'");
			SacrificeTotal = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Grenade);
			Item.maxStack = 999;
			Item.damage = 50;
			Item.shoot = ModContent.ProjectileType<Resizable_Mine_P>();
			Item.ammo = ModContent.ItemType<Resizable_Mine_One>();
			Item.shootSpeed = 5.2f;
			Item.knockBack = 4.8f;
			Item.value = Item.sellPrice(silver: 26);
			Item.rare = ItemRarityID.Cyan;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 2);
			recipe.AddIngredient(ItemID.ExplosivePowder);
			recipe.AddIngredient(ItemID.LunarOre, 2);
			recipe.AddTile(TileID.LunarCraftingStation);
			recipe.Register();
		}
	}
	public class Resizable_Mine_P : ModProjectile, IIsExplodingProjectile {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Resizable Mine");
			Origins.MagicTripwireRange[Type] = 40;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ProximityMineI);
			Projectile.timeLeft = 420;
			Projectile.scale = 0.5f;
			Projectile.penetrate = 1;
		}
		public override bool PreKill(int timeLeft) {
			Projectile.penetrate = -1;
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 96;
			Projectile.height = 96;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.Damage();
			ExplosiveGlobalProjectile.ExplosionVisual(Projectile, true, sound: SoundID.Item62);
			return false;
		}
		public bool IsExploding() => Projectile.penetrate == -1;
	}
}
