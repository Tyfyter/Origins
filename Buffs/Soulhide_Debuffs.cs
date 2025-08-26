using Origins.NPCs;
using PegasusLib.UI;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Shadefire_Debuff : ModBuff {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			Buff_Hint_Handler.ModifyTip(Type, 7.5f);
			ID = Type;
		}
		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<OriginGlobalNPC>().shadeFire = true;
		}
	}
	public class Soulhide_Weakened_Debuff : ModBuff {
		public static int ID { get; private set; }
		public override string Texture => "Terraria/Images/Buff_153";
		public LocalizedText EffectDescription;
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			Buff_Hint_Handler.CombineBuffHintModifiers(Type, modifyBuffTip: (lines, item, player) => {
				lines.Add(Language.GetTextValue("Mods.PegasusLib.BuffTooltip.AffectDamage", -OriginGlobalNPC.soulhideWeakenAmount, DamageClass.Default.DisplayName));
			});
			ID = Type;
		}
		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<OriginGlobalNPC>().soulhideWeakenedDebuff = true;
		}
	}
}
