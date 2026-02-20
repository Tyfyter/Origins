using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class Mojo_Harvest : Mojo_Flask {
		public override int FlaskUseCount => 0;
		public override LocalizedText Tooltip => this.GetLocalization(nameof(Tooltip));
		public override void SetDefaults() {
			base.SetDefaults();
			Item.buffTime = 30;
			Item.consumable = true;
			Item.maxStack = Item.CommonMaxStack;
		}
		public override bool? UseItem(Player player) {
			ApplyAssimilationHeal(player, true);
			return true;
		}
	}
}
