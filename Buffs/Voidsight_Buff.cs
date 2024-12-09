using Terraria;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Voidsight_Buff : ModBuff {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			player.nightVision = true;
			Lighting.AddLight(player.Center, 2.7f, 1.5f, 3f);
		}
	}
}
