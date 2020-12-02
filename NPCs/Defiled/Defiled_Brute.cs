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
        byte frame = 0;
        byte anger = 0;
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
        public override bool PreAI() {
            npc.TargetClosest();
            npc.aiStyle = npc.HasPlayerTarget ? AIStyleID.Fighter : -1;
            if(((npc.Center-npc.targetRect.Center.ToVector2())*new Vector2(1,2)).Length()>480) {
                if(npc.life<npc.lifeMax) {
                    npc.aiStyle = 41;
                } else {
                    npc.target = 0;
                    npc.aiStyle = 3;
                }
            }
            if (npc.HasPlayerTarget) {
                npc.FaceTarget();
                npc.spriteDirection = npc.direction;
            }
            
            else if(anger == 1) {
                anger = 4;
            }
            return npc.aiStyle!=1;
        }
        public override void FindFrame(int frameHeight) {
            npc.frame = new Rectangle(0, 15*(frame&4)/2, 168, 168);
        }
        public override bool StrikeNPC(ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit) {
            anger = 6;
            return true;
        }
    }
}
