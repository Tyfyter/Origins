using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Rasterized_Debuff : ModBuff {
		public const int duration = 24;
		public static int ID { get; private set; }
		public LocalizedText EffectDescription;
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			EffectDescription = this.GetLocalization(nameof(EffectDescription));
			OriginsSets.Buffs.BuffHintModifiers[Type] = (null, (lines, _) => lines.Add(EffectDescription.Value));
			ID = Type;
		}
	}
}
