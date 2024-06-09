using Origins.Items.Materials;
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
			return OriginSystem.HasDefiledWastelands;
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
			return OriginSystem.HasRivenHive;
		}
		public override bool IsQuestFish() => true;
		public override void AnglerQuestChat(ref string description, ref string catchLocation) {
			description = Language.GetTextValue("Mods.Origins.FishQuest.Bonehead.Description");
			catchLocation = Language.GetTextValue("Mods.Origins.FishQuest.Bonehead.Location");
		}
	}
	public class Duskarp : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 2;
		}
		public override void SetDefaults() {
			Item.DefaultToQuestFish();
		}
		public override bool IsAnglerQuestAvailable() {
			return false; //OriginSystem.WorldEvil == OriginSystem.dusk;
		}
		public override bool IsQuestFish() => true;
		public override void AnglerQuestChat(ref string description, ref string catchLocation) {
			description = Language.GetTextValue("Mods.Origins.FishQuest.Duskarp.Description");
			catchLocation = Language.GetTextValue("Mods.Origins.FishQuest.Duskarp.Location");
		}
	}
	public class Tire : ModItem {
		public override void SetStaticDefaults() {
			ItemID.Sets.ExtractinatorMode[Type] = 1;
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.maxStack = 9999;
			Item.rare = ItemRarityID.Gray;
			Item.autoReuse = true;
			Item.consumable = true;
		}
		public override void ExtractinatorUse(int extractinatorBlockType, ref int resultType, ref int resultStack) {

		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(ModContent.ItemType<Rubber>(), 3);
			recipe.AddIngredient(this);
			recipe.AddTile(TileID.HeavyWorkBench);
			recipe.Register();
		}
	}
	public class Tearracuda : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 3;

			ItemID.Sets.ShimmerTransformToItem[ItemID.Ebonkoi] = ItemID.Hemopiranha;
			ItemID.Sets.ShimmerTransformToItem[ItemID.Hemopiranha] = ModContent.ItemType<Bilemouth>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Bilemouth>()] = ModContent.ItemType<Tearracuda>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Tearracuda>()] = ModContent.ItemType<Polyeel>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Polyeel>()] = ItemID.Ebonkoi;
		}
		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
			Item.width = 26;
			Item.height = 26;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(0, 0, 15);
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(ItemID.SeafoodDinner);
			recipe.AddIngredient(this, 2);
			recipe.AddTile(TileID.CookingPots);
			recipe.Register();

			recipe = Recipe.Create(ItemID.HeartreachPotion);
			recipe.AddIngredient(ItemID.BottledWater);
			recipe.AddIngredient(ItemID.Daybloom);
			recipe.AddIngredient(this);
			recipe.AddTile(TileID.CookingPots);
			recipe.Register();
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
			Item.value = Item.sellPrice(0, 0, 15);
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(ItemID.SeafoodDinner);
			recipe.AddIngredient(this, 2);
			recipe.AddTile(TileID.CookingPots);
			recipe.Register();

			recipe = Recipe.Create(ItemID.HeartreachPotion);
			recipe.AddIngredient(ItemID.BottledWater);
			recipe.AddIngredient(ItemID.Daybloom);
			recipe.AddIngredient(this);
			recipe.AddTile(TileID.CookingPots);
			recipe.Register();
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
			Recipe recipe = Recipe.Create(ItemID.SeafoodDinner);
			recipe.AddIngredient(this, 2);
			recipe.AddTile(TileID.CookingPots);
			recipe.Register();

			recipe = Recipe.Create(ItemID.HeartreachPotion);
			recipe.AddIngredient(ItemID.BottledWater);
			recipe.AddIngredient(ItemID.Daybloom);
			recipe.AddIngredient(this);
			recipe.AddTile(TileID.CookingPots);
			recipe.Register();
		}
	}
}
