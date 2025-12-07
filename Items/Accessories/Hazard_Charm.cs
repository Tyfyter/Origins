using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Hazard_Charm : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Misc,
			WikiCategories.ExplosiveBoostAcc,
			WikiCategories.SelfDamageProtek
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 26);
			Item.value = Item.sellPrice(gold: 9, silver:50);
			Item.rare = ItemRarityID.Green;
		}
		public override void AddRecipes() {
			CreateRecipe()
            .AddIngredient(ItemID.LavaCharm)
            .AddIngredient(ModContent.ItemType<Bomb_Charm>())
			.AddIngredient(ModContent.ItemType<Trap_Charm>())
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.explosiveSelfDamage -= 0.2f;
			originPlayer.trapCharm = true;
			player.lavaMax += 7 * 60;
		}
	}
}
