using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Defiled {
    public class Defiled_Brute : ModNPC {
        public const float speedMult = 0.75f;
        //public float SpeedMult => npc.frame.Y==510?1.6f:0.8f;
        //bool attacking = false;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("{$Defiled} Krusher");
            Main.npcFrameCount[NPC.type] = 4;
        }
        public override void SetDefaults() {
            NPC.CloneDefaults(NPCID.Zombie);
            NPC.aiStyle = NPCAIStyleID.Fighter;
            NPC.lifeMax = 160;
            NPC.defense = 9;
            NPC.damage = 49;
            NPC.width = 76;
            NPC.height = 66;
            NPC.friendly = false;
            NPC.HitSound = Origins.Sounds.DefiledHurt.WithPitchRange(0.5f, 0.75f);
            NPC.DeathSound = Origins.Sounds.DefiledKill.WithPitchRange(0.5f, 0.75f);
        }
        static int MaxMana => 200;
        static int MaxManaDrain => 24;
        float Mana {
            get;
            set;
        }
        public override void OnHitPlayer(Player target, int damage, bool crit) {
            int maxDrain = (int)Math.Min(MaxMana - Mana, MaxManaDrain);
            int manaDrain = Math.Min(maxDrain, target.statMana);
            target.statMana -= manaDrain;
            Mana += manaDrain;
            if (target.manaRegenDelay < 10) target.manaRegenDelay = 10;
        }
        public override void UpdateLifeRegen(ref int damage) {
            if (NPC.life < NPC.lifeMax && Mana > 0) {
                int factor = 37 / ((NPC.life / 40) + 2);
                NPC.lifeRegen += factor;
                Mana -= factor / 90f;// 3 mana for every 2 health regenerated
            }
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                new FlavorTextBestiaryInfoElement("A stumbling defender of the Wastelands. It will be summoned if other {$Defiled} antibodies struggle."),
            });
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot) {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Strange_String>(), 1, 1, 3));
            //npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Defiled_Spirit>(), 10));
            //npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Bombardment>(), 58));
            npcLoot.Add(ItemDropRule.Common(ItemID.SilverCoin));
            npcLoot.Add(ItemDropRule.Common(ItemID.CopperCoin, 1, 3, 3));
        }

        public override bool PreAI() {
            if (Main.rand.NextBool(1000)) SoundEngine.PlaySound(Origins.Sounds.DefiledIdle.WithPitchRange(0.5f, 0.75f), NPC.Center);
            //if(!attacking) {
                if(NPC.collideY&&Math.Sign(NPC.velocity.X)==NPC.direction)NPC.velocity.X/=speedMult;
                //npc.Hitbox = new Rectangle((int)npc.position.X+(npc.oldDirection == 1 ? 70 : 52), (int)npc.position.Y, 56, npc.height);
            //}
            return true;
        }
        public override void AI() {
            NPC.TargetClosest();
            /*if(npc.localAI[3]<=0&&npc.targetRect.Intersects(new Rectangle((int)npc.position.X-(npc.direction == 1 ? 70 : 52), (int)npc.position.Y, 178, npc.height))) {
                if(!attacking) {
                    npc.frame = new Rectangle(0, 680, 182, 170);
                    npc.frameCounter = 0;
                    npc.velocity.X*=0.25f;
                }
                attacking = true;
            }
            if(npc.localAI[3]>0)npc.localAI[3]--;*/
            if (NPC.HasPlayerTarget) {
                NPC.FaceTarget();
                NPC.spriteDirection = NPC.direction;
            }
            /*if(attacking) {
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
            //}else{*/
                if(++NPC.frameCounter>9) {
                    //add frame height to frame y position and modulo by frame height multiplied by walking frame count
                    NPC.frame = new Rectangle(0, (NPC.frame.Y+66)%264, 76, 66);
                    NPC.frameCounter = 0;
                }
                if(NPC.collideY&&Math.Sign(NPC.velocity.X)==NPC.direction)NPC.velocity.X*=speedMult;
            //}
        }
        public override void PostAI() {
            //if(!attacking) {
                //if(npc.collideY&&Math.Sign(npc.velocity.X)==npc.direction)npc.velocity.X*=SpeedMult;
                //npc.Hitbox = new Rectangle((int)npc.position.X+(npc.oldDirection == 1 ? 70 : 52), (int)npc.position.Y, 56, npc.height);
            //}
        }
        /*public void GetMeleeCollisionData(Rectangle victimHitbox, int enemyIndex, ref int specialHitSetter, ref float damageMultiplier, ref Rectangle npcRect, ref float knockbackMult) {
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
            npcRect = new Rectangle((int)npc.position.X+(flip?70:52), (int)npc.position.Y, 56, npc.height);* /
        }*/
        public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit) {
            Rectangle spawnbox = projectile.Hitbox.MoveToWithin(NPC.Hitbox);
            for(int i = Main.rand.Next(3); i-->0;)Gore.NewGore(NPC.GetSource_Death(), Main.rand.NextVectorIn(spawnbox), projectile.velocity, Mod.GetGoreSlot("Gores/NPCs/DF_Effect_Small"+Main.rand.Next(1,4)));
        }
        public override void OnHitByItem(Player player, Item item, int damage, float knockback, bool crit) {
            int halfWidth = NPC.width / 2;
            int baseX = player.direction > 0 ? 0 : halfWidth;
            for(int i = Main.rand.Next(3); i-->0;)Gore.NewGore(NPC.GetSource_Death(), NPC.position+new Vector2(baseX + Main.rand.Next(halfWidth),Main.rand.Next(NPC.height)), new Vector2(knockback*player.direction, -0.1f*knockback), Mod.GetGoreSlot("Gores/NPCs/DF_Effect_Small"+Main.rand.Next(1,4)));
        }
        public override void HitEffect(int hitDirection, double damage) {
            if(NPC.life<0) {
                for(int i = 0; i < 6; i++)Gore.NewGore(NPC.GetSource_Death(), NPC.position+new Vector2(Main.rand.Next(NPC.width),Main.rand.Next(NPC.height)), NPC.velocity, Mod.GetGoreSlot("Gores/NPCs/DF3_Gore"));
                for(int i = 0; i < 10; i++)Gore.NewGore(NPC.GetSource_Death(), NPC.position+new Vector2(Main.rand.Next(NPC.width),Main.rand.Next(NPC.height)), NPC.velocity, Mod.GetGoreSlot("Gores/NPCs/DF_Effect_Medium"+Main.rand.Next(1,4)));
            }
        }
        public override void SendExtraAI(BinaryWriter writer) {
            writer.Write(Mana);
        }
        public override void ReceiveExtraAI(BinaryReader reader) {
            Mana = reader.ReadSingle();
        }
    }
}
