using Origins.Dev;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    [AutoloadEquip(EquipType.Back)]
	public class Solar_Panel : ModItem, ICustomWikiStat {
		public string[] Categories => new string[] {
			"Resource"
		};
		public static int BackSlot { get; private set; }
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Solar Panel");
			// Tooltip.SetDefault("Sunlight exposure increases mana regeneration\n'Don't worry, mana was always renewable!'");
			BackSlot = Item.backSlot;
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 34);
			Item.rare = ItemRarityID.Lime;
			Item.value = Item.sellPrice(gold: 6);
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().solarPanel = true;
		}
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.BandofStarpower);
			recipe.AddIngredient(ItemID.SunStone);
			recipe.AddIngredient(ModContent.ItemType<Silicon>(), 8);
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}
	}
}
