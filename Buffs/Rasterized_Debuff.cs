using PegasusLib.UI;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Rasterized_Debuff : ModBuff {
		public const int duration = 24;
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			Buff_Hint_Handler.ModifyTip(Type, 0, this.GetLocalization("EffectDescription").Key);
			ID = Type;
		}
	}
}
