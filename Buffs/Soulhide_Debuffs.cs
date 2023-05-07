using Origins.NPCs;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Weak_Shadowflame_Debuff : ModBuff {
		public static int ID { get; private set; } = -1;
		public override string Texture => "Terraria/Images/Buff_153";
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Dark Flames");
			Description.SetDefault("Losing life (but half as strong as the normal kind since it'd be kinda OP to get full hardmode debuff DPS from a prehardmode set)");
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
			DisplayName.SetDefault("Weakened");
			Description.SetDefault("Damage reduced by 15%");
			ID = Type;
		}
		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<OriginGlobalNPC>().soulhideWeakenedDebuff = true;
		}
	}
}
