using Origins.Dev;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Back)]
	public class Solar_Panel : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Vitality"
		];
		public static int BackSlot { get; private set; }
		public override void SetStaticDefaults() {
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
			CreateRecipe()
			.AddIngredient(ItemID.BandofStarpower)
			.AddIngredient(ItemID.SunStone)
			.AddIngredient(ModContent.ItemType<Silicon_Bar>(), 8)
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
}
