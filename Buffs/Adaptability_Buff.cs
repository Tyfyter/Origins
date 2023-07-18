using Terraria;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Adaptability_Buff : ModBuff {
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Protean");
			// Description.SetDefault("Weapon boosts are shared to all classes!");
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			player.GetModPlayer<OriginPlayer>().statSharePercent += 0.5f;
		}
	}
}
