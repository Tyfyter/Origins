using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Fish {
	public class Prikish : ModItem {
		public override void SetStaticDefaults() {
			SacrificeTotal = 3;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Ebonkoi);
			Item.rare = ItemRarityID.Quest;
		}
	}
	public class Bonehead_Jellyfish : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Bonehead Jellyfish");
			SacrificeTotal = 3;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Ebonkoi);
			Item.rare = ItemRarityID.Quest;
		}
	}
	public class Duskarp : ModItem {
		public override void SetStaticDefaults() {
			SacrificeTotal = 3;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Ebonkoi);
			Item.rare = ItemRarityID.Quest;
		}
	}
	public class Tire : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Tire");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.maxStack = 9990;
			Item.rare = ItemRarityID.Gray;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(ModContent.ItemType<Rubber>(), 3);
			recipe.AddIngredient(this);
			recipe.AddTile(TileID.HeavyWorkBench);
			recipe.Register();
		}
	}
}
