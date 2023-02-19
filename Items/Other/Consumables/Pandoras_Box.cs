using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class Pandoras_Box : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Pandora's Box");
			Tooltip.SetDefault("'Unknown contents inside'");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ManaCrystal);
			Item.rare = ItemRarityID.Yellow;
			Item.UseSound = SoundID.Item139.WithPitch(0.2f);
			Item.maxStack = 1;
		}
	}
}
