using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Face)]
	public class Sonar_Visor : ModItem, ICustomWikiStat {
		public string[] Categories => new string[] {
			"Combat",
			"Info"
		};
		public override void SetDefaults() {
			Item.DefaultToAccessory(28, 20);
			Item.value = Item.sellPrice(gold: 7);
			Item.rare = ItemRarityID.Yellow;
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.advancedImaging = true;
			originPlayer.sonarVisor = true;
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
