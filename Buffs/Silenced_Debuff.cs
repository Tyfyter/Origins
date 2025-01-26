using Origins.NPCs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Silenced_Debuff : ModBuff {
		public override string Texture => "Terraria/Images/Buff_" + BuffID.Silenced;
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<OriginGlobalNPC>().silencedDebuff = true;
		}
	}
}
