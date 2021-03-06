﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using static Origins.OriginExtensions;

namespace Origins.NPCs.Defiled {
    public class Defiled_Tripod : ModNPC {
        public const float horizontalSpeed = 3.2f;
        public const float horizontalAirSpeed = 2f;
        public const float verticalSpeed = 4f;
        bool attacking = false;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Defiled Tripod");
            Main.npcFrameCount[npc.type] = 4;
        }
        public override void SetDefaults() {
            npc.CloneDefaults(NPCID.Zombie);
            npc.aiStyle = NPCAIStyleID.None;//NPCAIStyleID.Fighter;
            npc.lifeMax = 475;
            npc.defense = 28;
            npc.damage = 52;
            npc.width = 98;
            npc.height = 98;
            npc.scale = 0.85f;
            npc.friendly = false;
        }
        public override void AI() {
            npc.TargetClosest();
            if (npc.HasPlayerTarget) {
                npc.FaceTarget();
                npc.spriteDirection = npc.direction;
            }

            npc.velocity = npc.oldVelocity * new Vector2(npc.collideX?0.5f:1, npc.collideY?0.75f:1);
            int targetVMinDirection = Math.Sign(npc.targetRect.Bottom - npc.Bottom.Y);
            int targetVMaxDirection = Math.Sign(npc.targetRect.Top - npc.Bottom.Y);
            if(npc.collideY&&targetVMaxDirection==1) {
                npc.position.Y++;
            }
            float moveDir = Math.Sign(npc.velocity.X);
            float absX = moveDir==0?0:npc.velocity.X / moveDir;

            if(npc.collideY) {
                npc.ai[1] = 0;
                //npc.rotation = 0;
                LinearSmoothing(ref npc.rotation, 0, 0.15f);
                if(moveDir != -npc.direction) {
                    if(absX < horizontalSpeed) {
                        npc.velocity.X += npc.direction * 0.5f;
                    } else {
                        LinearSmoothing(ref npc.velocity.X, npc.direction*horizontalSpeed, 0.1f);
                    }
                } else {
                    npc.velocity.X += npc.direction * 0.15f;
                }
                if(npc.ai[0] > 0) {
                    npc.ai[0]--;
                } else {
                    if(npc.collideX) {
                        if(npc.targetRect.Bottom > npc.Top.Y) {
                            //npc.velocity.X *= 2;
                            npc.position.X += npc.direction;
                        } else {
                            if(npc.velocity.Y > -4) {
                                npc.velocity.Y -= 1;
                            }
                            if(moveDir != -npc.direction) {
                                LinearSmoothing(ref npc.rotation, -MathHelper.PiOver2 * moveDir, 0.15f);
                                //npc.rotation = -MathHelper.PiOver2 * moveDir;
                                if(targetVMinDirection==-1) {
                                    npc.position.Y--;
                                }
                            }
                        }
                    } else if(moveDir == npc.direction && absX > 3) {
                        float dist = npc.targetRect.Distance(npc.Center);
                        if(dist > 96 && dist < 240) {
                            npc.velocity.X += moveDir * 4;
                            npc.velocity.Y -= (npc.Center.Y - npc.targetRect.Center.Y > 80) ? 8 : 4f;
                            npc.ai[0] = 35;
                        }
                    }
                }
            } else if(npc.collideX) {
                npc.ai[1] = 0;
                if(targetVMinDirection==-1) {
                    if(npc.velocity.Y > -verticalSpeed)npc.velocity.Y -= 1;
                    if(moveDir != -npc.direction) {
                        LinearSmoothing(ref npc.rotation, -MathHelper.PiOver2 * moveDir, 0.15f);
                        //npc.rotation = -MathHelper.PiOver2 * moveDir;
                    }
                } else {
                    //npc.velocity.X *= 2;
                    npc.position.X += npc.direction;
                }
            } else {
                if(++npc.ai[1]>1)LinearSmoothing(ref npc.rotation, 0, 0.15f);
                if(moveDir != -npc.direction) {
                    if(absX<horizontalAirSpeed)npc.velocity.X += npc.direction*0.2f;
                }
            }

			if(npc.velocity.RotatedBy(-npc.rotation).X*npc.direction>0.5f&&++npc.frameCounter>6) {
				//add frame height to frame y position and modulo by frame height multiplied by walking frame count
				npc.frame = new Rectangle(0, (npc.frame.Y+100)%400, 98, 98);
				npc.frameCounter = 0;
			}
        }
        public override void HitEffect(int hitDirection, double damage) {
            if(npc.life<0) {
                for(int i = 0; i < 3; i++)Gore.NewGore(npc.position+new Vector2(Main.rand.Next(npc.width),Main.rand.Next(npc.height)), npc.velocity, mod.GetGoreSlot("Gores/NPCs/DF3_Gore"));
                for(int i = 0; i < 6; i++)Gore.NewGore(npc.position+new Vector2(Main.rand.Next(npc.width),Main.rand.Next(npc.height)), npc.velocity, mod.GetGoreSlot("Gores/NPCs/DF_Effect_Medium"+Main.rand.Next(1,4)));
            }
        }
    }
}
