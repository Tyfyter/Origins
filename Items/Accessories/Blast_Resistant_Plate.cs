using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Blast_Resistant_Plate : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Vitality",
			"ExplosiveBoostAcc",
			"SelfDamageProtek"
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 28);
			Item.defense = 3;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().explosiveSelfDamage -= 0.2f;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
            .AddRecipeGroup("AdamantiteBars", 10)
            .AddIngredient(ItemID.Obsidian, 20)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
}
