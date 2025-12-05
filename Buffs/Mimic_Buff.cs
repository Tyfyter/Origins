using Terraria;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Mimic_Buff : ModBuff {
		public override string Texture => "Terraria/Images/Buff_160";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			player.GetAttackSpeed(DamageClass.Generic) += 0.1f;
			player.GetAttackSpeed(DamageClass.Generic) *= 1.15f;
			player.GetCritChance(DamageClass.Generic) += 15;
			player.lifeRegen += 20;
		}
	}
}
