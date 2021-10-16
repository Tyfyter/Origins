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
    public class Defiled_Brute : ModNPC, IMeleeCollisionDataNPC {
        public const float speedMult = 0.75f;
        public float SpeedMult => npc.frame.Y==510?1.6f:0.8f;
        bool attacking = false;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Defiled Krusher");
            Main.npcFrameCount[npc.type] = 7;
        }
        public override void SetDefaults() {
            npc.CloneDefaults(NPCID.Zombie);
            npc.aiStyle = NPCAIStyleID.Fighter;
            npc.lifeMax = 160;
            npc.defense = 9;
            npc.damage = 49;
            npc.width = 56;
            npc.height = 108;
            npc.friendly = false;
        }
        public override bool PreAI() {
            if(!attacking) {
                if(npc.collideY&&Math.Sign(npc.velocity.X)==npc.direction)npc.velocity.X/=SpeedMult;
                //npc.Hitbox = new Rectangle((int)npc.position.X+(npc.oldDirection == 1 ? 70 : 52), (int)npc.position.Y, 56, npc.height);
            }
            return true;
        }
        public override void AI() {
            npc.TargetClosest();
            if(npc.localAI[3]<=0&&npc.targetRect.Intersects(new Rectangle((int)npc.position.X-(npc.direction == 1 ? 70 : 52), (int)npc.position.Y, 178, npc.height))) {
                if(!attacking) {
                    npc.frame = new Rectangle(0, 680, 182, 170);
                    npc.frameCounter = 0;
                    npc.velocity.X*=0.25f;
                }
                attacking = true;
            }
            if(npc.localAI[3]>0)npc.localAI[3]--;
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
                            npc.localAI[3] = 60;
                        }
                    } else {
                        npc.frame = new Rectangle(0, (npc.frame.Y+170)%1190, 182, 170);
                        npc.frameCounter = 0;
                    }
                }
                if (npc.collideY) {
                    npc.velocity.X*=0.5f;
                }
            }else{
                if(++npc.frameCounter>9) {
                    //add frame height to frame y position and modulo by frame height multiplied by walking frame count
                    npc.frame = new Rectangle(0, (npc.frame.Y+170)%680, 182, 170);
                    npc.frameCounter = 0;
                }
                if(npc.collideY&&Math.Sign(npc.velocity.X)==npc.direction)npc.velocity.X*=SpeedMult;
            }
        }
        public override void PostAI() {
            if(!attacking) {
                //if(npc.collideY&&Math.Sign(npc.velocity.X)==npc.direction)npc.velocity.X*=SpeedMult;
                //npc.Hitbox = new Rectangle((int)npc.position.X+(npc.oldDirection == 1 ? 70 : 52), (int)npc.position.Y, 56, npc.height);
            }
        }
        public void GetMeleeCollisionData(Rectangle victimHitbox, int enemyIndex, ref int specialHitSetter, ref float damageMultiplier, ref Rectangle npcRect, ref float knockbackMult) {
            bool flip = npc.direction == 1;
            //Rectangle armHitbox = new Rectangle((int)npc.position.X+(flip?0:108), (int)npc.position.Y, 70, npc.height);
            bool h = victimHitbox.Intersects(npcRect);
            if(attacking) {
                if(npc.frame.Y>=1018) {
                    npcRect = new Rectangle(npcRect.Center.X+(npc.direction*63), npcRect.Y, 52, npc.height);
                    if(npc.frameCounter<9&&victimHitbox.Intersects(npcRect)) {
                        damageMultiplier = 3;
                        knockbackMult = 2f;
                        return;
                    }
                }
            }
            /*if(victimHitbox.Intersects(armHitbox)) {
                npcRect = armHitbox;
                return;
            }
            npcRect = new Rectangle((int)npc.position.X+(flip?70:52), (int)npc.position.Y, 56, npc.height);*/
        }
        public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit) {
            Rectangle spawnbox = projectile.Hitbox.MoveToWithin(npc.Hitbox);
            for(int i = Main.rand.Next(3); i-->0;)Gore.NewGore(Main.rand.NextVectorIn(spawnbox), projectile.velocity, mod.GetGoreSlot("Gores/NPCs/DF_Effect_Small"+Main.rand.Next(1,4)));
        }
        public override void OnHitByItem(Player player, Item item, int damage, float knockback, bool crit) {
            int halfWidth = npc.width / 2;
            int baseX = player.direction > 0 ? 0 : halfWidth;
            for(int i = Main.rand.Next(3); i-->0;)Gore.NewGore(npc.position+new Vector2(baseX + Main.rand.Next(halfWidth),Main.rand.Next(npc.height)), new Vector2(knockback*player.direction, -0.1f*knockback), mod.GetGoreSlot("Gores/NPCs/DF_Effect_Small"+Main.rand.Next(1,4)));
        }
        public override void HitEffect(int hitDirection, double damage) {
            if(npc.life<0) {
                for(int i = 0; i < 6; i++)Gore.NewGore(npc.position+new Vector2(Main.rand.Next(npc.width),Main.rand.Next(npc.height)), npc.velocity, mod.GetGoreSlot("Gores/NPCs/DF3_Gore"));
                for(int i = 0; i < 10; i++)Gore.NewGore(npc.position+new Vector2(Main.rand.Next(npc.width),Main.rand.Next(npc.height)), npc.velocity, mod.GetGoreSlot("Gores/NPCs/DF_Effect_Medium"+Main.rand.Next(1,4)));
            }
        }
    }
}
