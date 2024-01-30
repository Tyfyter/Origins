using Origins.NPCs;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Slow_Debuff : ModBuff {
		public override string Texture => "Terraria/Images/Buff_32";
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<OriginGlobalNPC>().slowDebuff = true;
		}
	}
}
