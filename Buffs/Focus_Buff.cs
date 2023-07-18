using Terraria;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Focus_Buff : ModBuff {
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Mana Channeling");
			// Description.SetDefault("Your mana is being used to increase damage");
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			//mana logic + damage
		}
	}
}
