using Terraria;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Fervor_Buff : ModBuff {
		public static int ID { get; private set; }
		public string[] Categories => [
			"GenericBoostBuff"
		];
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			player.GetAttackSpeed(DamageClass.Generic) += 0.1f;
		}
	}
}
