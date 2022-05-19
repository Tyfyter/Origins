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
        public const int duration = 60;
        public static int ID { get; private set; } = -1;
        public override void SetDefaults() {
            DisplayName.SetDefault("Toxic Shock");
            ID = Type;
        }
		public override void Update(Player player, ref int buffIndex) {
            player.GetModPlayer<OriginPlayer>().toxicShock = true;
		}
		public override bool ReApply(NPC npc, int time, int buffIndex) {
            npc.GetGlobalNPC<OriginGlobalNPC>().toxicShockTime = 0;
			return false;
		}
	}
}
