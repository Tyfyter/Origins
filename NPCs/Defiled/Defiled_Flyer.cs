using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace Origins.NPCs.Defiled {
    public class Defiled_Flyer : ModNPC {
        byte frame = 0;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Defiled Phantom");
            Main.npcFrameCount[npc.type] = 4;
        }
        public override void SetDefaults() {
            npc.CloneDefaults(NPCID.Bunny);
            npc.aiStyle = 14;
            npc.lifeMax = 40;
            npc.defense = 8;
            npc.damage = 20;
            npc.width = 136;
            npc.height = 44;
            npc.friendly = false;
            npc.FaceTarget();
            npc.spriteDirection = npc.direction;
        }

        public override void FindFrame(int frameHeight) {
            npc.frame = new Rectangle(0, 15*(frame&4)/2, 136, 44);
        }
    }
}
