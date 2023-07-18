using Terraria;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Antisolve_Buff : ModBuff {
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Unsolveable");
			// Description.SetDefault("Cannot be broken down by any chemicals");
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			player.buffImmune[Toxic_Shock_Debuff.ID] = true;
			player.buffImmune[Torn_Debuff.ID] = true;
		}
	}
}
