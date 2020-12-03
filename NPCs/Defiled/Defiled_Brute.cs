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
    public class Defiled_Brute : ModNPC {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Defiled Krusher");
            Main.npcFrameCount[npc.type] = 7;
        }
        public override void SetDefaults() {
            npc.CloneDefaults(NPCID.Zombie);
            npc.aiStyle = 3;
            npc.lifeMax = 160;
            npc.defense = 9;
            npc.damage = 49;
            npc.width = 124;
            npc.height = 108;
            npc.friendly = false;
        }
        public override void AI() {
            npc.TargetClosest();
            npc.aiStyle = AIStyleID.Fighter;
            if(((npc.Center-npc.targetRect.Center.ToVector2())*new Vector2(1,2)).Length()>480) {
                if(npc.life<npc.lifeMax) {
                    npc.aiStyle = 41;
                }/* else {
                    npc.target = 0;
                    npc.aiStyle = 41;
                }*/
            }
            if (npc.HasPlayerTarget) {
                npc.FaceTarget();
                npc.spriteDirection = npc.direction;
            }
            if(++npc.frameCounter>7) {
                //add frame height to frame y position and modulo by frame height multiplied by walking frame count
                npc.frame = new Rectangle(0, (npc.frame.Y+170)%680, 182, 170);
                npc.frameCounter = 0;
            }
        }
        public override void HitEffect(int hitDirection, double damage) {
            if(npc.life<0) {
                for(int i = 0; i < 6; i++)Gore.NewGore(npc.position+new Vector2(Main.rand.Next(npc.width),Main.rand.Next(npc.height)), npc.velocity, mod.GetGoreSlot("Gores/NPCs/DF3_Gore"));
                for(int i = 0; i < 10; i++)Gore.NewGore(npc.position+new Vector2(Main.rand.Next(npc.width),Main.rand.Next(npc.height)), npc.velocity, mod.GetGoreSlot("Gores/NPCs/DF_Effect_Medium"+Main.rand.Next(1,4)));
            }
        }
    }
}
