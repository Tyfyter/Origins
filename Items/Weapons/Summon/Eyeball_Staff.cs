using Microsoft.Xna.Framework;
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
using static Origins.OriginExtensions;

namespace Origins.Items.Weapons.Summon {
    public class Eyeball_Staff : ModItem {
        internal static int projectileID = 0;
        internal static int buffID = 0;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Eyeball Staff");
            Tooltip.SetDefault("Summons a mini Eye of Cthulhu to fight for you\nCan summon 2 minions per slot");
            ItemID.Sets.StaffMinionSlotsRequired[item.type] = 1;
        }
        public override void SetDefaults() {
            item.damage = 12;
            item.mana = 10;
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
            if(buffID==0)buffID = ModContent.BuffType<Mini_EOC_Buff>();
            player.AddBuff(item.buffType, 2);
            position = Main.MouseWorld;
            return true;
        }
    }
}
namespace Origins.Buffs {
    public class Mini_EOC_Buff : ModBuff {
        public override void SetDefaults() {
            DisplayName.SetDefault("Mini Eye of Cthulhu");
            Description.SetDefault("The Eye of Cthulhu will fight for you");
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
            Eyeball_Staff.buffID = Type;
        }

        public override void Update(Player player, ref int buffIndex) {
            if(player.ownedProjectileCounts[Eyeball_Staff.projectileID] > 0) {
                player.buffTime[buffIndex] = 18000;
            } else {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
}
namespace Origins.Items.Weapons.Summon.Minions {
    public class Mini_EOC : ModProjectile {
		public const int frameSpeed = 5;
		public override void SetStaticDefaults() {
            Eyeball_Staff.projectileID = projectile.type;
			DisplayName.SetDefault("Mini Eye of Cthulhu");
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[projectile.type] = 4;
			// This is necessary for right-click targeting
			ProjectileID.Sets.MinionTargettingFeature[projectile.type] = true;

			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[projectile.type] = true;
			// This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned
			ProjectileID.Sets.MinionSacrificable[projectile.type] = true;
			ProjectileID.Sets.Homing[projectile.type] = true;
		}

		public sealed override void SetDefaults() {
			projectile.width = 28;
			projectile.height = 28;
			projectile.tileCollide = true;
			projectile.friendly = true;
			projectile.minion = true;
			projectile.minionSlots = 0f;
			projectile.penetrate = -1;
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = 12;
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
				player.ClearBuff(Eyeball_Staff.buffID);
			}
			if (player.HasBuff(Eyeball_Staff.buffID)) {
				projectile.timeLeft = 2;
			}
            OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
            originPlayer.minionSubSlots[0]+=0.5f;
            int eyeCount = player.ownedProjectileCounts[Eyeball_Staff.projectileID]/2;
            if(originPlayer.minionSubSlots[0]<=eyeCount) {
                projectile.minionSlots = 0.5f;
            } else {
                projectile.minionSlots = 0;
            }
            #endregion

            #region General behavior
            Vector2 idlePosition = player.Top+new Vector2(player.direction*-player.width/2, 0);
            idlePosition.X -= 48f*player.direction;

			// Teleport to player if distance is too big
			Vector2 vectorToIdlePosition = idlePosition - projectile.Center;
			float distanceToIdlePosition = vectorToIdlePosition.Length();
			if (Main.myPlayer == player.whoAmI && distanceToIdlePosition > 2000f) {
				// Whenever you deal with non-regular events that change the behavior or position drastically, make sure to only run the code on the owner of the projectile,
				// and then set netUpdate to true
				projectile.position = idlePosition;
				projectile.velocity *= 0.1f;
				projectile.netUpdate = true;
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
			float targetDist = 700f;
			float targetAngle = -2;
			Vector2 targetCenter = projectile.Center;
            int target = -1;
			bool foundTarget = false;
            if(projectile.ai[1]<0) goto movement;

			if (player.HasMinionAttackTargetNPC) {
				NPC npc = Main.npc[player.MinionAttackTargetNPC];
				float dist = Vector2.Distance(npc.Center, projectile.Center);
				if (dist < 2000f) {
					targetDist = dist;
					targetCenter = npc.Center;
                    target = player.MinionAttackTargetNPC;
					foundTarget = true;
				}
			}
			if (!foundTarget) {
				for (int i = 0; i < Main.maxNPCs; i++) {
					NPC npc = Main.npc[i];
					if (npc.CanBeChasedBy()) {
                        Vector2 diff = projectile.Center-projectile.Center;
                        float dist = diff.Length();
						if(dist>targetDist)continue;
						float dot = NormDot(diff,projectile.velocity);
						bool inRange = dist < targetDist;
						bool lineOfSight = Collision.CanHitLine(projectile.position, projectile.width, projectile.height, npc.position, npc.width, npc.height);
                        if (((dot>targetAngle && inRange) || !foundTarget) && lineOfSight) {
                            targetDist = dist;
                            targetAngle = dot;
							targetCenter = npc.Center;
                            target = npc.whoAmI;
							foundTarget = true;
						}
					}
				}
			}

			projectile.friendly = foundTarget;
            #endregion

            #region Movement
            movement:
            // Default movement parameters (here for attacking)
            float speed = 6f+projectile.localAI[0]/15;
            float turnSpeed = 1f+Math.Max((projectile.localAI[0]-15)/30,0);
			float currentSpeed = projectile.velocity.Length();
            projectile.tileCollide = true;
            if(foundTarget) {
                projectile.tileCollide = true;
                if(projectile.ai[0] != target) {
                    projectile.ai[0] = target;
                    projectile.ai[1] = 0;
                } else {
                    if(++projectile.ai[1]>180) {
                        projectile.ai[1] = -60;
                    }
                }
                if((int)Math.Ceiling(targetAngle)==-1) {
                    targetCenter.Y-=16;
                }
            } else {
				if (distanceToIdlePosition > 640f) {
                    projectile.tileCollide = false;
					speed = 16f;
				} else if (distanceToIdlePosition < 64f) {
					speed = 4f;
                    turnSpeed = 0;
				} else {
					speed = 6f;
				}
                if(!Collision.CanHitLine(projectile.position, projectile.width, projectile.height, idlePosition, 1, 1)) {
                    projectile.tileCollide = false;
                }
            }
            LinearSmoothing(ref currentSpeed, speed, currentSpeed<1?1:0.1f+projectile.localAI[0]/90f);
            Vector2 direction = foundTarget?targetCenter - projectile.Center:vectorToIdlePosition;
			direction.Normalize();
            projectile.velocity = Vector2.Normalize(projectile.velocity+direction*turnSpeed)*currentSpeed;
            #endregion

            #region Animation and visuals
            // So it will lean slightly towards the direction it's moving
            projectile.rotation = (float)Math.Atan(projectile.velocity.Y/projectile.velocity.X);
            projectile.spriteDirection = Math.Sign(projectile.velocity.X);

			// This is a simple "loop through all frames from top to bottom" animation
			projectile.frameCounter++;
			if (projectile.frameCounter >= frameSpeed) {
				projectile.frameCounter = 0;
				projectile.frame++;
				if (projectile.frame >= Main.projFrames[projectile.type]) {
					projectile.frame = 0;
				}
			}
            #endregion
            if(projectile.localAI[0]>0)projectile.localAI[0]--;
		}
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            damage+=(int)(projectile.localAI[0]/3);
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            Vector2 intersect = Rectangle.Intersect(projectile.Hitbox,target.Hitbox).Center.ToVector2()-projectile.Hitbox.Center.ToVector2();
            if(intersect.X!=0&&(Math.Sign(intersect.X)==Math.Sign(projectile.velocity.X))) {
                projectile.velocity.X = -projectile.velocity.X;
            }
            if(intersect.Y!=0&&(Math.Sign(intersect.Y)==Math.Sign(projectile.velocity.Y))) {
                projectile.velocity.Y = -projectile.velocity.Y;
            }
            projectile.localAI[0]+=30-projectile.localAI[0]/3;
            projectile.ai[1] = 0;
        }
    }
}
