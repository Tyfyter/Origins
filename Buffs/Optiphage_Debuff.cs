using Terraria;
using Terraria.ModLoader;

namespace Origins.Buffs {
    public class Optiphage_Debuff : ModBuff {
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			player.blind = true;
			player.lifeRegen -= 9;
			Main.LocalPlayer.GetModPlayer<OriginPlayer>().CorruptionAssimilation += 0.0001f;
		}
	}
}
