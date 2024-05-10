using Terraria;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Ambition_Buff : ModBuff {
		public static int ID { get; private set; } = -1;
		public string[] Categories => new string[] {
			"GenericBoostBuff"
		};
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			player.GetDamage(DamageClass.Generic) += 0.025f;
			player.GetCritChance(DamageClass.Generic) += 0.025f;
			player.GetAttackSpeed(DamageClass.Generic) += 0.025f;
			player.GetModPlayer<OriginPlayer>().statSharePercent += 0.125f;
		}
	}
}
