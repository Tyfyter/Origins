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
            npc.width = 104;
            npc.height = 38;
            npc.friendly = false;
            npc.lifeRegen = 50;
        }
        public override void AI() {
            npc.FaceTarget();
            if(!npc.HasValidTarget)npc.direction = Math.Sign(npc.velocity.X);
            npc.spriteDirection = npc.direction;
            if(++npc.frameCounter>5) {
                npc.frame = new Rectangle(0, (npc.frame.Y+38)%152, 104, 36);
                npc.frameCounter = 0;
            }
        }
        public override void HitEffect(int hitDirection, double damage) {
            if(npc.life<0) {
                for(int i = 0; i < 2; i++)Gore.NewGore(npc.position+new Vector2(Main.rand.Next(npc.width),Main.rand.Next(npc.height)), npc.velocity, mod.GetGoreSlot("Gores/NPCs/DF3_Gore"));
                for(int i = 0; i < 6; i++)Gore.NewGore(npc.position+new Vector2(Main.rand.Next(npc.width),Main.rand.Next(npc.height)), npc.velocity, mod.GetGoreSlot("Gores/NPCs/DF_Effect_Medium"+Main.rand.Next(1,4)));
            }
        }
    }
}
