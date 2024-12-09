using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Headphones_Buff : ModBuff {
		public static int ID { get; private set; }
		public string[] Categories => [
			"GenericBoostBuff"
		];
		public override void SetStaticDefaults() {
			BuffID.Sets.IsATagBuff[Type] = true;
			ID = Type;
		}
	}
}
