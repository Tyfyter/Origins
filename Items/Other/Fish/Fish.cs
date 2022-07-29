using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Fish {
	public class Prikish : ModItem {
		public override void SetStaticDefaults() {
			SacrificeTotal = 3;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Ebonkoi);
			Item.rare = ItemRarityID.Quest;
		}
	}
}
