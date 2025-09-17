using Origins.NPCs;
using PegasusLib.UI;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Slow_Debuff : ModBuff {
		public override string Texture => "Terraria/Images/Buff_32";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			Buff_Hint_Handler.ModifyTip(Type, 0, "BuffDescription.Slow");
			ID = Type;
		}
		public override void Update(NPC npc, ref int buffIndex) {
			if (!npc.boss) npc.GetGlobalNPC<OriginGlobalNPC>().slowDebuff = true;
		}
	}
}
