using Origins.Dev;
using Origins.Water;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Tiles.Other {
	public class Brine_Fountain : WaterFountainBase<Brine_Water_Style> {
		public override int Height => 3;
		public override int Frames => 5;
	}
	public class Brine_Fountain_Item : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"WaterFountain"
		];
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Brine_Fountain>());
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.buyPrice(gold: 4);
		}
	}
}