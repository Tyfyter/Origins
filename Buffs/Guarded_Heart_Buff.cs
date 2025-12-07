using Terraria;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Guarded_Heart_Buff : ModBuff {
		public static int ID { get; private set; }
		public string[] Categories => [
			"GenericBoostBuff"
		];
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			player.moveSpeed *= 1.5f;
			player.GetDamage(DamageClass.Generic) *= 1.2f;
		}
	}
}
