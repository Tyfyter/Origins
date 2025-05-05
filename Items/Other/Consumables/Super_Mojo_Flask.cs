using Terraria;

namespace Origins.Items.Other.Consumables {
	public class Super_Mojo_Flask : Mojo_Flask {
		public override int FlaskUseCount => 7;
		public override void SetDefaults() {
			base.SetDefaults();
			Item.buffTime = 25;
		}
	}
}
