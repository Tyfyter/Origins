using Terraria;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Skimswim_Buff : ModBuff {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
	}
}
