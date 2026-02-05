using ModLiquidLib.ModLoader;
using Origins.Liquids;

namespace Origins.Items.Tools.Liquids {
	public class OilBucket : BucketBase {
		public override int LiquidType => LiquidLoader.LiquidType<Oil>();
	}
}
