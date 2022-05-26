using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;

namespace Origins.NPCs.Riven {
    public class Flagellant : ModNPC {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Flagellant");
            Main.npcFrameCount[npc.type] = 4;
        }
        public override void SetDefaults() {
            npc.CloneDefaults(NPCID.BloodJelly);
            npc.lifeMax = 380;
            npc.defense = 20;
            npc.damage = 70;
            npc.width = 56;
            npc.height = 60;
            npc.frame.Height = 58;
        }
        public override void FindFrame(int frameHeight) {
		    npc.spriteDirection = npc.direction;
		    npc.frameCounter += 1.0;
		    if (npc.frameCounter >= 24.0){
			    npc.frameCounter = 0.0;
		    }
		    npc.frame.Y = 60 * (int)(npc.frameCounter / 6.0);
        }
    }
}
