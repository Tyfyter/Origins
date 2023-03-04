using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class Priority_Mail : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Priority Mail");
			Tooltip.SetDefault("Enemies recently struck by a teammate receive 18% more damage");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(30, 28);
			Item.accessory = true;
			Item.rare = ItemRarityID.Green;
			Item.value = Item.sellPrice(gold: 1);
		}
		public override void UpdateEquip(Player player) {
			//player.GetModPlayer<OriginPlayer>().priorityMail = true;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 2);
			recipe.AddIngredient(ItemID.Book);
			recipe.AddIngredient(ItemID.FallenStar, 2);
			recipe.AddIngredient(ItemID.PaperAirplaneA, 4);
			recipe.AddIngredient(ModContent.ItemType<Asylum_Whistle>());
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
	}
}
