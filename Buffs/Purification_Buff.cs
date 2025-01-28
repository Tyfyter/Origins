using System;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Purification_Buff : ModBuff {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			OriginPlayer originPlayer = player.OriginPlayer();
			foreach (AssimilationInfo info in originPlayer.IterateAssimilation()) {
				player.buffImmune[info.Type.Type] = true;
				info.Percent = 0;
			}
			originPlayer.tornSeverityDecayRate = 0.5f / 1f;
		}
	}
}
