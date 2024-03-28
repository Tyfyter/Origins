using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Blast_Resistant_Plate : ModItem, ICustomWikiStat {
		public string[] Categories => new string[] {
			"Vitality",
			"ExplosiveBoostAcc"
		};
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 28);
			Item.defense = 3;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().explosiveSelfDamage -= 0.15f;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
            recipe.AddRecipeGroup("AdamantiteBars", 10);
            recipe.AddIngredient(ItemID.Obsidian, 20);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
	}
}
