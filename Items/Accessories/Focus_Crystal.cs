using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Balloon)]
	public class Focus_Crystal : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat"
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(28, 30);
			Item.rare = ItemRarityID.Yellow;
			Item.value = Item.sellPrice(gold: 7);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
            .AddIngredient(ItemID.ShinyStone)
            .AddIngredient(ModContent.ItemType<Ruby_Reticle>())
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.rubyReticle = true;
			originPlayer.focusCrystal = true;
		}
	}
}
