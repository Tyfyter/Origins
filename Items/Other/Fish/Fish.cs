using AltLibrary.Common.Systems;
using Origins.Items.Materials;
using Origins.World.BiomeData;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Other.Fish {
	public class Prikish : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 2;
		}
		public override void SetDefaults() {
			Item.DefaultToQuestFish();
		}
		public override bool IsAnglerQuestAvailable() {
			return WorldBiomeManager.GetWorldEvil(true) == ModContent.GetInstance<Defiled_Wastelands_Alt_Biome>();
		}
		public override bool IsQuestFish() => true;
		public override void AnglerQuestChat(ref string description, ref string catchLocation) {
			// How the angler describes the fish to the player.
			description = Language.GetTextValue("Mods.Origins.FishQuest.Prikish.Description");
			// What it says on the bottom of the angler's text box of how to catch the fish.
			catchLocation = Language.GetTextValue("Mods.Origins.FishQuest.Prikish.Location");
		}
	}
	public class Bonehead_Jellyfish : ModItem {
		static short glowmask;
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 2;
		}
		public override void SetDefaults() {
			Item.DefaultToQuestFish();
			Item.glowMask = glowmask;
		}
		public override bool IsAnglerQuestAvailable() {
			return WorldBiomeManager.GetWorldEvil(true) == ModContent.GetInstance<Riven_Hive_Alt_Biome>();
		}
		public override bool IsQuestFish() => true;
		public override void AnglerQuestChat(ref string description, ref string catchLocation) {
			description = Language.GetTextValue("Mods.Origins.FishQuest.Bonehead_Jellyfish.Description");
			catchLocation = Language.GetTextValue("Mods.Origins.FishQuest.Bonehead_Jellyfish.Location");
		}
	}
	public class Scrapfish : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 2;
		}
		public override void SetDefaults() {
			Item.DefaultToQuestFish();
		}
		public override bool IsAnglerQuestAvailable() {
			return WorldBiomeManager.GetWorldEvil(true) == ModContent.GetInstance<Ashen_Alt_Biome>();
		}
		public override bool IsQuestFish() => true;
		public override void AnglerQuestChat(ref string description, ref string catchLocation) {
			// How the angler describes the fish to the player.
			description = Language.GetTextValue("Mods.Origins.FishQuest.Scrapfish.Description");
			// What it says on the bottom of the angler's text box of how to catch the fish.
			catchLocation = Language.GetTextValue("Mods.Origins.FishQuest.Scrapfish.Location");
		}
	}
	public class Bobbit_Worm : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 2;
		}
		public override void SetDefaults() {
			Item.DefaultToQuestFish();
		}
		public override bool IsAnglerQuestAvailable() {
			return Main.hardMode;
		}
		public override bool IsQuestFish() => true;
		public override void AnglerQuestChat(ref string description, ref string catchLocation) {
			description = Language.GetTextValue("Mods.Origins.FishQuest.Bobbit_Worm.Description");
			catchLocation = Language.GetTextValue("Mods.Origins.FishQuest.Bobbit_Worm.Location");
		}
	}
	public class Fiberbass : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 2;
		}
		public override void SetDefaults() {
			Item.DefaultToQuestFish();
		}
		public override bool IsAnglerQuestAvailable() {
			return Main.hardMode;
		}
		public override bool IsQuestFish() => true;
		public override void AnglerQuestChat(ref string description, ref string catchLocation) {
			description = Language.GetTextValue("Mods.Origins.FishQuest.Fiberbass.Description");
			catchLocation = Language.GetTextValue("Mods.Origins.FishQuest.Fiberbass.Location");
		}
	}
	public class Tire : ModItem {
		public override void SetStaticDefaults() {
			ItemID.Sets.ExtractinatorMode[Type] = ItemID.OldShoe;
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.maxStack = Item.CommonMaxStack;
			Item.rare = ItemRarityID.Gray;
			Item.autoReuse = true;
			Item.consumable = true;
		}
		public override void AddRecipes() {
			Recipe.Create(ModContent.ItemType<Rubber>(), 3)
			.AddIngredient(this)
			.AddTile(TileID.HeavyWorkBench)
			.Register();
		}
	}
	public class Tearracuda : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 3;
		}
		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
			Item.width = 26;
			Item.height = 26;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 15);
		}
		public override void AddRecipes() {
			Recipe.Create(ItemID.SeafoodDinner)
			.AddIngredient(this, 2)
			.AddTile(TileID.CookingPots)
			.Register();

			Recipe.Create(ItemID.HeartreachPotion)
			.AddIngredient(ItemID.BottledWater)
			.AddIngredient(ItemID.Daybloom)
			.AddIngredient(this)
			.AddTile(TileID.CookingPots)
			.Register();
		}
	}
	public class Bilemouth : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 3;
		}
		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
			Item.width = 26;
			Item.height = 26;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 15);
		}
		public override void AddRecipes() {
			Recipe.Create(ItemID.SeafoodDinner)
			.AddIngredient(this, 2)
			.AddTile(TileID.CookingPots)
			.Register();

			Recipe.Create(ItemID.HeartreachPotion)
			.AddIngredient(ItemID.BottledWater)
			.AddIngredient(ItemID.Daybloom)
			.AddIngredient(this)
			.AddTile(TileID.CookingPots)
			.Register();
		}
	}
	public class Polyeel : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 3;
		}
		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
			Item.width = 26;
			Item.height = 26;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(0, 0, 15);
		}
		public override void AddRecipes() {
			Recipe.Create(ItemID.SeafoodDinner)
			.AddIngredient(this, 2)
			.AddTile(TileID.CookingPots)
			.Register();

			Recipe.Create(ItemID.HeartreachPotion)
			.AddIngredient(ItemID.BottledWater)
			.AddIngredient(ItemID.Daybloom)
			.AddIngredient(this)
			.AddTile(TileID.CookingPots)
			.Register();
		}
	}
	public class Toadfish : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 3;
		}
		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
			Item.width = 26;
			Item.height = 26;
			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.sellPrice(silver: 30);
		}
		public override void AddRecipes() {
			Recipe.Create(ItemID.SeafoodDinner)
			.AddIngredient(this, 2)
			.AddTile(TileID.CookingPots)
			.Register();
		}
	}
}
