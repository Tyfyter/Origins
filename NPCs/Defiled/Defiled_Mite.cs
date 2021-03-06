﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace Origins.NPCs.Defiled {
    public class Defiled_Mite : ModNPC {
        internal const int spawnCheckDistance = 15;
        public const int aggroRange = 128;
        byte frame = 0;
        byte anger = 0;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Defiled Mite");
            Main.npcFrameCount[npc.type] = 4;
        }
        public override void SetDefaults() {
            npc.CloneDefaults(NPCID.Bunny);
            npc.aiStyle = NPCAIStyleID.None;
            npc.lifeMax = 22;
            npc.defense = 6;
            npc.damage = 34;
            npc.width = 34;
            npc.height = 28;
            npc.friendly = false;
        }
        public override bool PreAI() {
            npc.TargetClosest();
            npc.aiStyle = npc.HasPlayerTarget ? NPCAIStyleID.Fighter : NPCAIStyleID.None;
            if(((npc.Center-npc.targetRect.Center.ToVector2())*new Vector2(1,2)).Length()>aggroRange) {
                if(npc.life<npc.lifeMax) {
                    npc.aiStyle = NPCAIStyleID.Tortoise;
                } else {
                    npc.target = -1;
                    npc.aiStyle = NPCAIStyleID.None;
                }
            }
            if(npc.HasPlayerTarget) {
                npc.FaceTarget();
                npc.spriteDirection = npc.direction;
            }
            if(npc.collideY) {
                npc.rotation = 0;
                if(anger!=0) {
                    if(anger>1)anger--;
                    npc.aiStyle = NPCAIStyleID.Tortoise;
                }else if(npc.aiStyle==NPCAIStyleID.None) {
                    npc.velocity.X*=0.85f;
                } else if(npc.aiStyle==NPCAIStyleID.Fighter){
                    frame = (byte)((frame+1)&15);
                }
            }else {
                if(anger == 1) anger = 0;
            }
            return npc.aiStyle!=NPCAIStyleID.None;
        }
        public override void FindFrame(int frameHeight) {
            npc.frame = new Rectangle(0, 30*(frame&12)/4, 32, 30);
        }
        public override bool StrikeNPC(ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit) {
            anger = 6;
            return true;
        }
        public override int SpawnNPC(int tileX, int tileY) {
            Tile tile;
            for(int i = 0; i < spawnCheckDistance; i++) {
                tile = Main.tile[tileX, ++tileY];
                if(tile.active()) {
                    tileY--;
                    break;
                }
            }
            return base.SpawnNPC(tileX, tileY);
        }
        public override void HitEffect(int hitDirection, double damage) {
            if(npc.life<0) {
                Gore.NewGore(npc.position, npc.velocity, mod.GetGoreSlot("Gores/NPCs/DF1_Gore"));
                Gore.NewGore(npc.position, npc.velocity, mod.GetGoreSlot("Gores/NPCs/DF_Effect_Medium"+Main.rand.Next(1,4)));
                for(int i = 0; i < 3; i++)Gore.NewGore(npc.position, npc.velocity, mod.GetGoreSlot("Gores/NPCs/DF_Effect_Small"+Main.rand.Next(1,4)));
            }
        }
    }
}
