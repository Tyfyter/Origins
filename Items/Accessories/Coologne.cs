using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Coologne : ModItem, ICustomWikiStat {
		public string[] Categories => new string[] {
			"Combat"
		};
		public override void SetDefaults() {
			Item.DefaultToAccessory(22, 32);
			Item.rare = ItemRarityID.Pink;
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
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.FrozenTurtleShell);
			recipe.AddIngredient(ModContent.ItemType<Mysterious_Spray>());
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}
	}
}
