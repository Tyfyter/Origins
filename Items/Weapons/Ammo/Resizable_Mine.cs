using Origins.Projectiles;
using Origins.Tiles.Dusk;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ammo {
	public class Resizable_Mine_One : Resizable_Mine_Two {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Grenade);
			Item.damage = 14;
			Item.shoot = 1;
			Item.ammo = Type;
			Item.shootSpeed = 3.7f;
			Item.knockBack = 2f;
			Item.value = Item.sellPrice(silver: 2, copper: 33);
			Item.rare = ItemRarityID.White;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 4);
			recipe.AddIngredient(ItemID.ExplosivePowder);
			recipe.AddIngredient(ItemID.Wood, 4);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
	}
	public class Resizable_Mine_Two : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Grenade);
			Item.maxStack = 999;
			Item.damage = 20;
			Item.shoot = 2;
			Item.ammo = ModContent.ItemType<Resizable_Mine_One>();
			Item.shootSpeed = 4f;
            Item.knockBack = 3f;
			Item.value = Item.sellPrice(silver: 4, copper: 65);
			Item.rare = ItemRarityID.Green;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 4);
			recipe.AddIngredient(ItemID.ExplosivePowder);
            recipe.AddRecipeGroup(RecipeGroupID.IronBar, 2);
            recipe.AddTile(TileID.Anvils);
			recipe.Register();
        }
	}
	public class Resizable_Mine_Three : Resizable_Mine_Two {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Grenade);
			Item.maxStack = 999;
			Item.damage = 31;
			Item.shoot = 3;
			Item.ammo = ModContent.ItemType<Resizable_Mine_One>();
			Item.shootSpeed = 4.4f;
			Item.knockBack = 3.6f;
			Item.value = Item.sellPrice(silver: 8, copper: 80);
			Item.rare = ItemRarityID.Pink;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 4);
			recipe.AddIngredient(ItemID.ExplosivePowder);
			recipe.AddIngredient(ModContent.ItemType<Bleeding_Obsidian_Item>(), 4);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
	}
	public class Resizable_Mine_Four : Resizable_Mine_Two {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Grenade);
			Item.maxStack = 999;
			Item.damage = 45;
			Item.shoot = 4;
			Item.ammo = ModContent.ItemType<Resizable_Mine_One>();
			Item.shootSpeed = 4.8f;
			Item.knockBack = 4.3f;
			Item.value = Item.sellPrice(silver: 13);
			Item.rare = ItemRarityID.Yellow;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 4);
            recipe.AddIngredient(ItemID.ChlorophyteBar, 2);
            recipe.AddIngredient(ItemID.ExplosivePowder);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
	}
	public class Resizable_Mine_Five : Resizable_Mine_Two {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Grenade);
			Item.maxStack = 999;
			Item.damage = 50;
			Item.shoot = 5;
			Item.ammo = ModContent.ItemType<Resizable_Mine_One>();
			Item.shootSpeed = 5.2f;
			Item.knockBack = 4.8f;
			Item.value = Item.sellPrice(silver: 26);
			Item.rare = ItemRarityID.Cyan;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 4);
			recipe.AddIngredient(ItemID.ExplosivePowder);
			recipe.AddIngredient(ItemID.LunarBar, 2);
			recipe.AddTile(TileID.LunarCraftingStation);
			recipe.Register();
		}
	}
	public abstract class Resizable_Mine_P<TextureItem> : ModProjectile, IIsExplodingProjectile {
		public override string Texture => (typeof(TextureItem).Namespace + "." + typeof(TextureItem).Name).Replace('.', '/');
		public override void SetStaticDefaults() {
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
	public class Resizable_Mine_P_1 : Resizable_Mine_P<Resizable_Mine_One> { }
	public class Resizable_Mine_P_2 : Resizable_Mine_P<Resizable_Mine_Two> { }
	public class Resizable_Mine_P_3 : Resizable_Mine_P<Resizable_Mine_Three> { }
	public class Resizable_Mine_P_4 : Resizable_Mine_P<Resizable_Mine_Four> { }
	public class Resizable_Mine_P_5 : Resizable_Mine_P<Resizable_Mine_Five> { }
}
