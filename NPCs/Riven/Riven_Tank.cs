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
            DisplayName.SetDefault("misingno");
            Main.npcFrameCount[npc.type] = 4;
        }
        public override void SetDefaults() {
            npc.CloneDefaults(NPCID.Zombie);
            npc.aiStyle = NPCAIStyleID.Fighter;
            npc.lifeMax = 110;
            npc.defense = 8;
            npc.damage = 42;
            npc.width = 56;
            npc.height = 76;
            npc.friendly = false;
        }
        public override void AI() {
            npc.TargetClosest();
            if (npc.HasPlayerTarget) {
                npc.FaceTarget();
                npc.spriteDirection = npc.direction;
            }
			if(npc.collideY&&Math.Sign(npc.velocity.X)==npc.direction)npc.velocity.X/=speedMult;
			if(++npc.frameCounter>7) {
				//add frame height to frame y position and modulo (returns remainder of integer division) by frame height multiplied by walking frame count
				npc.frame = new Rectangle(0, (npc.frame.Y+78)%312, 80, 76);
				npc.frameCounter = 0;
			}
        }
        public override void PostAI() {
			if(npc.collideY&&Math.Sign(npc.velocity.X)==npc.direction)npc.velocity.X*=speedMult;
        }

        public void GetMeleeCollisionData(Rectangle victimHitbox, int enemyIndex, ref int specialHitSetter, ref float damageMultiplier, ref Rectangle npcRect, ref float knockbackMult) {
            npcRect.Y += (int)(8*npc.scale);
            npcRect.Height -= (int)(16*npc.scale);

            npcRect.X -= (int)(12*npc.scale);
            npcRect.Width += (int)(24*npc.scale);
        }

        /*public override void HitEffect(int hitDirection, double damage) {
            if(npc.life<0) {
                for(int i = 0; i < 3; i++)Gore.NewGore(npc.position+new Vector2(Main.rand.Next(npc.width),Main.rand.Next(npc.height)), npc.velocity, mod.GetGoreSlot("Gores/NPCs/DF3_Gore"));
                for(int i = 0; i < 6; i++)Gore.NewGore(npc.position+new Vector2(Main.rand.Next(npc.width),Main.rand.Next(npc.height)), npc.velocity, mod.GetGoreSlot("Gores/NPCs/DF_Effect_Medium"+Main.rand.Next(1,4)));
            }
        }*/
    }
}
