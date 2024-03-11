using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Focus_Crystal : ModItem, ICustomWikiStat {
		public string[] Categories => new string[] {
			"Combat"
		};
		public override void SetDefaults() {
			Item.DefaultToAccessory(28, 30);
			Item.rare = ItemRarityID.Yellow;
			Item.value = Item.sellPrice(gold: 2);
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.ShinyStone);
            recipe.AddIngredient(ModContent.ItemType<Ruby_Reticle>());
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
