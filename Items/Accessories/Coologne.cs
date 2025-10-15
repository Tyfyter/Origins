using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Coologne : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Vitality
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(22, 32);
			Item.rare = ItemRarityID.Pink;
			Item.master = true;
			Item.value = Item.sellPrice(gold: 10);
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			if (player.statLife <= player.statLifeMax2 * 0.5f) player.AddBuff(BuffID.IceBarrier, 5);
			Mysterious_Spray.EquippedEffect(player);
			if (!hideVisual) {
				Mysterious_Spray.VanityEffect(player);
			}
		}
		public override void UpdateVanity(Player player) {
			Mysterious_Spray.VanityEffect(player);
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.FrozenTurtleShell)
			.AddIngredient(ModContent.ItemType<Mysterious_Spray>())
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
}
