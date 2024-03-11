using Terraria;

namespace Origins.Items.Other.Dyes {
	public class High_Contrast_Dye : Dye_Item {
		public override bool UseShaderOnSelf => false;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("High-Contrast Dye");
			Item.ResearchUnlockCount = 3;
		}
	}
}
