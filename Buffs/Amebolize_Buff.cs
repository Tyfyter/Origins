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
    public class Amebolize_Buff : ModBuff {
		public override string Texture => "Terraria/Images/Buff_160";
		public static int ID { get; private set; } = -1;
        public override void SetStaticDefaults() {
            BuffID.Sets.IsAnNPCWhipDebuff[Type] = true;
            ID = Type;
        }
		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<OriginGlobalNPC>().amebolizeDebuff = true;
		}
	}
}
