using Terraria;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Adaptability_Buff : ModBuff {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			player.GetModPlayer<OriginPlayer>().statSharePercent += 0.5f;
		}
	}
}
