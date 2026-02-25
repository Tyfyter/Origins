using Origins.Liquids;

namespace Origins.Items.Tools.Liquids {
	public class Brine_Bucket : BucketBase<Brine> { }
	public class Brine_Bottomless_Bucket : Brine_Bucket {
		public override bool Endless => true;
	}
}
