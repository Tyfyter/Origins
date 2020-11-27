/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Origins.NPCs;

namespace Origins.Buffs {
    public class ShrapnelDebuff : ModBuff {
        public override void SetDefaults() {
            DisplayName.SetDefault("Impeding Shrapnel");
            canBeCleared = false;
            Main.persistentBuff[Type] = true;
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }
        public override bool Autoload(ref string name, ref string texture) {
            texture = "Terraria/Buff_84";
            return true;
        }
        public override void Update(NPC npc, ref int buffIndex) {
            int count = npc.GetGlobalNPC<OriginInstancedGlobalNPC>().shrapnelCount;
            if(count>2) {

            }
        }
    }
}*/
