using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.OriginExtensions;

namespace Origins.NPCs.Defiled {
    public class Defiled_Hunter_Head : Defiled_Hunter {
        public const float speed = 4f;
        public override void SetStaticDefaults() {
            Main.npcFrameCount[npc.type] = 4;
            base.SetStaticDefaults();
        }
        public override void SetDefaults() {
            npc.CloneDefaults(NPCID.DiggerHead);
            npc.aiStyle = AIStyleID.Fighter;
            npc.lifeMax = 360;
            npc.defense = 30;
            npc.damage = 64;
            npc.height = 30;
            npc.width = 32;
            npc.noGravity = false;
            npc.noTileCollide = false;
        }
        public override bool PreAI() {
            npc.TargetClosest();
            if(npc.HasPlayerTarget) {
                npc.FaceTarget();
                npc.spriteDirection = npc.direction;
            }
            return true;
        }
        public override void AI() {
            if(Main.netMode != NetmodeID.MultiplayerClient) {
                npc.oldPosition = npc.position;
                if(npc.collideY) {
                    npc.rotation = npc.velocity.ToRotation();
                } else {
                    LinearSmoothing(ref npc.rotation, npc.velocity.ToRotation(), 0.5f);
                }
                if(ai[0] == 0f) {
                    ai[3] = npc.whoAmI;
                    npc.realLife = npc.whoAmI;
                    int current = 0;
                    int last = npc.whoAmI;
                    int type = ModContent.NPCType<Defiled_Hunter_Body>();
                    for(int k = 0; k < 4; k++) {
                        current = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, type, npc.whoAmI);
                        Main.npc[current].realLife = npc.whoAmI;
                        Main.npc[current].frame = new Rectangle(0, (k*32)%192, 32, 30);
                        Main.npc[current].frameCounter = k%animationTime;
                        if(Main.npc[current].modNPC is Defiled_Hunter body) {
                            body.ai[3] = npc.whoAmI;
                            body.ai[1] = last;
                        }
                        //Main.NewText($"{current} {Main.npc[current].realLife} {(Main.npc[current].modNPC as Defiled_Hunter).ai[1]}");
                        if(Main.npc[last].modNPC is Defiled_Hunter lastHunter)lastHunter.ai[0] = current;
                        //NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, current);
                        last = current;
                    }
                    current = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, ModContent.NPCType<Defiled_Hunter_Tail>(), npc.whoAmI);
                    Main.npc[current].realLife = npc.whoAmI;
                    if(Main.npc[current].modNPC is Defiled_Hunter tail) {
                        tail.ai[3] = npc.whoAmI;
                        tail.ai[1] = last;
                    }
                    if(Main.npc[last].modNPC is Defiled_Hunter lastBody)lastBody.ai[0] = current;
                    //NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, current);
                }
            }
            //if(npc.collideY&&Math.Sign(npc.velocity.X)==npc.direction)npc.velocity.X/=speedMult;
        }
        public override void PostAI() {
            //if(npc.collideY&&Math.Sign(npc.velocity.X)==npc.direction)npc.velocity.X*=speedMult;
		        if (npc.velocity.X < -speed || npc.velocity.X > speed) {
			        if (npc.velocity.Y == 0f) {
				        npc.velocity *= 0.7f;
			        }
		        }else if (npc.velocity.X < speed && npc.direction == 1) {
			        npc.velocity.X += 0.1f;
			        if (npc.velocity.X > speed) {
				        npc.velocity.X = speed;
			        }
		        }else if (npc.velocity.X > -speed && npc.direction == -1) {
			        npc.velocity.X -= 0.1f;
			        if (npc.velocity.X < -speed) {
				        npc.velocity.X = -speed;
			        }
		        }
        }
        public override void FindFrame(int frameHeight) {
            if(++npc.frameCounter>animationTime) {
                npc.frame = new Rectangle(0, (npc.frame.Y+28)%112, 32, 26);
                npc.frameCounter = 0;
            }
        }
    }

    public class Defiled_Hunter_Body : Defiled_Hunter {
        public override void SetStaticDefaults() {
            Main.npcFrameCount[npc.type] = 6;
            base.SetStaticDefaults();
        }
        public override void SetDefaults() {
            npc.CloneDefaults(NPCID.DiggerBody);
            npc.aiStyle = AIStyleID.None;
            npc.height = 28;
            npc.width = 32;
            npc.noGravity = false;
            npc.noTileCollide = false;
        }
        public override void FindFrame(int frameHeight) {
            if(++npc.frameCounter>animationTime) {
                npc.frame = new Rectangle(0, (npc.frame.Y+32)%192, 32, 30);
                npc.frameCounter = 0;
            }
        }
    }

    public class Defiled_Hunter_Tail : Defiled_Hunter {
        public override void SetDefaults() {
            npc.CloneDefaults(NPCID.DiggerTail);
            npc.aiStyle = AIStyleID.None;
            npc.height = 28;
            npc.width = 32;
            npc.noGravity = false;
            npc.noTileCollide = false;
        }
        public override void FindFrame(int frameHeight) {
            npc.frame = new Rectangle(0, 0, 32, 30);
        }
    }

    public abstract class Defiled_Hunter : ModNPC {
        protected const int animationTime = 4;
        protected internal int[] ai = new int[4];
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Defiled Hunter");
            NPCID.Sets.TrailCacheLength[npc.type] = 2;
            NPCID.Sets.TrailingMode[npc.type] = 3;
        }

        public override void AI() {
            NPC head = Main.npc[npc.realLife];
            npc.life = head.active ? npc.lifeMax : 0;
            npc.immune = head.immune;
            if(Main.netMode != NetmodeID.MultiplayerClient) {
                NPC next = Main.npc[ai[1]];
                if(Math.Abs(AngleDif(npc.rotation, next.oldRot[1]))>MathHelper.PiOver2) {
                    npc.rotation = next.oldRot[1];
                } else {
                    LinearSmoothing(ref npc.rotation, next.oldRot[1], npc.collideY?0.1f:0.3f);
                }
                //Vector2 targetPos = next.oldPosition + new Vector2(next.width/2, next.height/2) - new Vector2(24, 0).RotatedBy(npc.rotation);;
                //npc.velocity = targetPos - npc.Center;

                Vector2 targetPos = next.oldPosition - new Vector2(24, 0).RotatedBy(npc.rotation);
                Vector2 unit = new Vector2(1,0).RotatedBy(npc.rotation);
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
                npc.position = targetPos + offset - new Vector2(0, 8);
                //LerpSmoothing(ref npc.position, targetPos + offset - new Vector2(0,8), 0.9f, 4);
                //Vector2 center = npc.Center;
                //float dist = Math.Max((targetPos-center).Length()-8, 0);
                //LinearSmoothing(ref center.X, targetPos.X, 1+dist);
                //LinearSmoothing(ref center.Y, targetPos.Y, 1+dist);
                //npc.Center = center;
                //npc.Center = next.oldPosition + new Vector2(next.width/2, next.height/2) - new Vector2(24, 0).RotatedBy(npc.rotation);
            }
        }

        public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit) {
            if(npc.realLife!=npc.whoAmI) {
                if(projectile.usesLocalNPCImmunity) {
                    projectile.localNPCImmunity[npc.realLife] = projectile.localNPCHitCooldown;
                    projectile.localNPCImmunity[npc.whoAmI] = 0;
                } else {
                    Main.npc[npc.realLife].immune[projectile.owner] = npc.immune[projectile.owner];
                }
            }
        }
        public override bool? CanBeHitByProjectile(Projectile projectile) {
            if(npc.realLife==npc.whoAmI)return null;
            if((projectile.usesLocalNPCImmunity?projectile.localNPCImmunity[npc.realLife]:Main.npc[npc.realLife].immune[projectile.owner])>0) {
                return false;
            }
            return null;
        }
        public override void HitEffect(int hitDirection, double damage) {
            if(npc.life<0) {
                Defiled_Hunter current = Main.npc[npc.realLife].modNPC as Defiled_Hunter;
                while(current.ai[0]!=0) {
                    deathEffect(current.npc);
                    current = Main.npc[current.ai[0]].modNPC as Defiled_Hunter;
                }
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor) {
            Rectangle frame = npc.frame;
            spriteBatch.Draw(Main.npcTexture[npc.type], npc.Center-Main.screenPosition, frame, drawColor, npc.rotation, new Vector2(frame.Width/2f,frame.Height/2f), npc.scale, SpriteEffects.FlipHorizontally|(npc.spriteDirection>0?SpriteEffects.None:SpriteEffects.FlipVertically), 1);
            return false;
        }
        protected static void deathEffect(NPC npc) {
            Gore.NewGore(npc.position, npc.velocity, Origins.instance.GetGoreSlot("Gores/NPCs/DF2_Gore"));
            Gore.NewGore(npc.position, npc.velocity, Origins.instance.GetGoreSlot("Gores/NPCs/DF_Effect_Small"+Main.rand.Next(1,4)));
        }
        public override void SendExtraAI(BinaryWriter writer) {
            writer.Write(ai[0]);
            writer.Write(ai[1]);
            writer.Write(ai[2]);
            writer.Write(ai[3]);
        }
        public override void ReceiveExtraAI(BinaryReader reader) {
            ai[0] = reader.ReadInt32();
            ai[1] = reader.ReadInt32();
            ai[2] = reader.ReadInt32();
            ai[3] = reader.ReadInt32();
        }
    }
}