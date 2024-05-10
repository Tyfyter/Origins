using Origins.NPCs;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Weak_Shadowflame_Debuff : ModBuff {
		public static int ID { get; private set; } = -1;
		public override string Texture => "Terraria/Images/Buff_153";
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<OriginGlobalNPC>().weakShadowflameDebuff = true;
		}
	}
	public class Soulhide_Weakened_Debuff : ModBuff {
		public static int ID { get; private set; } = -1;
		public override string Texture => "Terraria/Images/Buff_153";
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<OriginGlobalNPC>().soulhideWeakenedDebuff = true;
		}
	}
}
