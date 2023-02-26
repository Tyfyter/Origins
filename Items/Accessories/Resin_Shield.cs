using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Shield)]
	public class Resin_Shield : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Resin Shield");
			Tooltip.SetDefault("Blocks all self-damage on next hit\n10 second cooldown\n'A shield to withstand the test of time'");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(36, 38);
			Item.defense = 3;
			Item.value = Item.sellPrice(gold: 4);
			Item.rare = ItemRarityID.Pink;
		}
		public override void UpdateEquip(Player player) {
			player.noKnockback = true;
			player.fireWalk = true;
			//player.GetModPlayer<OriginPlayer>().resinShield = true;
		}
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.Amber, 6);
			recipe.AddIngredient(ItemID.ObsidianShield);
			//recipe.AddIngredient(ModContent.ItemType<Carburite>(), 12);
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}
	}
}
