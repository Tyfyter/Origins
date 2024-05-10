using Terraria;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Purification_Buff : ModBuff {
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			player.buffImmune[ModContent.BuffType<Corrupt_Assimilation_Debuff>()] = true;
			player.buffImmune[ModContent.BuffType<Crimson_Assimilation_Debuff>()] = true;
			player.buffImmune[ModContent.BuffType<Defiled_Assimilation_Debuff>()] = true;
			player.buffImmune[ModContent.BuffType<Riven_Assimilation_Debuff>()] = true;

			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.CorruptionAssimilation = 0;
			originPlayer.CrimsonAssimilation = 0;
			originPlayer.DefiledAssimilation = 0;
			originPlayer.RivenAssimilation = 0;
		}
	}
}
