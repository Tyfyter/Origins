using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Other.Fish {
	public class Prikish : ModItem {
		public override void SetStaticDefaults() {
			SacrificeTotal = 2;
		}
		public override void SetDefaults() {
			Item.DefaultToQuestFish();
		}
		public override bool IsAnglerQuestAvailable() {
			return OriginSystem.WorldEvil == OriginSystem.evil_wastelands;
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
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Bonehead Jellyfish");
			SacrificeTotal = 3;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Ebonkoi);
			Item.rare = ItemRarityID.Blue;
		}
	}
	public class Duskarp : ModItem {
		public override void SetStaticDefaults() {
			SacrificeTotal = 3;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Ebonkoi);
			Item.rare = ItemRarityID.Blue;
		}
	}
	public class Tire : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Tire");
			ItemID.Sets.ExtractinatorMode[Type] = 1;
			SacrificeTotal = 1;
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
		public override void ExtractinatorUse(ref int resultType, ref int resultStack) {
			base.ExtractinatorUse(ref resultType, ref resultStack);
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
			SacrificeTotal = 3;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Hemopiranha);
			Item.rare = ItemRarityID.Blue;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(ItemID.SeafoodDinner);
			recipe.AddIngredient(this, 2);
			recipe.AddTile(TileID.CookingPots);
			recipe.Register();
		}
	}
	public class Bilemouth : ModItem {
		public override void SetStaticDefaults() {
			SacrificeTotal = 3;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Hemopiranha);
			Item.rare = ItemRarityID.Blue;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(ItemID.SeafoodDinner);
			recipe.AddIngredient(this, 2);
			recipe.AddTile(TileID.CookingPots);
			recipe.Register();
		}
	}
}
