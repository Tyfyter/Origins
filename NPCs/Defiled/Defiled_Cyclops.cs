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
    public class Defiled_Cyclops : ModNPC {
        public const float speedMult = 1f;
        bool attacking = false;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Defiled Cyclops");
            Main.npcFrameCount[npc.type] = 7;
        }
        public override void SetDefaults() {
            npc.CloneDefaults(NPCID.Zombie);
            npc.aiStyle = NPCAIStyleID.Fighter;
            npc.lifeMax = 110;
            npc.defense = 8;
            npc.damage = 42;
            npc.width = 110;
            npc.height = 118;
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
				//add frame height to frame y position and modulo by frame height multiplied by walking frame count
				npc.frame = new Rectangle(0, (npc.frame.Y+120)%480, 110, 118);
				npc.frameCounter = 0;
			}
        }
        public override void PostAI() {
			if(npc.collideY&&Math.Sign(npc.velocity.X)==npc.direction)npc.velocity.X*=speedMult;
            /*if(!attacking) {
                npc.Hitbox = new Rectangle((int)npc.position.X+(npc.oldDirection == 1 ? 70 : 52), (int)npc.position.Y, 56, npc.height);
            }*/
        }
        /*public override bool CanHitPlayer(Player target, ref int cooldownSlot) {
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
        }*/
        public override void HitEffect(int hitDirection, double damage) {
            if(npc.life<0) {
                for(int i = 0; i < 3; i++)Gore.NewGore(npc.position+new Vector2(Main.rand.Next(npc.width),Main.rand.Next(npc.height)), npc.velocity, mod.GetGoreSlot("Gores/NPCs/DF3_Gore"));
                for(int i = 0; i < 6; i++)Gore.NewGore(npc.position+new Vector2(Main.rand.Next(npc.width),Main.rand.Next(npc.height)), npc.velocity, mod.GetGoreSlot("Gores/NPCs/DF_Effect_Medium"+Main.rand.Next(1,4)));
            }
        }
    }
}
