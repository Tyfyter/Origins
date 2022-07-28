using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Fish {
	public class Crusty_Crate : ModItem {
		static short glowmask;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Crusty Crate");
			Tooltip.SetDefault("'No, this is Patrick.'");
			glowmask = Origins.AddGlowMask(this);
			SacrificeTotal = 5;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.CrimsonFishingCrate);
			Item.createTile = -1;
			Item.glowMask = glowmask;
		}
		///TODO: add ModifyItemLoot once that exists
	}
}
