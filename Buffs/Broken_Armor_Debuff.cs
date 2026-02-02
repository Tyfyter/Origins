using Origins.NPCs;
using PegasusLib.UI;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Buffs {
	// TODO: remove when tml 1.4.5 releases
	public class Broken_Armor_Debuff : ModBuff {
		public override string Texture => $"Terraria/Images/Buff_{BuffID.BrokenArmor}";
		public override LocalizedText DisplayName => Language.GetText("BuffName.BrokenArmor");
		public override LocalizedText Description => Language.GetText("BuffDescription.BrokenArmor");
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			Buff_Hint_Handler.ModifyTip(Type, 0, "Mods.Origins.Buffs.Broken_Armor_Debuff.EffectDescription");
			ID = Type;
		}
		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<OriginGlobalNPC>().brokenArmorDebuff = true;
		}
	}
}
