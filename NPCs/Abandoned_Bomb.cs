///TODO:this
/*using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs {
	public class Abandoned_Bomb : ModNPC {
        public override string Texture => "Origins/Items/Weapons/Summon/Minions/Happy_Boi";
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Abandoned Bomb");
            //Origins.ExplosiveProjectiles[Projectile.type] = true;
            // Sets the amount of frames this minion has on its spritesheet
            Main.npcFrameCount[Type] = 11;
        }

        public sealed override void SetDefaults() {
            NPC.width = 30;
            NPC.height = 48;
            NPC.noTileCollide = false;
            NPC.friendly = false;
        }

        public override void AI() {

            #region Find target
            // Starting search distance
            float distanceFromTarget = 700f;
            Vector2 targetCenter = NPC.position;
            int target = -1;
            bool foundTarget = false;

            //projectile.friendly = foundTarget;
            #endregion

            #region Movement

            // Default movement parameters (here for attacking)
            float speed = 8f;
            float inertia = 6f;

            if (foundTarget) {
                // Minion has a target: attack (here, fly towards the enemy)
                Vector2 direction = targetCenter - NPC.Center;
                int directionX = Math.Sign(direction.X);
                NPC.spriteDirection = directionX;
                bool wallColliding = NPC.collideX;
                float YRatio = direction.Y / ((direction.X * -directionX) + 0.1f);
                if (direction.Y < 160 && (wallColliding || YRatio > 1) && NPC.collideY) {
                    float jumpStrength = 6;
                    if (wallColliding) {
                        if (Collision.TileCollision(NPC.position - new Vector2(18), Vector2.UnitX, NPC.width, NPC.height, false, false).X == 0) {
                            jumpStrength++;
                            if (Collision.TileCollision(NPC.position - new Vector2(36), Vector2.UnitX, NPC.width, NPC.height, false, false).X == 0) {
                                jumpStrength++;
                            }
                        }
                    } else {
                        if (YRatio > 1.1f) {
                            jumpStrength++;
                            if (YRatio > 1.2f) {
                                jumpStrength++;
                                if (YRatio > 1.3f) {
                                    jumpStrength++;
                                }
                            }
                        }
                    }
                    NPC.velocity.Y = -jumpStrength;
                }
                Rectangle hitbox = new Rectangle((int)NPC.Center.X - 24, (int)NPC.Center.Y - 24, 48, 48);
                NPC targetNPC = Main.npc[target];
                int targetMovDir = Math.Sign(targetNPC.velocity.X);
                Rectangle targetFutureHitbox = targetNPC.Hitbox;
                targetFutureHitbox.X += (int)(targetNPC.velocity.X * 10);
                bool waitForStart = (targetMovDir == directionX && !hitbox.Intersects(targetFutureHitbox));
                if (distanceFromTarget > 48f || !hitbox.Intersects(targetNPC.Hitbox) || (waitForStart && NPC.ai[0] <= 0f)) {
                    NPC.ai[0] = 0f;
                    direction.Normalize();
                    direction *= speed;
                    NPC.velocity.X = (NPC.velocity.X * (inertia - 1) + direction.X) / inertia;
                } else {
                    NPC.velocity.X = (NPC.velocity.X * (inertia - 1)) / inertia;
                    if (NPC.ai[0] <= 0f) {
                        NPC.ai[0] = 1;
                    }
                }
            }
            #endregion

            #region Animation and visuals
            if (NPC.ai[0] > 0) {
                NPC.frame = 7 + (int)(NPC.ai[0] / 6f);
                if (++NPC.ai[0] > 30) {
                    //NPC.NewNPC(NPC.Center, Vector2.Zero, NPCID.SolarWhipSwordExplosion, NPC.damage, 0, NPC.owner, 1, 1);
                    NPC.Kill();
                }
            } else if (OnGround) {
                NPC.localAI[1]--;
                const int frameSpeed = 4;
                if (Math.Abs(NPC.velocity.X) < 0.01f) {
                    NPC.velocity.X = 0f;
                }
                if ((NPC.velocity.X != 0) ^ (NPC.oldVelocity.X != 0)) {
                    NPC.frameCounter = 0;
                }
                if (NPC.velocity.X != 0) {
                    NPC.frameCounter++;
                    if (NPC.frameCounter >= frameSpeed) {
                        NPC.frameCounter = 0;
                        NPC.frame++;
                        if (NPC.frame >= 6) {
                            NPC.frame = 0;
                        }
                    }
                } else {
                    NPC.frameCounter++;
                    if (NPC.frameCounter >= frameSpeed) {
                        NPC.frameCounter = 0;
                        NPC.frame = 0;
                    }
                }
            } else if (NPC.frame > 6) {
                NPC.frame = 1;
            }

            // Some visuals here
            if (NPC.frame < 7) {
                Lighting.AddLight(NPC.Center, Color.Green.ToVector3() * 0.18f);
            } else if (NPC.frame < 9) {
                Lighting.AddLight(NPC.Center, Color.Red.ToVector3() * 0.24f);
            }
            #endregion
        }
        public void Kill() {
            NPC.position.X += NPC.width / 2;
            NPC.position.Y += NPC.height / 2;
            NPC.width = 128;
            NPC.height = 128;
            NPC.position.X -= NPC.width / 2;
            NPC.position.Y -= NPC.height / 2;
        }

        public override bool OnTileCollide(Vector2 oldVelocity) {
            if (oldVelocity.Y > NPC.velocity.Y) {
                OnGround = true;
            } else {
                if (Collision.SlopeCollision(NPC.position, new Vector2(0, 4), NPC.width, NPC.height).Y != 4) {
                    OnGround = true;
                }
            }
            if (oldVelocity.X > NPC.velocity.X) {
                CollidingX = (sbyte)(1 - Collision.TileCollision(NPC.position, Vector2.UnitX, NPC.width, NPC.height, false, false).X);
            } else if (oldVelocity.X < NPC.velocity.X) {
                CollidingX = (sbyte)(-1 - Collision.TileCollision(NPC.position, -Vector2.UnitX, NPC.width, NPC.height, false, false).X);
            } else {
                CollidingX = 0;
            }
            return true;
        }
        public override bool PreDraw(ref Color lightColor) {
            Texture2D texture = TextureAssets.NPC[NPC.type].Value;
            Main.EntitySpriteDraw(
                texture,
                NPC.Center - Main.screenPosition,
                new Rectangle(0, NPC.frame * 52, 56, 50),
                lightColor,
                NPC.rotation,
                new Vector2(28, 25),
                NPC.scale,
                NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                0);
            Main.EntitySpriteDraw(
                Mod.Assets.Request<Texture2D>("Items/Weapons/Summon/Minions/Happy_Boi_Glow").Value,
                NPC.Center - Main.screenPosition,
                new Rectangle(0, NPC.frame * 52, 56, 50),
                Color.White,
                NPC.rotation,
                new Vector2(28, 25),
                NPC.scale,
                NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                0);
            return false;
        }
    }
}*/