using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Fish {
	public class Amoeba_Buggy_Item : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Amoeba Buggy");
			Tooltip.SetDefault("'It won't stop saying ''Split apart, lost one''...'");
			SacrificeTotal = 5;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.FairyCritterBlue);
		}
		// TODO: Needs critter behavior
	}
	public class Bug_Item : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Bug");
			Tooltip.SetDefault("'Don't try to smash it, it'll make more problems'");
			SacrificeTotal = 5;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.GoldWaterStrider);
		}
		// TODO: Needs critter behavior
	}
	public class Cicada_3301_Item : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Cicada 3301");
			Tooltip.SetDefault("'It'll occasionally bite, and it will HURT'");
			SacrificeTotal = 5;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.FairyCritterBlue);
		}
		// TODO: Needs critter behavior
	}
}
