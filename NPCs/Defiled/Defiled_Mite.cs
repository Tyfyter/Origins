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
    public class Defiled_Mite : ModNPC {
        byte frame = 0;
        byte anger = 0;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Defiled Mite");
            Main.npcFrameCount[npc.type] = 2;
        }
        public override void SetDefaults() {
            npc.CloneDefaults(NPCID.Bunny);
            npc.aiStyle = -1;
            npc.lifeMax = 22;
            npc.defense = 6;
            npc.damage = 34;
            npc.width = 32;
            npc.height = 28;
            npc.friendly = false;
        }
        public override bool PreAI() {
            npc.TargetClosest();
            npc.aiStyle = npc.HasPlayerTarget ? AIStyleID.Fighter : -1;
            if(((npc.Center-npc.targetRect.Center.ToVector2())*new Vector2(1,2)).Length()>480) {
                if(npc.life<npc.lifeMax) {
                    npc.aiStyle = AIStyleID.Tortoise;
                } else {
                    npc.target = -1;
                    npc.aiStyle = -1;
                }
            }
            npc.rotation = 0;
            if(npc.HasPlayerTarget) {
                npc.FaceTarget();
                npc.spriteDirection = npc.direction;
            }
            if(npc.collideY) {
                if(anger!=0) {
                    if(anger>1)anger--;
                    npc.aiStyle = AIStyleID.Tortoise;
                }else if(npc.aiStyle==-1) {
                    npc.velocity.X*=0.85f;
                } else if(npc.aiStyle==AIStyleID.Fighter){
                    frame = (byte)((frame+1)&7);
                }
            }else if(anger == 1) {
                anger = 0;
            }
            return npc.aiStyle!=-1;
        }
        public override void FindFrame(int frameHeight) {
            npc.frame = new Rectangle(0, 15*(frame&4)/2, 32, 30);
        }
        public override bool StrikeNPC(ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit) {
            anger = 6;
            return true;
        }
    }
}
