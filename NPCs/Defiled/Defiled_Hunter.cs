using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.OriginExtensions;

namespace Origins.NPCs.Defiled {
    public class Defiled_Hunter_Head : Defiled_Hunter {
        public const float speed = 4f;
        public override void SetStaticDefaults() {
            Main.npcFrameCount[NPC.type] = 4;
            base.SetStaticDefaults();
        }
        public override void SetDefaults() {
            NPC.CloneDefaults(NPCID.DiggerHead);
            NPC.aiStyle = NPCAIStyleID.Fighter;
            NPC.lifeMax = 360;
            NPC.defense = 30;
            NPC.damage = 64;
            NPC.height = 30;
            NPC.width = 32;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.HitSound = Origins.Sounds.DefiledHurt;
            NPC.DeathSound = Origins.Sounds.DefiledKill;
        }
        float Mana {
            get => NPC.localAI[0];
            set => NPC.localAI[0] = value;
        }
        public override void UpdateLifeRegen(ref int damage) {
            if (NPC.life < NPC.lifeMax && Mana > 0) {
                int factor = Main.rand.RandomRound((1 - (NPC.life / 360f)) * 12 + 4);
                NPC.lifeRegen += factor;
                Mana -= factor / 240f;// 1 mana for every 2 health regenerated
            }
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                new FlavorTextBestiaryInfoElement("Acrobatic despite its centi-pedal appearance. Moves slowly at first and then rapidly when nearby the threat."),
            });
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot) {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Black_Bile>(), 1, 1, 3));
            npcLoot.Add(ItemDropRule.Common(ItemID.SilverCoin, 1, 5, 5));
        }
        public override bool PreAI() {
            NPC.TargetClosest();
            if(NPC.HasPlayerTarget) {
                NPC.FaceTarget();
                NPC.spriteDirection = NPC.direction;
            }
            return true;
        }
		public override void OnSpawn(IEntitySource source) {
            ai[3] = NPC.whoAmI;
            NPC.realLife = NPC.whoAmI;
            int current = 0;
            int last = NPC.whoAmI;
            int type = ModContent.NPCType<Defiled_Hunter_Body>();
            for (int k = 0; k < 4; k++) {
                current = NPC.NewNPC(source, (int)NPC.Center.X, (int)NPC.Center.Y, type, NPC.whoAmI);
                Main.npc[current].realLife = NPC.whoAmI;
                Main.npc[current].frame = new Rectangle(0, (k * 32) % 192, 32, 30);
                Main.npc[current].frameCounter = k % animationTime;
                if (Main.npc[current].ModNPC is Defiled_Hunter body) {
                    body.ai[3] = NPC.whoAmI;
                    body.ai[1] = last;
                }
                //Main.NewText($"{current} {Main.npc[current].realLife} {(Main.npc[current].modNPC as Defiled_Hunter).ai[1]}");
                if (Main.npc[last].ModNPC is Defiled_Hunter lastHunter) lastHunter.ai[0] = current;
                //NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, current);
                last = current;
            }
            current = NPC.NewNPC(source, (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<Defiled_Hunter_Tail>(), NPC.whoAmI);
            Main.npc[current].realLife = NPC.whoAmI;
            if (Main.npc[current].ModNPC is Defiled_Hunter tail) {
                tail.ai[3] = NPC.whoAmI;
                tail.ai[1] = last;
            }
            if (Main.npc[last].ModNPC is Defiled_Hunter lastBody) lastBody.ai[0] = current;
        }
		public override void AI() {
            if (Main.rand.NextBool(800)) SoundEngine.PlaySound(Origins.Sounds.DefiledIdle, NPC.Center);
            if (Main.netMode != NetmodeID.MultiplayerClient) {
                NPC.oldPosition = NPC.position;
                if(NPC.collideY) {
                    NPC.rotation = NPC.velocity.ToRotation();
                } else {
                    LinearSmoothing(ref NPC.rotation, NPC.velocity.ToRotation(), 0.5f);
                }
            }
            //if(npc.collideY&&Math.Sign(npc.velocity.X)==npc.direction)npc.velocity.X/=speedMult;
        }
        public override void PostAI() {
            //if(npc.collideY&&Math.Sign(npc.velocity.X)==npc.direction)npc.velocity.X*=speedMult;
		        if (NPC.velocity.X < -speed || NPC.velocity.X > speed) {
			        if (NPC.velocity.Y == 0f) {
				        NPC.velocity *= 0.7f;
			        }
		        }else if (NPC.velocity.X < speed && NPC.direction == 1) {
			        NPC.velocity.X += 0.1f;
			        if (NPC.velocity.X > speed) {
				        NPC.velocity.X = speed;
			        }
		        }else if (NPC.velocity.X > -speed && NPC.direction == -1) {
			        NPC.velocity.X -= 0.1f;
			        if (NPC.velocity.X < -speed) {
				        NPC.velocity.X = -speed;
			        }
		        }
        }
        public override void FindFrame(int frameHeight) {
            if(++NPC.frameCounter>animationTime) {
                NPC.frame = new Rectangle(0, (NPC.frame.Y+28)%112, 32, 26);
                NPC.frameCounter = 0;
            }
        }
    }

    public class Defiled_Hunter_Body : Defiled_Hunter {
        public override void SetStaticDefaults() {
            Main.npcFrameCount[NPC.type] = 6;
            base.SetStaticDefaults();
        }
        public override void SetDefaults() {
            NPC.CloneDefaults(NPCID.DiggerBody);
            NPC.aiStyle = NPCAIStyleID.None;
            NPC.height = 28;
            NPC.width = 32;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.HitSound = Origins.Sounds.DefiledHurt;
            NPC.DeathSound = Origins.Sounds.DefiledKill;
        }
        public override void FindFrame(int frameHeight) {
            if(++NPC.frameCounter>animationTime) {
                NPC.frame = new Rectangle(0, (NPC.frame.Y+32)%192, 32, 30);
                NPC.frameCounter = 0;
            }
        }
    }

    public class Defiled_Hunter_Tail : Defiled_Hunter {
        public override void SetDefaults() {
            NPC.CloneDefaults(NPCID.DiggerTail);
            NPC.aiStyle = NPCAIStyleID.None;
            NPC.height = 28;
            NPC.width = 32;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.HitSound = Origins.Sounds.DefiledHurt;
            NPC.DeathSound = Origins.Sounds.DefiledKill;
        }
        public override void FindFrame(int frameHeight) {
            NPC.frame = new Rectangle(0, 0, 32, 30);
        }
    }

    public abstract class Defiled_Hunter : ModNPC {
        protected const int animationTime = 4;
        protected internal int[] ai = new int[4];
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("{$Defiled} Hunter");
            NPCID.Sets.TrailCacheLength[NPC.type] = 2;
            NPCID.Sets.TrailingMode[NPC.type] = 3;
        }

        public override void AI() {
            if(NPC.realLife < 0) {
                NPC.active = false;
                return;
            }
            NPC head = Main.npc[NPC.realLife];
            NPC.life = head.active ? NPC.lifeMax : 0;
            NPC.immune = head.immune;
            if(Main.netMode != NetmodeID.MultiplayerClient) {
                NPC next = Main.npc[ai[1]];
                if(Math.Abs(AngleDif(NPC.rotation, next.oldRot[1]))>MathHelper.PiOver2) {
                    NPC.rotation = next.oldRot[1];
                } else {
                    LinearSmoothing(ref NPC.rotation, next.oldRot[1], NPC.collideY?0.1f:0.3f);
                }
                //Vector2 targetPos = next.oldPosition + new Vector2(next.width/2, next.height/2) - new Vector2(24, 0).RotatedBy(npc.rotation);;
                //npc.velocity = targetPos - npc.Center;

                Vector2 targetPos = next.oldPosition - new Vector2(24, 0).RotatedBy(NPC.rotation);
                Vector2 unit = new Vector2(1,0).RotatedBy(NPC.rotation);
                /*Vector2? validPos = null;
                for(int i = -2; i <= 2; i++) {
                    if(Collision.CanHit(targetPos,32,28,targetPos+unit-new Vector2(0,8*i),32,28)) {
                        validPos = targetPos-new Vector2(0, 8*i);
                    }
                }
                if(validPos.HasValue) {
                    npc.Center = targetPos + new Vector2(next.width/2, next.height/2);
                } else if((targetPos-npc.Center).Length()>16){
                    npc.Center = targetPos;
                }*/
                Vector2 offset = Collision.AdvancedTileCollision(new bool[TileLoader.TileCount], targetPos - new Vector2(0,8), new Vector2(0,16), 32, 30);
                NPC.position = targetPos + offset - new Vector2(0, 8);
                //LerpSmoothing(ref npc.position, targetPos + offset - new Vector2(0,8), 0.9f, 4);
                //Vector2 center = npc.Center;
                //float dist = Math.Max((targetPos-center).Length()-8, 0);
                //LinearSmoothing(ref center.X, targetPos.X, 1+dist);
                //LinearSmoothing(ref center.Y, targetPos.Y, 1+dist);
                //npc.Center = center;
                //npc.Center = next.oldPosition + new Vector2(next.width/2, next.height/2) - new Vector2(24, 0).RotatedBy(npc.rotation);
            }
        }
        static int MaxMana => 50;
        static int MaxManaDrain => 50;
        public override void OnHitPlayer(Player target, int damage, bool crit) {
            ref float mana = ref Main.npc[NPC.realLife].localAI[0];
            int maxDrain = (int)Math.Min(MaxMana - mana, MaxManaDrain);
            int manaDrain = Math.Min(maxDrain, target.statMana);
            target.statMana -= manaDrain;
            mana += manaDrain;
            if (target.manaRegenDelay < 10) target.manaRegenDelay = 10;
        }

        public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit) {
            if(NPC.realLife!=NPC.whoAmI) {
                if(projectile.usesLocalNPCImmunity) {
                    projectile.localNPCImmunity[NPC.realLife] = projectile.localNPCHitCooldown;
                    projectile.localNPCImmunity[NPC.whoAmI] = 0;
                } else {
                    Main.npc[NPC.realLife].immune[projectile.owner] = NPC.immune[projectile.owner];
                }
            }
            Rectangle spawnbox = projectile.Hitbox.MoveToWithin(NPC.Hitbox);
            for(int i = Main.rand.Next(3); i-->0;)Gore.NewGore(NPC.GetSource_Death(), Main.rand.NextVectorIn(spawnbox), projectile.velocity, Mod.GetGoreSlot("Gores/NPCs/DF_Effect_Small"+Main.rand.Next(1,4)));
        }
        public override bool? CanBeHitByProjectile(Projectile projectile) {
            if(NPC.realLife==NPC.whoAmI)return null;
            if (NPC.realLife < 0) return null;
            if((projectile.usesLocalNPCImmunity?projectile.localNPCImmunity[NPC.realLife]:Main.npc[NPC.realLife].immune[projectile.owner])>0) {
                return false;
            }
            return null;
        }

        public override void OnHitByItem(Player player, Item item, int damage, float knockback, bool crit) {
            if(NPC.realLife!=NPC.whoAmI) {
                NPC head = Main.npc[NPC.realLife];
                head.immune[player.whoAmI] = Math.Max(head.immune[player.whoAmI], NPC.immune[player.whoAmI]);
            }
            int halfWidth = NPC.width / 2;
            int baseX = player.direction > 0 ? 0 : halfWidth;
            for(int i = Main.rand.Next(3); i-->0;)Gore.NewGore(NPC.GetSource_Death(), NPC.position+new Vector2(baseX + Main.rand.Next(halfWidth),Main.rand.Next(NPC.height)), new Vector2(knockback*player.direction, -0.1f*knockback), Mod.GetGoreSlot("Gores/NPCs/DF_Effect_Small"+Main.rand.Next(1,4)));
        }
        public override bool? CanBeHitByItem(Player player, Item item) {
            if(NPC.realLife==NPC.whoAmI)return null;
            if(Main.npc[NPC.realLife].immune[player.whoAmI]>0) {
                return false;
            }
            return null;
        }
        public override void HitEffect(int hitDirection, double damage) {
            if(NPC.life<0) {
                Defiled_Hunter current = Main.npc[NPC.realLife].ModNPC as Defiled_Hunter;
                while(current.ai[0]!=0) {
                    deathEffect(current.NPC);
                    current = Main.npc[current.ai[0]].ModNPC as Defiled_Hunter;
                }
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
            Rectangle frame = NPC.frame;
            spriteBatch.Draw(TextureAssets.Npc[NPC.type].Value, NPC.Center-Main.screenPosition, frame, drawColor, NPC.rotation, new Vector2(frame.Width/2f,frame.Height/2f), NPC.scale, SpriteEffects.FlipHorizontally|(NPC.spriteDirection>0?SpriteEffects.None:SpriteEffects.FlipVertically), 1);
            return false;
        }
        protected static void deathEffect(NPC npc) {
            Gore.NewGore(npc.GetSource_Death(), npc.position, npc.velocity, Origins.instance.GetGoreSlot("Gores/NPCs/DF2_Gore"));
            Gore.NewGore(npc.GetSource_Death(), npc.position, npc.velocity, Origins.instance.GetGoreSlot("Gores/NPCs/DF_Effect_Small"+Main.rand.Next(1,4)));
        }
        public override void SendExtraAI(BinaryWriter writer) {
            writer.Write(ai[0]);
            writer.Write(ai[1]);
            writer.Write(ai[2]);
            writer.Write(ai[3]);
            writer.Write(NPC.localAI[0]);
        }
        public override void ReceiveExtraAI(BinaryReader reader) {
            ai[0] = reader.ReadInt32();
            ai[1] = reader.ReadInt32();
            ai[2] = reader.ReadInt32();
            ai[3] = reader.ReadInt32();
            NPC.localAI[0] = reader.ReadSingle();
        }
    }
}