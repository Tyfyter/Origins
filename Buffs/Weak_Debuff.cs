using Origins.NPCs;
using PegasusLib.UI;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Weak_Debuff : ModBuff {
		public override string Texture => $"Terraria/Images/Buff_{BuffID.Weak}";
		public override LocalizedText DisplayName => Language.GetText("BuffName.Weak");
		public override LocalizedText Description => Language.GetText("BuffDescription.Weak");
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			Buff_Hint_Handler.ModifyTip(Type, 0, "BuffDescription.Weak");
			ID = Type;
		}
		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<OriginGlobalNPC>().weakDebuff = true;
		}
	}
}
