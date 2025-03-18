using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	//[AutoloadEquip(EquipType.Shield)]
	public class Tiki_Buckler : ModItem {
		public override LocalizedText Tooltip => OriginExtensions.CombineTooltips(
				Language.GetOrRegister($"Mods.Origins.Items.{nameof(Akaliegis)}.Tooltip"),
				Language.GetOrRegister($"ItemTooltip.PygmyNecklace")
			);
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 34);
			Item.defense = 3;
			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.sellPrice(gold: 1);
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Akaliegis>()
			.AddIngredient(ItemID.PygmyNecklace)
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
		public override void UpdateEquip(Player player) {
			player.maxMinions += 1;
			player.OriginPlayer().akaliegis = true;
			Akaliegis.DoEnduranceBuff(player);
		}
	}
}
