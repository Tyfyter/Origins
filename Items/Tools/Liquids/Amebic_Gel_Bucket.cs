using Origins.Liquids;

namespace Origins.Items.Tools.Liquids {
	public class Amebic_Gel_Bottomless_Bucket : BucketBase<Amebic_Gel> {
		public override string Texture => typeof(Brine_Bottomless_Bucket).GetDefaultTMLName();
		public override bool Endless => true;
	}
}
