using Origins.Dev;
using Terraria;
using Terraria.ID;

namespace Origins.Items.Other.Dyes {
	public class High_Contrast_Dye : Dye_Item, ICustomWikiStat {
		public override bool UseShaderOnSelf => false;
		public string[] Categories => [
			WikiCategories.SpecialEffectDye
		];
		public override void SetStaticDefaults() {
			ItemID.Sets.NonColorfulDyeItems.Add(Type);
			Item.ResearchUnlockCount = 3;
		}
	}
}
