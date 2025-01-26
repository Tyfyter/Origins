using Origins.Dev;
using Origins.Water;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Tiles.Other {
	public class Defiled_Fountain : WaterFountainBase<Defiled_Water_Style> { }
	public class Defiled_Fountain_Item : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"WaterFountain"
		];
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Defiled_Fountain>());
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.buyPrice(gold: 4);
		}
	}
}