using AltLibrary.Common.Systems;
using Origins.Dev;
using Origins.Tiles.Dusk;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Ammo.Canisters {
	[LegacyName("Resizable_Mine_One")]
	public class Resizable_Mine_Wood : ModItem, ICanisterAmmo {
		public CanisterData GetCanisterData => new(new(219, 131, 72), new(255, 193, 97), false);
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.DefaultToCanister(12);
			Item.shootSpeed = -1.3f;
			Item.knockBack = 2f;
			Item.value = Item.sellPrice(silver: 2, copper: 33);
			Item.rare = ItemRarityID.Blue;
			Item.ArmorPenetration += 12;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 8)
			.AddIngredient(ItemID.ExplosivePowder)
			.AddIngredient(ItemID.Wood, 2)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
	}
	[LegacyName("Resizable_Mine_Two")]
	public class Resizable_Mine_Iron : ModItem, ICustomWikiStat, ICanisterAmmo {
		public CanisterData GetCanisterData => new(new(188, 171, 167), new(246, 69, 84), false);
		public string[] Categories => [
			WikiCategories.Canistah
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
			Item.ArmorPenetration += 4;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 10)
			.AddIngredient(ItemID.ExplosivePowder)
			.AddRecipeGroup(RecipeGroupID.IronBar, 2)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	public class Resizable_Mine_Evil : ModItem, ICustomWikiStat, ICanisterAmmo {
		public CanisterData GetCanisterData => new(new(212, 33, 88), new(255, 108, 163), false);
		public string[] Categories => [
			WikiCategories.Canistah
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
			Item.ArmorPenetration += 4;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 12)
			.AddIngredient(ItemID.ExplosivePowder)
			.AddRecipeGroup(RecipeGroups.EvilBars, 2)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	public class Resizable_Mine_Hellstone : ModItem, ICanisterAmmo {
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
			Item.ArmorPenetration += 3;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 16)
			.AddIngredient(ItemID.ExplosivePowder)
			.AddIngredient(ItemID.HellstoneBar, 2)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	[LegacyName("Resizable_Mine_Three")]
	public class Resizable_Mine_Bleeding_Obsidian : ModItem, ICanisterAmmo {
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
			Recipe.Create(Type, 22)
			.AddIngredient(ItemID.ExplosivePowder)
			.AddIngredient(ModContent.ItemType<Bleeding_Obsidian_Item>(), 2)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
	[LegacyName("Resizable_Mine_Four")]
	public class Resizable_Mine_Chlorophyte : ModItem, ICanisterAmmo {
		public CanisterData GetCanisterData => new(new(161, 236, 0), new(97, 255, 238), false);
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.DefaultToCanister(60);
			Item.shootSpeed = 0.6f;
			Item.knockBack = 4.4f;
			Item.value = Item.sellPrice(silver: 13);
			Item.rare = ItemRarityID.Yellow;
			Item.ArmorPenetration += 2;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 28)
			.AddIngredient(ItemID.ChlorophyteOre, 2)
			.AddIngredient(ItemID.ExplosivePowder)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
	[LegacyName("Resizable_Mine_Five")]
	public class Resizable_Mine_Luminite : ModItem, ICanisterAmmo {
		public CanisterData GetCanisterData => new(new(223, 218, 205), new(97, 255, 133), false);
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.DefaultToCanister(75);
			Item.shootSpeed = 1f;
			Item.knockBack = 4.9f;
			Item.value = Item.sellPrice(silver: 26);
			Item.rare = ItemRarityID.Cyan;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 36)
			.AddIngredient(ItemID.ExplosivePowder)
			.AddIngredient(ItemID.LunarOre, 2)
			.AddTile(TileID.LunarCraftingStation)
			.Register();
		}
	}
}
