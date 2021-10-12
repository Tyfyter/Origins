using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Items.Weapons.Summon;
using Origins.Items.Weapons.Summon.Minions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Summon {
    public class Bomb_Artifact : ModItem {
        internal static int projectileID = 0;
        internal static int buffID = 0;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Bomb Artifact");
            Tooltip.SetDefault("Summons a friendly bomb to fight for you");
            Origins.ExplosiveItems[item.type] = true;
        }
        public override void SetDefaults() {
            item.damage = 80;
            item.mana = 20;
            item.width = 32;
            item.height = 32;
            item.useTime = 36;
            item.useAnimation = 36;
            item.useStyle = 1;
            item.value = Item.buyPrice(0, 30, 0, 0);
            item.rare = ItemRarityID.Blue;
            item.UseSound = SoundID.Item44;
            item.buffType = buffID;
            item.shoot = projectileID;
            item.noMelee = true;
            item.summon = true;
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            player.AddBuff(item.buffType, 2);
            position = Main.MouseWorld;
            return true;
        }
    }
}
namespace Origins.Buffs {
    public class Friendly_Bomb_Buff : ModBuff {
        public override void SetDefaults() {
            DisplayName.SetDefault("Friendly Bomb");
            Description.SetDefault("The bomb will fight for you");
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
            Bomb_Artifact.buffID = Type;
        }

