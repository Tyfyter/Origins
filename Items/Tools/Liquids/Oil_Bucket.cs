using ModLiquidLib.ModLoader;
using Origins.Liquids;

namespace Origins.Items.Tools.Liquids {
	public class Oil_Bucket : BucketBase {
		public override int LiquidType => LiquidLoader.LiquidType<Oil>();
	}
	public class Oil_Bottomless_Bucket : Oil_Bucket {
		public override bool Endless => true;
	}
}
