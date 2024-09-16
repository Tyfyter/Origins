using Origins.Projectiles;
using Origins.Tiles.Dusk;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.Dev;
using Microsoft.Xna.Framework;
namespace Origins.Items.Weapons.Ammo.Canisters {
	public class Resizable_Mine_One : ModItem, ICanisterAmmo {
		public CanisterData GetCanisterData => new(new(219, 131, 72), new(255, 193, 97), false);
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Grenade);
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Item.useStyle = ItemUseStyleID.None;
			Item.damage = 14;
			Item.ammo = Type;
			Item.shootSpeed = -1.3f;
			Item.knockBack = 2f;
			Item.value = Item.sellPrice(silver: 2, copper: 33);
			Item.rare = ItemRarityID.Blue;
			Item.ArmorPenetration += 1;
			Item.maxStack = 9999;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 8);
			recipe.AddIngredient(ItemID.ExplosivePowder);
			recipe.AddIngredient(ItemID.Wood, 2);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
	}
	public class Resizable_Mine_Two : ModItem, ICustomWikiStat, ICanisterAmmo {
		public CanisterData GetCanisterData => new(new(188, 171, 167), new(246, 69, 84), false);
		public string[] Categories => [
			"Canistah"
		];
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.DefaultToCanister(22);
			Item.shootSpeed = 0f;
			Item.knockBack = 3f;
			Item.value = Item.sellPrice(silver: 4, copper: 65);
			Item.rare = ItemRarityID.Green;
			Item.ArmorPenetration += 2;
			Item.maxStack = 9999;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 8);
			recipe.AddIngredient(ItemID.ExplosivePowder);
			recipe.AddRecipeGroup(RecipeGroupID.IronBar, 2);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
	public class Resizable_Mine_Three : ModItem, ICanisterAmmo {
		public CanisterData GetCanisterData => new(new(141, 22, 38), new(163, 108, 255), false);
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.DefaultToCanister(35);
			Item.shootSpeed = 0.2f;
			Item.knockBack = 3.6f;
			Item.value = Item.sellPrice(silver: 8, copper: 80);
			Item.rare = ItemRarityID.Pink;
			Item.ArmorPenetration += 3;
			Item.maxStack = 9999;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 8);
			recipe.AddIngredient(ItemID.ExplosivePowder);
			recipe.AddIngredient(ModContent.ItemType<Bleeding_Obsidian_Item>(), 2);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
	}
	public class Resizable_Mine_Four : ModItem, ICanisterAmmo {
		public CanisterData GetCanisterData => new(new(161, 236, 0), new(97, 255, 238), false);
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.DefaultToCanister(48);
			Item.shootSpeed = 0.6f;
			Item.knockBack = 4.3f;
			Item.value = Item.sellPrice(silver: 13);
			Item.rare = ItemRarityID.Yellow;
			Item.ArmorPenetration += 4;
			Item.maxStack = 9999;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 8);
			recipe.AddIngredient(ItemID.ChlorophyteOre, 2);
			recipe.AddIngredient(ItemID.ExplosivePowder);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
	}
	public class Resizable_Mine_Five : ModItem, ICanisterAmmo {
		public CanisterData GetCanisterData => new(new(223, 218, 205), new(97, 255, 133), false);
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.DefaultToCanister(60);
			Item.shootSpeed = 1f;
			Item.knockBack = 4.8f;
			Item.value = Item.sellPrice(silver: 26);
			Item.rare = ItemRarityID.Cyan;
			Item.ArmorPenetration += 5;
			Item.maxStack = 9999;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 16);
			recipe.AddIngredient(ItemID.ExplosivePowder);
			recipe.AddIngredient(ItemID.LunarOre, 2);
			recipe.AddTile(TileID.LunarCraftingStation);
			recipe.Register();
		}
	}
}
