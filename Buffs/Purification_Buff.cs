using System;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Purification_Buff : ModBuff {
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Willed");
			Description.SetDefault("The evils of the world cannot manipulate you");
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			player.buffImmune[ModContent.BuffType<Corrupt_Assimilation_Debuff>()] = true;
			player.buffImmune[ModContent.BuffType<Crimson_Assimilation_Debuff>()] = true;
			player.buffImmune[ModContent.BuffType<Defiled_Assimilation_Debuff>()] = true;
			player.buffImmune[ModContent.BuffType<Riven_Assimilation_Debuff>()] = true;

			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.cassimilationCurrent -= Math.Min(1f, 0f);
			originPlayer.crassimilationCurrent -= Math.Min(1f, 0f);
			originPlayer.dassimilationCurrent -= Math.Min(1f, 0f);
			originPlayer.rassimilationCurrent -= Math.Min(1f, 0f);
		}
	}
}
