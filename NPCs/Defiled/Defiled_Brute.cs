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

namespace Origins.NPCs.Defiled {
    public class Defiled_Brute : ModNPC {
        public const float speedMult = 0.75f;
        bool attacking = false;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Defiled Krusher");
            Main.npcFrameCount[npc.type] = 7;
        }
        public override void SetDefaults() {
            npc.CloneDefaults(NPCID.Zombie);
            npc.aiStyle = AIStyleID.Fighter;
            npc.lifeMax = 160;
            npc.defense = 9;
            npc.damage = 49;
            npc.width = 178;
            npc.height = 108;
            npc.friendly = false;
        }
        public override bool PreAI() {
            if(!attacking) {
                npc.Hitbox = new Rectangle((int)npc.position.X-(npc.direction == 1 ? 70 : 52), (int)npc.position.Y, 178, npc.height);
            }
            return true;
        }
        public override void AI() {
            npc.TargetClosest();
            if(npc.Hitbox.Intersects(npc.targetRect)) {
                if(!attacking) {
                    npc.frame = new Rectangle(0, 680, 182, 170);
                    npc.frameCounter = 0;
                    npc.velocity.X*=0.25f;
                }
                attacking = true;
            }
            if (npc.HasPlayerTarget) {
                npc.FaceTarget();
                npc.spriteDirection = npc.direction;
            }
            if(attacking) {
                if(++npc.frameCounter>7) {
                    //add frame height to frame y position and modulo by frame height multiplied by walking frame count
                    if(npc.frame.Y>=1018) {
                        if(npc.frameCounter>19) {
                            npc.frame = new Rectangle(0, 0, 182, 170);
                            npc.frameCounter = 0;
                            attacking = false;
                        }
                    } else {
                        npc.frame = new Rectangle(0, (npc.frame.Y+170)%1188, 182, 170);
                        npc.frameCounter = 0;
                    }
                }
            }else{
                if(npc.collideY&&Math.Sign(npc.velocity.X)==npc.direction)npc.velocity.X/=speedMult;
                if(++npc.frameCounter>9) {
                    //add frame height to frame y position and modulo by frame height multiplied by walking frame count
                    npc.frame = new Rectangle(0, (npc.frame.Y+170)%680, 182, 170);
                    npc.frameCounter = 0;
                }
            }
        }
        public override void PostAI() {
            if(!attacking) {
                if(npc.collideY&&Math.Sign(npc.velocity.X)==npc.direction)npc.velocity.X*=speedMult;
                npc.Hitbox = new Rectangle((int)npc.position.X+(npc.oldDirection == 1 ? 70 : 52), (int)npc.position.Y, 56, npc.height);
            }
        }
        public override bool CanHitPlayer(Player target, ref int cooldownSlot) {
            switch(GetMeleeCollisionData(target.Hitbox)) {
                case 2:
                double damage = target.Hurt(PlayerDeathReason.ByNPC(npc.whoAmI), npc.damage*3, npc.direction);
                if(!target.noKnockback&&damage>0)target.velocity.X+=2*npc.direction;
                break;
                case 1:
                return true;
            }
            return false;
        }
        public override bool? CanHitNPC(NPC target) {
            switch(GetMeleeCollisionData(target.Hitbox)) {
                case 2:
                if(target.friendly&&!target.dontTakeDamageFromHostiles)target.StrikeNPC(npc.damage*3, 4, npc.direction);
                break;
                case 1:
                return null;
            }
            return false;
        }
        private byte GetMeleeCollisionData(Rectangle targetHitbox) {
            bool flip = npc.direction == 1;
            Rectangle armHitbox = new Rectangle((int)npc.position.X+(flip?0:108), (int)npc.position.Y, 70, npc.height);
            if(attacking) {
                if(npc.frame.Y>=1018) {
                    armHitbox = new Rectangle((int)npc.position.X+(flip?126:0), (int)npc.position.Y, 52, npc.height);
                    if(npc.frameCounter<9&&targetHitbox.Intersects(armHitbox)) {
                        return 2;
                    }
                }
            }
            if(targetHitbox.Intersects(armHitbox)) {
                return 1;
            }
            Rectangle bodyHitbox = new Rectangle((int)npc.position.X+(flip?70:52), (int)npc.position.Y, 56, npc.height);
            if(targetHitbox.Intersects(bodyHitbox)) {
                return 1;
            }
            return 0;
        }
        public override void HitEffect(int hitDirection, double damage) {
            if(npc.life<0) {
                for(int i = 0; i < 6; i++)Gore.NewGore(npc.position+new Vector2(Main.rand.Next(npc.width),Main.rand.Next(npc.height)), npc.velocity, mod.GetGoreSlot("Gores/NPCs/DF3_Gore"));
                for(int i = 0; i < 10; i++)Gore.NewGore(npc.position+new Vector2(Main.rand.Next(npc.width),Main.rand.Next(npc.height)), npc.velocity, mod.GetGoreSlot("Gores/NPCs/DF_Effect_Medium"+Main.rand.Next(1,4)));
            }
        }
    }
}
