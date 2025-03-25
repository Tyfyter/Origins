using Terraria;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Greater_Summoning_Buff : ModBuff {
		public static int ID { get; private set; }
		public string[] Categories => [
			"SummonBoostBuff"
		];
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			player.maxMinions += 2;
		}
	}
}
