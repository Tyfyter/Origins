using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Origins.NPCs;

namespace Origins.Buffs {
    public class Toxic_Shock_Debuff : ModBuff {
        public const int stun_duration = 4;
        public const int default_duration = 60;
        public static int ID { get; private set; } = -1;
        public override void SetDefaults() {
            DisplayName.SetDefault("Toxic Shock");
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
