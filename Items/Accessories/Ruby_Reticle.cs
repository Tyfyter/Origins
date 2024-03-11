using Origins.Dev;
using Origins.Tiles.Other;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Ruby_Reticle : ModItem, ICustomWikiStat {
		public string[] Categories => new string[] {
			"Combat"
		};
		public override void SetDefaults() {
			Item.DefaultToAccessory(28, 30);
			Item.rare = ItemRarityID.Green;
			Item.value = Item.sellPrice(gold: 2);
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.Ruby, 4);
			recipe.AddIngredient(ModContent.ItemType<Carburite_Item>(), 24);
            recipe.AddRecipeGroup(OriginSystem.CursedFlameRecipeGroupID, 5);
            recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().rubyReticle = true;
		}
	}
}
