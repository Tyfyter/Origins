using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Focus_Crystal : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Focus Crystal");
			Tooltip.SetDefault("Life regeneration is boosted from dealing damage\nCritical strike chance is increased by 15% of weapon damage\nBase damage increased while standing still");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(28, 30);
			Item.rare = ItemRarityID.Yellow;
			Item.value = Item.sellPrice(gold: 2);
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Ruby_Reticle>());
			recipe.AddIngredient(ItemID.ShinyStone);
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.rubyReticle = true;
			originPlayer.focusCrystal = true;
		}
	}
}
