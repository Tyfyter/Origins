using Origins.Items.Accessories;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
namespace Origins.Items.Weapons.Melee {
	public class The_Bird : ModItem, ICustomWikiStat {
        public string[] Categories => [
            "Sword",
			"DeveloperItem",
			"ReworkExpected"
        ];
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WoodenSword);
			Item.damage = 99;
			Item.useAnimation = Item.useTime = 20;
			Item.rare = ItemRarityID.Cyan;
			Item.knockBack = 99999996;
		}
		public override void AddRecipes() {
			//Recipe.Create(Type)
			//.AddIngredient(ModContent.ItemType<Baseball_Bat>())
			//.AddIngredient(ModContent.ItemType<Razorwire>())
			//.AddCondition(player.name == "Pandora");
			//.Register();
		}
	}
}
