using Origins.Liquids;
using Terraria;

namespace Origins.Items.Tools.Liquids {
	public class Oil_Bucket : BucketBase<Oil> {
		public override int GetLiquid(int x, int y) {
			Tile tile = Main.tile[x, y];
			if (tile.LiquidType == Burning_Oil.ID || y > Main.UnderworldLayer) return Burning_Oil.ID;
			return LiquidType;
		}
	}
	public class Oil_Bottomless_Bucket : Oil_Bucket {
		public override bool Endless => true;
	}
}
