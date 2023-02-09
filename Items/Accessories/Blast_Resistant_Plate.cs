using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Blast_Resistant_Plate : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Blast Resistant Plate");
			Tooltip.SetDefault("Reduces explosive self-damage by 20%");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 28);
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void UpdateEquip(Player player) {
			player.statDefense += 3;
			player.GetModPlayer<OriginPlayer>().explosiveSelfDamage -= 0.2f;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.TitaniumBar, 10);
			recipe.AddIngredient(ItemID.Obsidian, 20);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
			recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.AdamantiteBar, 10);
			recipe.AddIngredient(ItemID.Obsidian, 20);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
	}
}
