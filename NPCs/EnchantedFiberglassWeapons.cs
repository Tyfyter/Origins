using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Weapons.Fiberglass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using static Origins.OriginExtensions;

namespace Origins.NPCs {
    public class Enchanted_Fiberglass_Bow : ModNPC {
        Color[] oldColor = new Color[10];
        int[] oldDir = new int[10];
        public override void SetStaticDefaults() {
            NPCID.Sets.TrailingMode[npc.type] = 3;
        }
        public override void SetDefaults() {
            npc.CloneDefaults(NPCID.PossessedArmor);
            npc.noGravity = true;
            npc.damage = 10;
            npc.life = npc.lifeMax = 95;
            npc.defense = 10;
            npc.aiStyle = 0;//104;//10,
            npc.width = npc.height = 27;
            npc.alpha = 200;
        }
        public override void AI() {
            npc.TargetClosest();
		    npc.spriteDirection = npc.direction;
		    npc.rotation = (npc.Center-Main.player[npc.target].Center).ToRotation();
            Vector2 speed = new Vector2(-12,0).RotatedBy(Main.rand.NextFloat(npc.rotation-0.05f,npc.rotation+0.05f));
            Vector2 pos = npc.Center + speed;
            if(Collision.CanHit(pos, 1, 1, Main.player[npc.target].Center, 1, 1) && Main.netMode != NetmodeID.MultiplayerClient) {
                npc.localAI[0] += 1f;
                if(npc.localAI[0] >= 75f) {
                    Projectile.NewProjectile(pos.X, pos.Y, speed.X, speed.Y, ProjectileID.WoodenArrowHostile, 32, 0f);
                    npc.localAI[0] = 0f;
                    teleport();
                }
            }else npc.localAI[0] = 0f;
            if(npc.spriteDirection == 1)npc.rotation+=MathHelper.Pi;
        }
        public override void OnHitByItem(Player player, Item item, int damage, float knockback, bool crit) {
            npc.localAI[0] = -15f;
            teleport();
        }
        public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit) {
            npc.localAI[0] = 0f;
            teleport();
        }
        public override void NPCLoot() {
            if(Main.rand.Next(10)==0)Item.NewItem(npc.Center, ModContent.ItemType<Broken_Fiberglass_Bow>());
        }
        public override void PostDraw(SpriteBatch spriteBatch, Color drawColor) {
            for(int i = npc.oldPos.Length - 1; i > 0; i--) {
                spriteBatch.Draw(Main.npcTexture[npc.type], npc.oldPos[i] + new Vector2(13.5f,13.5f) - Main.screenPosition, new Rectangle(0,0,18,36), oldColor[i], npc.oldRot[i], new Vector2(9,18), 1f, oldDir[i] != 1?SpriteEffects.None:SpriteEffects.FlipHorizontally, 1f);
                oldDir[i] = oldDir[i - 1];
                oldColor[i] = oldColor[i - 1];
                oldColor[i].A-=5;
            }
		    oldDir[0] = npc.spriteDirection;
		    oldColor[0] = drawColor;
            oldColor[0].A-=5;
        }
        void teleport() {
            float rot = npc.rotation+MathHelper.Pi/2f;
            WeightedRandom<Vector2> options = new WeightedRandom<Vector2>();
            for(int i = 0; i < 16; i++) {
                int length = 0;
                Vector2 unit = new Vector2(4, 0).RotatedBy(rot+Main.rand.NextFloat(-0.05f, 0.05f));
                Vector2 pos = GetLoSLength(Main.player[npc.target].Center, new Point(8,8), unit, new Point(8,8), 75, out length);
                pos-=unit*(length<75 ? 4 : 1);
                if(length>=32) {
                    options.Add(pos, length*Main.rand.NextFloat(0.9f, 1.1f));
                    //Main.npc[NPC.NewNPC((int)pos.X,(int)pos.Y,NPCID.WaterSphere)].velocity = Vector2.Zero; //accidental miniboss if no velocity change
                }
                rot+=MathHelper.Pi/16f;
            }
            if(options.elements.Count==0)for(int i = 0; i < 16; i++) {
                int length = 0;
                Vector2 unit = new Vector2(4, 0).RotatedBy(rot+Main.rand.NextFloat(-0.05f, 0.05f));
                Vector2 pos = GetLoSLength(Main.player[npc.target].Center, new Point(8,8), unit, new Point(8,8), 75, out length);
                pos-=unit*(length<75 ? 4 : 1);
                if(length>=24) {
                    options.Add(pos, length*Main.rand.NextFloat(0.9f, 1.1f));
                    //Main.npc[NPC.NewNPC((int)pos.X,(int)pos.Y,NPCID.WaterSphere)].velocity = Vector2.Zero;
                }
                rot+=MathHelper.Pi/16f;
            }
            if(options.elements.Count==0) {
                //npc.active = false;
                return;
            }
            npc.Center = options.Get();
        }
    }
    public class Enchanted_Fiberglass_Pistol : ModNPC {
        Color[] oldColor = new Color[10];
        int[] oldDir = new int[10];
        public override void SetStaticDefaults() {
            NPCID.Sets.TrailingMode[npc.type] = 3;
        }
        public override void SetDefaults() {
            npc.CloneDefaults(NPCID.PossessedArmor);
            npc.aiStyle = 0;
            npc.damage = 10;
            npc.life = npc.lifeMax = 57;
            npc.defense = 10;
            npc.noGravity = true;
            npc.width = npc.height = 27;
        }
        public override void AI() {
            npc.TargetClosest();
		    npc.spriteDirection = npc.direction;
		    npc.rotation = (npc.Center-Main.player[npc.target].Center).ToRotation();
            if(Collision.CanHit(npc.Center, 1, 1, Main.player[npc.target].Center, 1, 1) && Main.netMode != NetmodeID.MultiplayerClient) {
                npc.localAI[0] += 1f;
                if(npc.rotation == npc.oldRot[0])npc.localAI[0] += 2f;
                if(npc.localAI[0] >= 180f) {
                    Vector2 speed = new Vector2(-8,0).RotatedBy(Main.rand.NextFloat(npc.rotation-0.1f,npc.rotation+0.1f));
                    Projectile.NewProjectile(npc.Center.X, npc.Center.Y, speed.X, speed.Y, ProjectileID.BulletDeadeye, 32, 0f);
                    npc.life = 0;
                    npc.checkDead();
                }
            }else npc.localAI[0] = 0f;
            if(npc.spriteDirection == 1)npc.rotation+=MathHelper.Pi;
        }
        public override void NPCLoot() {
            if(npc.localAI[0] < 180f) {
                Item.NewItem(npc.Center, ModContent.ItemType<Fiberglass_Shard>(), Main.rand.Next(4)+4);
            } else {
                Vector2 speed = new Vector2(8,0).RotatedBy(npc.rotation);
                for(int i = Main.rand.Next(4,7); i >= 0; i--) {
                    speed = speed.RotatedByRandom(0.25f);
                    int proj =
                    Projectile.NewProjectile(npc.Center, speed, ModContent.ProjectileType<Fiberglass_Shard_Proj>(), 17, 3f, npc.target);
                    Main.projectile[proj].hostile = true;
                    Main.projectile[proj].friendly = false;
                    Main.projectile[proj].hide = false;
                }
            }
        }
        public override void PostDraw(SpriteBatch spriteBatch, Color drawColor) {
            for(int i = npc.oldPos.Length - 1; i > 0; i--) {
                spriteBatch.Draw(Main.npcTexture[npc.type], npc.oldPos[i] + new Vector2(13.5f,19) - Main.screenPosition, new Rectangle(0,0,38,22), oldColor[i], npc.oldRot[i], new Vector2(19,11), 1f, oldDir[i] != 1?SpriteEffects.None:SpriteEffects.FlipHorizontally, 1f);
                oldDir[i] = oldDir[i - 1];
                oldColor[i] = oldColor[i - 1];
                oldColor[i].A-=5;
            }
		    oldDir[0] = npc.spriteDirection;
		    oldColor[0] = drawColor;
            oldColor[0].A-=5;
        }
    }
    public class Enchanted_Fiberglass_Sword : ModNPC {
        Color[] oldColor = new Color[10];
        int[] oldDir = new int[10];
        int stuck = 0;
        Vector2 stuckVel = Vector2.Zero;
        public override void SetStaticDefaults() {
            NPCID.Sets.TrailingMode[npc.type] = 3;
        }
        public override void SetDefaults() {
            npc.CloneDefaults(NPCID.PossessedArmor);
            npc.damage = 10;
            npc.life = npc.lifeMax = 110;
            npc.aiStyle = 22;
            npc.noGravity = true;
            npc.knockBackResist/=4;
            npc.width = npc.height = 42;
            npc.HitSound = SoundID.DD2_CrystalCartImpact;
            //npc.DeathSound = SoundID.DD2_DefeatScene;
        }
        public override bool PreAI() {
            if(stuck>0) {
                stuck--;
                if(stuck<=0) {
                    npc.noTileCollide = false;
                    npc.velocity = -stuckVel/3;
                    npc.position+=npc.velocity;
                }
                return false;
            }
            return true;
        }
        public override bool CanHitPlayer(Player target, ref int cooldownSlot) {
            if(stuck>0)return false;
            return base.CanHitPlayer(target, ref cooldownSlot);
        }
        public override void AI() {
            if(npc.localAI[0]<30 && stuck <= 0) {
                npc.TargetClosest();
            }
            npc.damage = stuck>0?0:npc.localAI[0]>30?50:10;
            npc.defense = stuck>0?0:npc.localAI[0]>30?10:20;
		    npc.spriteDirection = npc.direction;
            float targetRot = npc.rotation;
            float rotSpeed = 0.15f;
            Player target = npc.HasValidTarget?Main.player[npc.target]:null;
            if(npc.HasValidTarget && (npc.Center-target.Center).Length()<80+42 && Main.netMode != NetmodeID.MultiplayerClient) {
                npc.localAI[0] += 1f;
                if(npc.localAI[0]<30) {
                    bool close = (npc.Center-Main.player[npc.target].Center).Length()<16+42;
                    targetRot = -1.5f*npc.direction;
                    if(npc.direction == -1) {
                        npc.velocity.X -= 0.2f;
                        if(npc.velocity.X < (close?target.velocity.X-0.2f:-1)) {
                            npc.velocity.X = (close?target.velocity.X-0.2f:-1);
                        }
                        if(npc.velocity.X > 1) {
                            npc.velocity.X = 1;
                        }
                    } else if(npc.direction == 1) {
                        npc.velocity.X += 0.2f;
                        if(npc.velocity.X > 1) {
                            npc.velocity.X = 1;
                        }
                        if(npc.velocity.X < (close?target.velocity.X+0.2f:-1)) {
                            npc.velocity.X = (close?target.velocity.X+0.2f:-1);
                        }
                    }
                } else if(npc.localAI[0]<60) {
                    bool close = npc.direction != oldDir[0];
                    rotSpeed = 0.3f;
                    if(!close) {
                        targetRot = 3*npc.direction;
                        if(npc.direction == -1) {
                            npc.velocity.X -= 0.4f;
                        } else if(npc.direction == 1) {
                            npc.velocity.X += 0.4f;
                        }
                    } else {
                        npc.direction = oldDir[0];
                        npc.localAI[0] = 90;
                        targetRot-=rotSpeed*npc.direction;
                        npc.velocity = npc.oldVelocity;
                        npc.aiStyle = 0;
                        if(npc.collideX) {
                            getStuck();
                        }
                    }
                } else {
                    rotSpeed = 0.3f;
                    npc.localAI[0] = 90;
                    targetRot-=rotSpeed*npc.direction;
                    npc.velocity = npc.oldVelocity;
                    npc.aiStyle = 0;
                    if(npc.collideX) {
                        getStuck();
                    }
                }
            } else {
                if(npc.localAI[0]<30) {
                    npc.localAI[0] = 0f;
                    targetRot = 0f;
                    npc.aiStyle = 22;
                } else {
                    rotSpeed = 0.3f;
                    targetRot-=rotSpeed*npc.direction;
                    npc.localAI[0]--;
                    npc.aiStyle = 0;
                    npc.velocity = npc.oldVelocity;
                    npc.target = -1;
                    if(npc.collideX) {
                        getStuck();
                    }
                    if(npc.localAI[0]<30) npc.rotation%=MathHelper.Pi;
                }
            }
            npc.rotation+=MathHelper.Pi/2f;
            targetRot+=MathHelper.Pi/2f;
            if(npc.rotation>targetRot) {
                npc.rotation-=rotSpeed;
                if(npc.rotation<targetRot)npc.rotation = targetRot;
            }else if(npc.rotation<targetRot) {
                npc.rotation+=rotSpeed;
                if(npc.rotation>targetRot)npc.rotation = targetRot;
            }
            targetRot-=MathHelper.Pi/2f;
            npc.rotation-=MathHelper.Pi/2f;
        }
        void getStuck() {
            stuck = Main.rand.Next(80, 100);
            npc.position+=npc.velocity;
            stuckVel = npc.velocity;
            npc.velocity*=0;
            npc.localAI[0] = 0;
            npc.rotation%=MathHelper.Pi;
            npc.noTileCollide = true;
        }
        public override void PostDraw(SpriteBatch spriteBatch, Color drawColor) {
            for(int i = npc.oldPos.Length - 1; i > 0; i--) {
                spriteBatch.Draw(Main.npcTexture[npc.type], npc.oldPos[i] + new Vector2(21,21) - Main.screenPosition, new Rectangle(0,0,42,42), oldColor[i], npc.oldRot[i], new Vector2(21,21), 1f, oldDir[i] != 1?SpriteEffects.None:SpriteEffects.FlipHorizontally, 1f);
                oldDir[i] = oldDir[i - 1];
                oldColor[i] = oldColor[i - 1];
                oldColor[i].A-=10;
            }
		    oldDir[0] = npc.spriteDirection;
		    oldColor[0] = drawColor;
            oldColor[0].A-=10;
        }
    }
}
