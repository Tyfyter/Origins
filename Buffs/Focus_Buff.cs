using Terraria;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Focus_Buff : ModBuff {
		public static int ID { get; private set; } = -1;
		public string[] Categories => new string[] {
			"GenericBoostBuff"
		};
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			//mana logic + damage
		}
	}
}
