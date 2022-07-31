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
using Terraria.GameContent.ItemDropRules;
using Origins.Items.Other.Consumables;

namespace Origins.NPCs.Riven {
    public class Flagellant : ModNPC {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Flagellant");
            Main.npcFrameCount[NPC.type] = 4;
        }
        public override void SetDefaults() {
            NPC.CloneDefaults(NPCID.BloodJelly);
            NPC.lifeMax = 380;
            NPC.defense = 20;
            NPC.damage = 70;
            NPC.width = 56;
            NPC.height = 60;
            NPC.frame.Height = 58;
        }
        public override void FindFrame(int frameHeight) {
		    NPC.spriteDirection = NPC.direction;
		    NPC.frameCounter += 1.0;
		    if (NPC.frameCounter >= 24.0){
			    NPC.frameCounter = 0.0;
		    }
		    NPC.frame.Y = 60 * (int)(NPC.frameCounter / 6.0);
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot) {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Jam_Sandwich>(), 17));
        }
    }
}
