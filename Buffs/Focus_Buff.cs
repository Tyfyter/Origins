using Terraria;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Focus_Buff : ModBuff {
		public static int ID { get; private set; }
		public string[] Categories => [
			"GenericBoostBuff"
		];
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			player.GetModPlayer<OriginPlayer>().focusPotion = true;
		}
	}
}
