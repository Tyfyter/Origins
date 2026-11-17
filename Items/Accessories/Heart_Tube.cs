using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Heart_Tube : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Vitality
		];
		public override void SetStaticDefaults() => Origins.AddGlowMask(this);
		public override void SetDefaults() {
			Item.DefaultToAccessory(24, 36);
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(gold: 8);
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			OriginPlayer oP = player.OriginPlayer();
			oP.pacemaker = true;
			if (!oP.pacemakerActive)
				player.endurance += (1 - player.endurance) * 0.25f;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.FrozenTurtleShell)
			.AddIngredient<Pacemaker>()
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
}
