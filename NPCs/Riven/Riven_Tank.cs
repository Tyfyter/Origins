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
    public class Riven_Tank : ModNPC, IMeleeCollisionDataNPC {
        public const float speedMult = 0.75f;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Riven Giant");
            Main.npcFrameCount[NPC.type] = 4;
        }
        public override void SetDefaults() {
            NPC.CloneDefaults(NPCID.Zombie);
            NPC.aiStyle = NPCAIStyleID.Fighter;
            NPC.lifeMax = 110;
            NPC.defense = 8;
            NPC.damage = 42;
            NPC.width = 56;
            NPC.height = 76;
            NPC.friendly = false;
        }
        public override void AI() {
            NPC.TargetClosest();
            if (NPC.HasPlayerTarget) {
                NPC.FaceTarget();
                NPC.spriteDirection = NPC.direction;
            }
			if(NPC.collideY&&Math.Sign(NPC.velocity.X)==NPC.direction)NPC.velocity.X/=speedMult;
			if(++NPC.frameCounter>7) {
				//add frame height to frame y position and modulo (returns remainder of integer division) by frame height multiplied by walking frame count
				NPC.frame = new Rectangle(0, (NPC.frame.Y+78)%312, 80, 76);
				NPC.frameCounter = 0;
			}
        }
        public override void PostAI() {
			if(NPC.collideY&&Math.Sign(NPC.velocity.X)==NPC.direction)NPC.velocity.X*=speedMult;
        }

        public void GetMeleeCollisionData(Rectangle victimHitbox, int enemyIndex, ref int specialHitSetter, ref float damageMultiplier, ref Rectangle npcRect, ref float knockbackMult) {
            npcRect.Y += (int)(8*NPC.scale);
            npcRect.Height -= (int)(16*NPC.scale);

            npcRect.X -= (int)(12*NPC.scale);
            npcRect.Width += (int)(24*NPC.scale);
        }

        /*public override void HitEffect(int hitDirection, double damage) {
            if(npc.life<0) {
                for(int i = 0; i < 3; i++)Gore.NewGore(npc.position+new Vector2(Main.rand.Next(npc.width),Main.rand.Next(npc.height)), npc.velocity, mod.GetGoreSlot("Gores/NPCs/DF3_Gore"));
                for(int i = 0; i < 6; i++)Gore.NewGore(npc.position+new Vector2(Main.rand.Next(npc.width),Main.rand.Next(npc.height)), npc.velocity, mod.GetGoreSlot("Gores/NPCs/DF_Effect_Medium"+Main.rand.Next(1,4)));
            }
        }*/
    }
}
