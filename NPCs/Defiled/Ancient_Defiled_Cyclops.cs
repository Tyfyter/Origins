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
using Origins.Items.Materials;
using Terraria.GameContent.ItemDropRules;
using Origins.Items.Weapons.Defiled;

namespace Origins.NPCs.Defiled {
	public class Ancient_Defiled_Cyclops : ModNPC, IMeleeCollisionDataNPC {
        public const float speedMult = 1f;
        bool attacking = false;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Defiled Cyclops");
            Main.npcFrameCount[Type] = 7;
        }
        public override void SetDefaults() {
            NPC.CloneDefaults(NPCID.Zombie);
            NPC.aiStyle = NPCAIStyleID.Fighter;
            NPC.lifeMax = 300;
            NPC.defense = 12;
            NPC.damage = 63;
            NPC.width = 110;
            NPC.height = 120;
            NPC.friendly = false;
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot) {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Strange_String>(), 1, 1, 3));
            //npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Defiled_Spirit>(), 10));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Ancient_Kruncher>()));
        }
        public override void AI() {
            NPC.TargetClosest();
            if (NPC.Hitbox.Intersects(NPC.targetRect)) {
                if (!attacking) {
                    NPC.frame = new Rectangle(0, 120 * 4, 110, 120);
                    NPC.frameCounter = 0;
                    NPC.velocity.X *= 0.25f;
                }
                attacking = true;
            }
            if (NPC.HasPlayerTarget) {
                NPC.FaceTarget();
                NPC.spriteDirection = NPC.direction;
            }
            if (attacking) {
                if (++NPC.frameCounter > 7) {
                    //add frame height to frame y position and modulo by frame height multiplied by walking frame count
                    if (NPC.frame.Y >= 720) {
                        if (NPC.frameCounter > 19) {
                            NPC.frame = new Rectangle(0, 0, 110, 120);
                            NPC.frameCounter = 0;
                            attacking = false;
                        }
                    } else {
                        NPC.frame = new Rectangle(0, (NPC.frame.Y + 120) % 840, 110, 120);
                        NPC.frameCounter = 0;
                    }
                }
            } else {
                if (NPC.collideY && Math.Sign(NPC.velocity.X) == NPC.direction) NPC.velocity.X /= speedMult;
                if (++NPC.frameCounter > 7) {
                    //add frame height to frame y position and modulo by frame height multiplied by walking frame count
                    NPC.frame = new Rectangle(0, (NPC.frame.Y + 120) % (840 - 120 * 3), 110, 120);
                    NPC.frameCounter = 0;
                }
            }
        }
        public override void PostAI() {
            if (NPC.collideY && Math.Sign(NPC.velocity.X) == NPC.direction) NPC.velocity.X *= speedMult;
        }

        public void GetMeleeCollisionData(Rectangle victimHitbox, int enemyIndex, ref int specialHitSetter, ref float damageMultiplier, ref Rectangle npcRect, ref float knockbackMult) {
            if (attacking) {
                if (NPC.frame.Y >= 720) {
                    int hitboxWidth = 32;
                    int hitboxHeight = 64;

                    Rectangle armHitbox = new Rectangle((int)NPC.Center.X + ((NPC.width / 2) * NPC.direction) - hitboxWidth / 2, (int)NPC.Center.Y - hitboxHeight / 2, hitboxWidth, hitboxHeight);
                    if (NPC.frameCounter < 9 && victimHitbox.Intersects(armHitbox)) {
                        damageMultiplier = 2;
                        knockbackMult = 1.5f;
                        npcRect = armHitbox;
                        return;
                    }
                }
            }
            npcRect.Width /= 2;
            npcRect.X += npcRect.Width / 2;
        }
        public override void HitEffect(int hitDirection, double damage) {
            if (NPC.life < 0) {
                for (int i = 0; i < 3; i++) Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, Mod.GetGoreSlot("Gores/NPCs/DF3_Gore"));
                for (int i = 0; i < 6; i++) Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, Mod.GetGoreSlot("Gores/NPCs/DF_Effect_Medium" + Main.rand.Next(1, 4)));
            }
        }
    }
}