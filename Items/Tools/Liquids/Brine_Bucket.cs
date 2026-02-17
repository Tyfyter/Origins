using Origins.Liquids;

namespace Origins.Items.Tools.Liquids {
	public class Brine_Bucket : BucketBase<Brine> {
		public override string Texture => typeof(Oil_Bucket).GetDefaultTMLName();
	}
	public class Brine_Bottomless_Bucket : Brine_Bucket {
		public override string Texture => typeof(Oil_Bottomless_Bucket).GetDefaultTMLName();
		public override bool Endless => true;
	}
}
