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
    public class Riven_Fighter : ModNPC {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Riven Acolyte");
            Main.npcFrameCount[npc.type] = 5;
        }
        public override void SetDefaults() {
            npc.CloneDefaults(NPCID.Zombie);
            npc.aiStyle = NPCAIStyleID.Fighter;
            npc.lifeMax = 110;
            npc.defense = 8;
            npc.damage = 42;
            npc.width = 36;
            npc.height = 40;
            npc.friendly = false;
        }
        public override void AI() {
            npc.TargetClosest();
            if (npc.HasPlayerTarget) {
                npc.FaceTarget();
                npc.spriteDirection = npc.direction;
            }
            //increment frameCounter every frame and run the following code when it exceeds 7 (i.e. run the following code every 8 frames)
			if(npc.collideY && ++npc.frameCounter>7) {
				//add frame height (with buffer) to frame y position and modulo by frame height (with buffer) multiplied by walking frame count
				npc.frame = new Rectangle(0, (npc.frame.Y+40)%160, 36, 40);
                //reset frameCounter so this doesn't trigger every frame after the first time
				npc.frameCounter = 0;
			}
        }
        public override void HitEffect(int hitDirection, double damage) {
            //spawn gore if npc is dead after being hit
            if(npc.life<0) {
                for(int i = 0; i < 3; i++)Gore.NewGore(npc.position+new Vector2(Main.rand.Next(npc.width),Main.rand.Next(npc.height)), npc.velocity, mod.GetGoreSlot("Gores/NPCs/DF3_Gore"));
                for(int i = 0; i < 6; i++)Gore.NewGore(npc.position+new Vector2(Main.rand.Next(npc.width),Main.rand.Next(npc.height)), npc.velocity, mod.GetGoreSlot("Gores/NPCs/DF_Effect_Medium"+Main.rand.Next(1,4)));
            }
		    npc.frame = new Rectangle(0, 160, 36, 40);
		    npc.frameCounter = 0;
        }
    }
}
