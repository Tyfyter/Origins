using Origins.NPCs;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Shadefire_Debuff : ModBuff {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			ID = Type;
		}
		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<OriginGlobalNPC>().shadeFire = true;
		}
	}
	public class Soulhide_Weakened_Debuff : ModBuff {
		public static int ID { get; private set; }
		public override string Texture => "Terraria/Images/Buff_153";
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			ID = Type;
		}
		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<OriginGlobalNPC>().soulhideWeakenedDebuff = true;
		}
	}
}
