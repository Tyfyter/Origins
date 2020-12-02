using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace Origins.NPCs.ICARUS {
    public class Swarmer : ModNPC {
        byte frame = 0;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Swarmer");
            Main.npcFrameCount[npc.type] = 3;
        }
        public override void SetDefaults() {
            npc.CloneDefaults(NPCID.Bunny);
            npc.aiStyle = 55;
            npc.lifeMax = 80;
            npc.defense = 3;
            npc.damage = 15;
            npc.width = 18;
            npc.height = 18;
            npc.friendly = false;
            npc.FaceTarget();
            npc.spriteDirection = npc.direction;
        }

        public override void FindFrame(int frameHeight) {
            npc.frame = new Rectangle(0, 15*(frame&4)/2, 18, 18);
        }
    }
}
