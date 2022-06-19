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

namespace Origins.NPCs.Defiled {
    public class Shattered_Mummy : ModNPC {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Shattered Mummy");
            Main.npcFrameCount[NPC.type] = 4;
        }
        public override void SetDefaults() {
            NPC.CloneDefaults(NPCID.Zombie);
            NPC.aiStyle = NPCAIStyleID.Fighter;
            NPC.lifeMax = 180;
            NPC.defense = 18;
            NPC.knockBackResist = 0.5f;
            NPC.damage = 60;
            NPC.width = 32;
            NPC.height = 48;
            NPC.value = 700;
            NPC.friendly = false;
            NPC.HitSound = Origins.Sounds.DefiledHurt1;
            NPC.DeathSound = Origins.Sounds.DefiledKill;
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot) {
            npcLoot.Add(ItemDropRule.Common(ItemID.DarkShard, 10));
            npcLoot.Add(ItemDropRule.Common(ItemID.Megaphone, 100));
            npcLoot.Add(ItemDropRule.Common(ItemID.Blindfold, 100));
            npcLoot.Add(ItemDropRule.Common(ItemID.MummyMask, 75));
            npcLoot.Add(ItemDropRule.Common(ItemID.MummyShirt, 75));
            npcLoot.Add(ItemDropRule.Common(ItemID.MummyPants, 75));
        }
        public override void AI() {
            NPC.TargetClosest();
            if (NPC.HasPlayerTarget) {
                NPC.FaceTarget();
                NPC.spriteDirection = NPC.direction;
            }
            //increment frameCounter every frame and run the following code when it exceeds 7 (i.e. run the following code every 8 frames)
			if(++NPC.frameCounter>7) {
				//add frame height (with buffer) to frame y position and modulo by frame height (with buffer) multiplied by walking frame count
				NPC.frame = new Rectangle(0, (NPC.frame.Y+50)%200, 32, 48);
                //reset frameCounter so this doesn't trigger every frame after the first time
				NPC.frameCounter = 0;
			}
        }
        public override void HitEffect(int hitDirection, double damage) {
            //spawn gore if npc is dead after being hit
            if(NPC.life<0) {
                for(int i = 0; i < 3; i++)Gore.NewGore(NPC.GetSource_Death(), NPC.position+new Vector2(Main.rand.Next(NPC.width),Main.rand.Next(NPC.height)), NPC.velocity, Mod.GetGoreSlot("Gores/NPCs/DF3_Gore"));
                for(int i = 0; i < 6; i++)Gore.NewGore(NPC.GetSource_Death(), NPC.position+new Vector2(Main.rand.Next(NPC.width),Main.rand.Next(NPC.height)), NPC.velocity, Mod.GetGoreSlot("Gores/NPCs/DF_Effect_Medium"+Main.rand.Next(1,4)));
            }
        }
    }
}
