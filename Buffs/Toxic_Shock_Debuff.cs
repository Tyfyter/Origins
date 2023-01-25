using Origins.NPCs;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Buffs {
    public class Toxic_Shock_Debuff : ModBuff {
        public const int stun_duration = 4;
        public const int default_duration = 60;
        public static int ID { get; private set; } = -1;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Toxic Shock");
            Description.SetDefault("The agony is getting to you.");
            ID = Type;
        }
		public override void Update(Player player, ref int buffIndex) {
            player.GetModPlayer<OriginPlayer>().toxicShock = true;
		}
		public override bool ReApply(NPC npc, int time, int buffIndex) {
            OriginGlobalNPC globalNPC = npc.GetGlobalNPC<OriginGlobalNPC>();
            if (globalNPC.toxicShockTime > Toxic_Shock_Debuff.stun_duration * 3) {
                globalNPC.toxicShockTime = 0;
            }
            return false;
		}
	}
}
