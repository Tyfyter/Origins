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
    public class Splitooth : ModNPC, ITileCollideNPC {
        public static int id { get; private set; }
        public int CollisionType => NPCID.SandsharkCrimson;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Splitooth");
            Main.npcFrameCount[npc.type] = 4;
            id = npc.type;
        }
        public override void SetDefaults() {
            npc.CloneDefaults(NPCID.SandsharkCrimson);
            npc.width = 96;
            npc.lifeMax = 380;
            npc.defense = 20;
            npc.damage = 70;
            npc.gfxOffY = 10f;
        }
        public override void FindFrame(int frameHeight) {
		    npc.spriteDirection = npc.direction;
		    npc.frameCounter += 1.0;
		    if (npc.frameCounter >= 20.0){
			    npc.frameCounter = 0.0;
		    }
		    npc.frame.Y = 38 * (int)(npc.frameCounter / 5.0);
        }

        /*public override void HitEffect(int hitDirection, double damage) {
           if(npc.life<0) {
               for(int i = 0; i < 3; i++)Gore.NewGore(npc.position+new Vector2(Main.rand.Next(npc.width),Main.rand.Next(npc.height)), npc.velocity, mod.GetGoreSlot("Gores/NPCs/DF3_Gore"));
               for(int i = 0; i < 6; i++)Gore.NewGore(npc.position+new Vector2(Main.rand.Next(npc.width),Main.rand.Next(npc.height)), npc.velocity, mod.GetGoreSlot("Gores/NPCs/DF_Effect_Medium"+Main.rand.Next(1,4)));
           }
        }*/
    }
}
