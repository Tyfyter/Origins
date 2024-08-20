using Origins.Dev;
using Terraria;

namespace Origins.Items.Other.Dyes {
	public class High_Contrast_Dye : Dye_Item, ICustomWikiStat {
		public override bool UseShaderOnSelf => false;
		public string[] Categories => [
			"SpecialEffectDye"
		];
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 3;
		}
	}
}
