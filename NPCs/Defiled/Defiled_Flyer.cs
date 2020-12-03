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
            npc.lifeRegen = 50;
        }
        public override void AI() {
            npc.FaceTarget();
            npc.spriteDirection = npc.direction;
            if(++npc.frameCounter>5) {
                npc.frame = new Rectangle(0, (npc.frame.Y+44)%176, 136, 44);
                npc.frameCounter = 0;
            }
            if (npc.life<npc.lifeMax) {
            }
        }
    }
}
