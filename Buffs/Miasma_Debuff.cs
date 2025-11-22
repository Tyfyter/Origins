using Terraria;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Miasma_Debuff : ModBuff {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			player.OriginPlayer().miasma = true;
		}
	}
}
