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
using Origins.Tiles.Riven;

namespace Origins.NPCs.Riven {
    public class Splitooth : ModNPC, ISandsharkNPC {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Splitooth");
            Main.npcFrameCount[NPC.type] = 4;
        }
        public override void SetDefaults() {
            NPC.CloneDefaults(NPCID.SandsharkCrimson);
            NPC.width = 96;
            NPC.lifeMax = 380;
            NPC.defense = 20;
            NPC.damage = 70;
            NPC.gfxOffY = 10f;
        }
        public void PreUpdateCollision() {
            TileID.Sets.ForAdvancedCollision.ForSandshark[ModContent.TileType<Weak_Riven_Flesh>()] = true;
            TileID.Sets.ForAdvancedCollision.ForSandshark[ModContent.TileType<Riven_Flesh>()] = true;
        }
        public void PostUpdateCollision() {
            TileID.Sets.ForAdvancedCollision.ForSandshark[ModContent.TileType<Weak_Riven_Flesh>()] = false;
            TileID.Sets.ForAdvancedCollision.ForSandshark[ModContent.TileType<Riven_Flesh>()] = false;
        }
        public override bool PreAI() {
            TileID.Sets.Conversion.Sand[ModContent.TileType<Weak_Riven_Flesh>()] = true;
            TileID.Sets.Conversion.Sand[ModContent.TileType<Riven_Flesh>()] = true;
            return true;
        }
        public override void PostAI() {
            TileID.Sets.Conversion.Sand[ModContent.TileType<Weak_Riven_Flesh>()] = false;
            TileID.Sets.Conversion.Sand[ModContent.TileType<Riven_Flesh>()] = false;
        }
        public override void FindFrame(int frameHeight) {
		    NPC.spriteDirection = NPC.direction;
		    NPC.frameCounter += 1.0;
		    if (NPC.frameCounter >= 20.0){
			    NPC.frameCounter = 0.0;
		    }
		    NPC.frame.Y = 38 * (int)(NPC.frameCounter / 5.0);
        }

        /*public override void HitEffect(int hitDirection, double damage) {
           if(npc.life<0) {
               for(int i = 0; i < 3; i++)Gore.NewGore(npc.position+new Vector2(Main.rand.Next(npc.width),Main.rand.Next(npc.height)), npc.velocity, mod.GetGoreSlot("Gores/NPCs/DF3_Gore"));
               for(int i = 0; i < 6; i++)Gore.NewGore(npc.position+new Vector2(Main.rand.Next(npc.width),Main.rand.Next(npc.height)), npc.velocity, mod.GetGoreSlot("Gores/NPCs/DF_Effect_Medium"+Main.rand.Next(1,4)));
           }
        }*/
    }
}
