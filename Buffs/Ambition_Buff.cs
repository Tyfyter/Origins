using Origins.Dev;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Ambition_Buff : ModBuff {
		public static int ID { get; private set; }
		public string[] Categories => [
			WikiCategories.GenericBoostBuff
		];
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			player.GetArmorPenetration(DamageClass.Generic) += 7;
		}
	}
}
