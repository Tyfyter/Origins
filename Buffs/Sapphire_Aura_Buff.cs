using Terraria;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Sapphire_Aura_Buff : ModBuff {
		public static int ID { get; private set; }
		public string[] Categories => [
			"GenericBoostBuff"
		];
		public override void SetStaticDefaults() {
			ID = Type;
			Main.buffNoTimeDisplay[Type] = true;
		}
		public override void Update(Player player, ref int buffIndex) {
			player.GetAttackSpeed(DamageClass.Generic) += 0.1f;
			player.GetAttackSpeed(DamageClass.Generic) *= 1.1f;
			player.lifeRegenCount += 10;
		}
	}
}
