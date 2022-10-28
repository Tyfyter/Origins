using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Origins.NPCs;

namespace Origins.Buffs {
    public class Slow_Debuff : ModBuff {
		public override string Texture => "Terraria/Images/Buff_32";
		public static int ID { get; private set; } = -1;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Slow");
            Description.SetDefault("This is the enemy version of the debuff, if you get this just tap the movement keys so you move slower or something, ¯\\_(ツ)_/¯");
            ID = Type;
        }
        public override void Update(NPC npc, ref int buffIndex) {
            npc.GetGlobalNPC<OriginGlobalNPC>().slowDebuff = true;
        }
    }
}
