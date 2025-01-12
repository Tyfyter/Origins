using Origins.Projectiles;
using Origins.Tiles.Dusk;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.Dev;
using Microsoft.Xna.Framework;
using Origins.Items.Materials;
namespace Origins.Items.Weapons.Ammo.Canisters {
	public class Resizable_Mine_One : ModItem, ICanisterAmmo {
		public CanisterData GetCanisterData => new(new(219, 131, 72), new(255, 193, 97), false);
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Grenade);
			Item.DefaultToCanister(14);
			Item.shootSpeed = -1.3f;
			Item.knockBack = 2f;
			Item.value = Item.sellPrice(silver: 2, copper: 33);
			Item.rare = ItemRarityID.Blue;
			Item.ArmorPenetration += 7;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 8)
			.AddIngredient(ItemID.ExplosivePowder)
			.AddIngredient(ItemID.Wood, 2)
			.AddTile(TileID.WorkBenches)
			.Register();
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
			Item.rare = ItemRarityID.Blue;
			Item.ArmorPenetration += 6;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 8)
			.AddIngredient(ItemID.ExplosivePowder)
			.AddRecipeGroup(RecipeGroupID.IronBar, 2)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	public class Resizable_Mine_Three : ModItem, ICustomWikiStat, ICanisterAmmo {
		public CanisterData GetCanisterData => new(new(212, 33, 88), new(255, 108, 163), false);
		public string[] Categories => [
			"Canistah"
		];
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.DefaultToCanister(28);
			Item.shootSpeed = 0.2f;
			Item.knockBack = 3.4f;
			Item.value = Item.sellPrice(silver: 6, copper: 50);
			Item.rare = ItemRarityID.Green;
			Item.ArmorPenetration += 5;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 8)
			.AddIngredient(ItemID.ExplosivePowder)
			.AddIngredient(ItemID.DemoniteBar, 2)
			.AddTile(TileID.Anvils)
			.Register();

			Recipe.Create(Type, 8)
			.AddIngredient(ItemID.ExplosivePowder)
			.AddIngredient(ItemID.CrimtaneBar, 2)
			.AddTile(TileID.Anvils)
			.Register();

			Recipe.Create(Type, 8)
			.AddIngredient(ItemID.ExplosivePowder)
			.AddIngredient(ModContent.ItemType<Defiled_Bar>(), 2)
			.AddTile(TileID.Anvils)
			.Register();

			Recipe.Create(Type, 8)
			.AddIngredient(ItemID.ExplosivePowder)
			.AddIngredient(ModContent.ItemType<Encrusted_Bar>(), 2)
			.AddTile(TileID.Anvils)
			.Register();

			Recipe.Create(Type, 8)
			.AddIngredient(ItemID.ExplosivePowder)
			.AddIngredient(ModContent.ItemType<Sanguinite_Bar>(), 2)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	public class Resizable_Mine_Four : ModItem, ICanisterAmmo {
		public CanisterData GetCanisterData => new(new(106, 20, 20), new(255, 165, 0), false);
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.DefaultToCanister(35);
			Item.shootSpeed = 0.3f;
			Item.knockBack = 3.6f;
			Item.value = Item.sellPrice(silver: 7, copper: 80);
			Item.rare = ItemRarityID.Orange;
			Item.ArmorPenetration += 4;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 8)
			.AddIngredient(ItemID.ExplosivePowder)
			.AddIngredient(ItemID.HellstoneBar, 2)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	public class Resizable_Mine_Five : ModItem, ICanisterAmmo {
		public CanisterData GetCanisterData => new(new(141, 22, 38), new(163, 108, 255), false);
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.DefaultToCanister(40);
			Item.shootSpeed = 0.4f;
			Item.knockBack = 4f;
			Item.value = Item.sellPrice(silver: 9, copper: 30);
			Item.rare = ItemRarityID.Pink;
			Item.ArmorPenetration += 3;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 8)
			.AddIngredient(ItemID.ExplosivePowder)
			.AddIngredient(ModContent.ItemType<Bleeding_Obsidian_Item>(), 2)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
	public class Resizable_Mine_Six : ModItem, ICanisterAmmo {
		public CanisterData GetCanisterData => new(new(161, 236, 0), new(97, 255, 238), false);
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.DefaultToCanister(48);
			Item.shootSpeed = 0.6f;
			Item.knockBack = 4.4f;
			Item.value = Item.sellPrice(silver: 13);
			Item.rare = ItemRarityID.Yellow;
			Item.ArmorPenetration += 2;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 8)
			.AddIngredient(ItemID.ChlorophyteOre, 2)
			.AddIngredient(ItemID.ExplosivePowder)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
	public class Resizable_Mine_Seven : ModItem, ICanisterAmmo {
		public CanisterData GetCanisterData => new(new(223, 218, 205), new(97, 255, 133), false);
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.DefaultToCanister(60);
			Item.shootSpeed = 1f;
			Item.knockBack = 4.9f;
			Item.value = Item.sellPrice(silver: 26);
			Item.rare = ItemRarityID.Cyan;
			Item.ArmorPenetration += 1;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 16)
			.AddIngredient(ItemID.ExplosivePowder)
			.AddIngredient(ItemID.LunarOre, 2)
			.AddTile(TileID.LunarCraftingStation)
			.Register();
		}
	}
}
