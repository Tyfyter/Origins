using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Origins.NPCs;
using Terraria.ID;

namespace Origins.Buffs {
    public class Torn_Buff : ModBuff {
		public static int ID { get; private set; } = -1;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Torn");
            Main.debuff[Type] = true;
            ID = Type;
        }
		public override void Update(Player player, ref int buffIndex) {
            player.GetModPlayer<OriginPlayer>().tornDebuff = true;
		}
		public override void Update(NPC npc, ref int buffIndex) {
            npc.GetGlobalNPC<OriginGlobalNPC>().tornDebuff = true;
        }
	}
}
