using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class Void_Lock : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Void Lock");
			Tooltip.SetDefault("Can be used to lock some chests only unlockable by you");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.rare = ItemRarityID.White;
		}
	}
}