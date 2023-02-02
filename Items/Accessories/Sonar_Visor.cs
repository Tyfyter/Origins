using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Face)]
	public class Sonar_Visor : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Sonar Visor");
			Tooltip.SetDefault("Increased projectile speed\nImmunity to Confusion\nAllows you to see tiles at a range");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaultsKeepSlots(ItemID.Aglet);
			Item.value = Item.sellPrice(gold: 7);
			Item.rare = ItemRarityID.Yellow;
		}
		public override void UpdateEquip(Player player) {
			//player.GetModPlayer<OriginPlayer>().sonarVisor = true;
			player.buffImmune[BuffID.Confused] = true;
		}
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.UltrabrightHelmet);
			recipe.AddIngredient(ModContent.ItemType<Advanced_Imaging>());
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}
	}
}
