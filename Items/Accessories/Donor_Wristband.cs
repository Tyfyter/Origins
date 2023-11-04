using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Donor_Wristband : ModItem {
		
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 20);
			Item.value = Item.sellPrice(silver: 60);
			Item.rare = ItemRarityID.Blue;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.CharmofMyths);
			recipe.AddIngredient(ModContent.ItemType<Plasma_Phial>());
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}
		public override void UpdateEquip(Player player) {
			player.lifeRegen += 2;
			player.pStone = true;
			player.GetModPlayer<OriginPlayer>().donorWristband = true;
		}
	}
}