        public override void Update(Player player, ref int buffIndex) {
            if(player.ownedProjectileCounts[Bomb_Artifact.projectileID] > 0) {
                player.buffTime[buffIndex] = 18000;
            } else {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
}
namespace Origins.Items.Weapons.Summon.Minions {
    public class Friendly_Bomb : ModProjectile {
        public override string Texture => "Origins/Items/Weapons/Summon/Minions/Happy_Boi";
        public bool OnGround {
            get {
                return projectile.localAI[1] > 0;
            }
            set {
                projectile.localAI[1] = value ? 2 : 0;
            }
        }
        public sbyte CollidingX {
            get {
                return (sbyte)projectile.localAI[0];
            }
            set {
                projectile.localAI[0] = value;
            }
        }
        public override void SetStaticDefaults() {
            Bomb_Artifact.projectileID = projectile.type;
			DisplayName.SetDefault("Friendly Bomb");
            Origins.ExplosiveProjectiles[projectile.type] = true;
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 11;
			// This is necessary for right-click targeting
			ProjectileID.Sets.MinionTargettingFeature[projectile.type] = true;

			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[projectile.type] = true;
			// This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned
			ProjectileID.Sets.MinionSacrificable[projectile.type] = true;
		}

		public sealed override void SetDefaults() {
			projectile.width = 30;
			projectile.height = 48;
			projectile.tileCollide = true;
			projectile.friendly = false;
			projectile.minion = true;
			projectile.minionSlots = 1f;
			projectile.penetrate = -1;
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = 1;
            projectile.ignoreWater = false;
		}

		// Here you can decide if your minion breaks things like grass or pots
		public override bool? CanCutTiles() {
			return false;
		}

		// This is mandatory if your minion deals contact damage (further related stuff in AI() in the Movement region)
		public override bool MinionContactDamage() {
			return true;
		}

		public override void AI() {
			Player player = Main.player[projectile.owner];

			#region Active check
			// This is the "active check", makes sure the minion is alive while the player is alive, and despawns if not
			if (player.dead || !player.active) {
				player.ClearBuff(Bomb_Artifact.buffID);
			}
			if (player.HasBuff(Bomb_Artifact.buffID)) {
				projectile.timeLeft = 2;
			}
			#endregion

			#region General behavior
			Vector2 idlePosition = player.Bottom;
            idlePosition.X -= 48f*player.direction;
            idlePosition.Y -= 25*projectile.scale;

			// Teleport to player if distance is too big
			Vector2 vectorToIdlePosition = (idlePosition + new Vector2(6*player.direction, 0)) - projectile.Center;
			float distanceToIdlePosition = vectorToIdlePosition.Length();
			if (distanceToIdlePosition > 1000f) {
                projectile.ai[1]++;
                if(projectile.ai[1] > 3600f) {
                    //TODO:if abandoned, bombs get angry at their owner and become a persistent enemy that will spawn around abandoned location and target their previous owner
                }
			    if (Main.myPlayer == player.whoAmI && distanceToIdlePosition > 2000f) {
				    // Whenever you deal with non-regular events that change the behavior or position drastically, make sure to only run the code on the owner of the projectile,
				    // and then set netUpdate to true
				    projectile.position = idlePosition;
				    projectile.velocity *= 0.1f;
				    projectile.netUpdate = true;
			    }
			} else {
                projectile.ai[1] = 0;
            }

			// If your minion is flying, you want to do this independently of any conditions
			float overlapVelocity = 0.04f;
			for (int i = 0; i < Main.maxProjectiles; i++) {
				// Fix overlap with other minions
				Projectile other = Main.projectile[i];
				if (i != projectile.whoAmI && other.active && other.owner == projectile.owner && Math.Abs(projectile.position.X - other.position.X) + Math.Abs(projectile.position.Y - other.position.Y) < projectile.width) {
					if (projectile.position.X < other.position.X) projectile.velocity.X -= overlapVelocity;
					else projectile.velocity.X += overlapVelocity;

					if (projectile.position.Y < other.position.Y) projectile.velocity.Y -= overlapVelocity;
					else projectile.velocity.Y += overlapVelocity;
				}
			}
			#endregion

			#region Find target
			// Starting search distance
			float distanceFromTarget = 700f;
			Vector2 targetCenter = projectile.position;
            int target = -1;
			bool foundTarget = false;

			if (player.HasMinionAttackTargetNPC) {
				NPC npc = Main.npc[player.MinionAttackTargetNPC];
				float between = Vector2.Distance(npc.Center, projectile.Center);
				if (between < 2000f) {
					distanceFromTarget = between;
					targetCenter = npc.Center;
                    target = player.MinionAttackTargetNPC;
					foundTarget = true;
				}
			}
			if (!foundTarget) {
				for (int i = 0; i < Main.maxNPCs; i++) {
					NPC npc = Main.npc[i];
					if (npc.CanBeChasedBy()) {
						float between = Vector2.Distance(npc.Center, projectile.Center);
						bool closest = Vector2.Distance(projectile.Center, targetCenter) > between;
						bool inRange = between < distanceFromTarget;
						bool lineOfSight = Collision.CanHitLine(projectile.position, projectile.width, projectile.height, npc.position, npc.width, npc.height);
						// Additional check for this specific minion behavior, otherwise it will stop attacking once it dashed through an enemy while flying though tiles afterwards
						// The number depends on various parameters seen in the movement code below. Test different ones out until it works alright
						bool closeThroughWall = between < 100f;
						if (((closest && inRange) || !foundTarget) && (lineOfSight || closeThroughWall)) {
							distanceFromTarget = between;
							targetCenter = npc.Center;
                            target = npc.whoAmI;
							foundTarget = true;
						}
					}
				}
			}

			//projectile.friendly = foundTarget;
			#endregion

			#region Movement

			// Default movement parameters (here for attacking)
			float speed = 8f;
			float inertia = 6f;

			if (foundTarget) {
                // Minion has a target: attack (here, fly towards the enemy)
				Vector2 direction = targetCenter - projectile.Center;
                int directionX = Math.Sign(direction.X);
                projectile.spriteDirection = directionX;
                bool wallColliding = CollidingX == directionX;
                float YRatio = direction.Y / ((direction.X * -directionX)+0.1f);
                if(direction.Y < 160 && (wallColliding || YRatio > 1) && OnGround) {
                    float jumpStrength = 6;
                    if(wallColliding) {
                        if(Collision.TileCollision(projectile.position - new Vector2(18), Vector2.UnitX, projectile.width, projectile.height, false, false).X == 0) {
                            jumpStrength++;
                            if(Collision.TileCollision(projectile.position - new Vector2(36), Vector2.UnitX, projectile.width, projectile.height, false, false).X == 0) {
                                jumpStrength++;
                            }
                        }
                    } else {
                        if(YRatio>1.1f) {
                            jumpStrength++;
                            if(YRatio>1.2f) {
                                jumpStrength++;
                                if(YRatio>1.3f) {
                                    jumpStrength++;
                                }
                            }
                        }
                    }
                    projectile.velocity.Y = -jumpStrength;
                }
                Rectangle hitbox = new Rectangle((int)projectile.Center.X-24, (int)projectile.Center.Y-24, 48, 48);
                NPC targetNPC = Main.npc[target];
                int targetMovDir = Math.Sign(targetNPC.velocity.X);
                Rectangle targetFutureHitbox = targetNPC.Hitbox;
                targetFutureHitbox.X += (int)(targetNPC.velocity.X*10);
                bool waitForStart = (targetMovDir == directionX && !hitbox.Intersects(targetFutureHitbox));
                if(distanceFromTarget > 48f || !hitbox.Intersects(targetNPC.Hitbox) || (waitForStart && projectile.ai[0] <= 0f)) {
                    projectile.ai[0] = 0f;
				    direction.Normalize();
				    direction *= speed;
				    projectile.velocity.X = (projectile.velocity.X * (inertia - 1) + direction.X) / inertia;
                } else {
					projectile.velocity.X = (projectile.velocity.X * (inertia - 1)) / inertia;
                    if(projectile.ai[0] <= 0f) {
                        projectile.ai[0] = 1;
                    }
                }
			} else {
				if (distanceToIdlePosition > 600f) {
					speed = 16f;
					inertia = 12f;
				} else {
					speed = 6f;
					inertia = 12f;
				}
                int direction = Math.Sign(vectorToIdlePosition.X);
                projectile.spriteDirection = direction;
                if(vectorToIdlePosition.Y < 160 && CollidingX == direction && OnGround) {
                    float jumpStrength = 6;
                    if(Collision.TileCollision(projectile.position - new Vector2(0, 18), new Vector2(4 * direction, 0), projectile.width, projectile.height, false, false).X==0) {
                        jumpStrength+=2;
                        if(Collision.TileCollision(projectile.position-new Vector2(0, 36), new Vector2(4*direction, 0), projectile.width, projectile.height, false, false).X==0) {
                            jumpStrength+=2;
                        }
                    }
                    projectile.velocity.Y = -jumpStrength;
                }
				if (distanceToIdlePosition > 12f) {
					// The immediate range around the player (when it passively floats about)

					// This is a simple movement formula using the two parameters and its desired direction to create a "homing" movement
					vectorToIdlePosition.Normalize();
					vectorToIdlePosition *= speed;
					projectile.velocity.X = (projectile.velocity.X * (inertia - 1) + vectorToIdlePosition.X) / inertia;
                } else {
					projectile.velocity.X = (projectile.velocity.X * (inertia - 1)) / inertia;
                }
			}
            #endregion

            //gravity
            projectile.velocity.Y += 0.4f;

            #region Animation and visuals
            if(projectile.ai[0]>0) {
                projectile.frame = 7+(int)(projectile.ai[0]/6f);
                if(++projectile.ai[0]>30) {
                    //Projectile.NewProjectile(projectile.Center, Vector2.Zero, ProjectileID.SolarWhipSwordExplosion, projectile.damage, 0, projectile.owner, 1, 1);
                    projectile.Kill();
                }
            }else if(OnGround) {
                projectile.localAI[1]--;
                const int frameSpeed = 4;
                if(Math.Abs(projectile.velocity.X)<0.01f) {
                    projectile.velocity.X = 0f;
                }
                if((projectile.velocity.X!=0)^(projectile.oldVelocity.X!=0)) {
                    projectile.frameCounter = 0;
                }
                if(projectile.velocity.X!=0) {
                    projectile.frameCounter++;
                    if(projectile.frameCounter >= frameSpeed) {
                        projectile.frameCounter = 0;
                        projectile.frame++;
                        if(projectile.frame >= 6) {
                            projectile.frame = 0;
                        }
                    }
                } else {
                    projectile.frameCounter++;
                    if(projectile.frameCounter >= frameSpeed) {
                        projectile.frameCounter = 0;
                        projectile.frame = 0;
                    }
                }
            }else if(projectile.frame > 6) {
                projectile.frame = 1;
            }

            // Some visuals here
            if(projectile.frame<7) {
			    Lighting.AddLight(projectile.Center, Color.Green.ToVector3() * 0.18f);
            } else if(projectile.frame<9) {
			    Lighting.AddLight(projectile.Center, Color.Red.ToVector3() * 0.24f);
            }
			#endregion
		}

        public override bool PreKill(int timeLeft) {
            projectile.type = ProjectileID.Grenade;
            return true;
        }
        public override void Kill(int timeLeft) {
            projectile.friendly = true;
			projectile.position.X += projectile.width / 2;
			projectile.position.Y += projectile.height / 2;
			projectile.width = 128;
			projectile.height = 128;
			projectile.position.X -= projectile.width / 2;
			projectile.position.Y -= projectile.height / 2;
			projectile.Damage();
        }

        public override bool OnTileCollide(Vector2 oldVelocity) {
            if(oldVelocity.Y > projectile.velocity.Y) {
                OnGround = true;
            } else {
                if(Collision.SlopeCollision(projectile.position, new Vector2(0, 4), projectile.width, projectile.height).Y!=4) {
                    OnGround = true;
                }
            }
            if(oldVelocity.X > projectile.velocity.X) {
                CollidingX = (sbyte)(1-Collision.TileCollision(projectile.position, Vector2.UnitX, projectile.width, projectile.height, false, false).X);
            }else if(oldVelocity.X < projectile.velocity.X) {
                CollidingX = (sbyte)(-1-Collision.TileCollision(projectile.position, -Vector2.UnitX, projectile.width, projectile.height, false, false).X);
            }else {
                CollidingX = 0;
            }
            return true;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            Texture2D texture = Main.projectileTexture[projectile.type];
            spriteBatch.Draw(
                texture,
                projectile.Center - Main.screenPosition,
                new Rectangle(0, projectile.frame*52, 56, 50),
                lightColor,
                projectile.rotation,
                new Vector2(28, 25),
                projectile.scale,
                projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                0);
            spriteBatch.Draw(
                ModContent.GetTexture("Origins/Items/Weapons/Summon/Minions/Happy_Boi_Glow"),
                projectile.Center - Main.screenPosition,
                new Rectangle(0, projectile.frame*52, 56, 50),
                Color.White,
                projectile.rotation,
                new Vector2(28, 25),
                projectile.scale,
                projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                0);
            return false;
        }
    }
}
