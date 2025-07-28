using Terraria;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class No_Fallthrough_Buff : ModBuff {
		public override void Update(Player player, ref int buffIndex) {
			player.OriginPlayer().noFallThrough = true;
		}
	}
}
