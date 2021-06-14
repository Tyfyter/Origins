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
    public class Riven_Mimic : ModNPC {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Riven Mimic");
            Main.npcFrameCount[npc.type] = 14;
        }
        public override void SetDefaults() {
            npc.CloneDefaults(NPCID.BigMimicCrimson);
        }
        public override void FindFrame(int frameHeight) {
            npc.CloneFrame(NPCID.BigMimicCrimson, frameHeight);
        }
        public override void HitEffect(int hitDirection, double damage) {
            //spawn gore if npc is dead after being hit
            if(npc.life<0) {
                for(int i = 0; i < 3; i++)Gore.NewGore(npc.position+new Vector2(Main.rand.Next(npc.width),Main.rand.Next(npc.height)), npc.velocity, mod.GetGoreSlot("Gores/NPCs/DF3_Gore"));
                for(int i = 0; i < 6; i++)Gore.NewGore(npc.position+new Vector2(Main.rand.Next(npc.width),Main.rand.Next(npc.height)), npc.velocity, mod.GetGoreSlot("Gores/NPCs/DF_Effect_Medium"+Main.rand.Next(1,4)));
            }
        }
    }
}
