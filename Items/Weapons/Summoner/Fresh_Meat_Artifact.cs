﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Weapons.Summoner;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.Dev;
using System.IO;
using Origins.Items.Weapons.Summoner.Minions;
using Origins.Buffs;
using Terraria.Graphics.Shaders;
using Origins.Projectiles;
using Terraria.Audio;

namespace Origins.Items.Weapons.Summoner {
	public class Fresh_Meat_Artifact : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Artifact",
			"Minion"
		];
		public override void SetStaticDefaults() {
			ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // This lets the player target anywhere on the whole screen while using a controller
			ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.damage = 30;
			Item.DamageType = DamageClass.Summon;
			Item.mana = 17;
			Item.shootSpeed = 8f;
			Item.width = 24;
			Item.height = 38;
			Item.useTime = 24;
			Item.useAnimation = 24;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noUseGraphic = true;
			Item.value = Item.sellPrice(gold: 1, silver: 50);
			Item.rare = ItemRarityID.Pink;
			Item.UseSound = SoundID.Item44;
			Item.buffType = Fresh_Meat_Buff.ID;
			Item.shoot = Fresh_Meat_Artifact_P.ID;
			Item.noMelee = true;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			player.AddBuff(Item.buffType, 2);
			Projectile projectile = Projectile.NewProjectileDirect(source, position, velocity, type, Item.damage, knockback, player.whoAmI);
			projectile.originalDamage = Item.damage;
			return false;
		}
	}
}
namespace Origins.Buffs {
	public class Fresh_Meat_Buff : ModBuff {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			ID = Type;
		}

		public override void Update(Player player, ref int buffIndex) {
			if (player.ownedProjectileCounts[Fresh_Meat_Artifact_P.ID] > 0) {
				player.buffTime[buffIndex] = 18000;
			} else {
				player.DelBuff(buffIndex);
				buffIndex--;
			}
		}
	}
}

namespace Origins.Items.Weapons.Summoner.Minions {
	public class Fresh_Meat_Artifact_P : ModProjectile, IArtifactMinion {
		public static int ID { get; private set; }
		public override string Texture => typeof(Fresh_Meat_Artifact).GetDefaultTMLName();
		Vector2 stickPos = default;
		float stickRot = 0;
		public int MaxLife { get; set; }
		public int Life { get; set; }
		public override void SetStaticDefaults() {
			// Sets the amount of frames this minion has on its spritesheet
			// This is necessary for right-click targeting
			ProjectileID.Sets.MinionTargettingFeature[Type] = true;

			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[Type] = true;
			// This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned
			ProjectileID.Sets.MinionSacrificable[Type] = true;
			Origins.ForceFelnumShockOnShoot[Type] = true;
			ID = Type;
		}
		public sealed override void SetDefaults() {
			MaxLife = 60 * 30;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.aiStyle = 1;
			Projectile.width = 22;
			Projectile.height = 22;
			Projectile.tileCollide = true;
			Projectile.friendly = false;
			Projectile.minion = true;
			Projectile.minionSlots = 1f;
			Projectile.penetrate = -1;
			Projectile.ignoreWater = false;
		}
		public int StickEnemy {
			get => (int)Projectile.ai[2] - 1;
			set {
				SetupDog();
				Projectile.ai[2] = value + 1;
			}
		}

		public override void AI() {
			Player player = Main.player[Projectile.owner];

			#region Active check
			// This is the "active check", makes sure the minion is alive while the player is alive, and despawns if not
			if (player.dead || !player.active) {
				player.ClearBuff(Fresh_Meat_Buff.ID);
			}
			if (player.HasBuff(Fresh_Meat_Buff.ID)) {
				Projectile.timeLeft = 2;
			}
			#endregion
			if (StickEnemy == -1) {
				Rectangle hitbox = Projectile.Hitbox;
				foreach (NPC npc in Main.ActiveNPCs) {
					if (!npc.friendly && hitbox.Intersects(npc.Hitbox)) {
						StickEnemy = npc.whoAmI;
						stickPos = (Projectile.Center - npc.Center).RotatedBy(-npc.rotation);
						stickRot = Projectile.rotation - npc.rotation;
						Projectile.netUpdate = true;
						break;
					}
				}
			} else {
				if (StickEnemy == -2) {
					Projectile.Center = stickPos;
					Projectile.rotation = stickRot;
					Projectile.velocity.X = 0;
					Projectile.velocity.Y = 0;
				} else {
					NPC npc = Main.npc[StickEnemy];
					if (npc.active) {
						Projectile.Center = npc.Center + stickPos.RotatedBy(npc.rotation);
						Projectile.rotation = stickRot + npc.rotation;
						Projectile.velocity.X = 0;
						Projectile.velocity.Y = 0;
					} else {
						StickEnemy = -1;
					}
				}
			}
			if (Main.rand.NextBool(3)) {
				Dust dust = Dust.NewDustDirect(
					Projectile.position,
					Projectile.width,
					Projectile.height,
					DustID.Blood,
					Projectile.velocity.X * 4,
					Projectile.velocity.Y * 4
				);
				dust.velocity *= 0.25f;
				dust.shader = GameShaders.Armor.GetSecondaryShader(player.cMinion, player);
			}
			if (--Life <= 0) Projectile.Kill();
		}
		void SetupDog() {
			if (Projectile.ai[1] == 0) {
				Projectile dog = Projectile.NewProjectileDirect(
					Projectile.GetSource_FromThis(),
					Projectile.Center,
					Vector2.Zero,
					Fresh_Meat_Dog.ID,
					Projectile.originalDamage,
					Projectile.knockBack,
					Projectile.owner,
					ai2: Projectile.whoAmI
				);
				dog.originalDamage = Projectile.originalDamage;
				Projectile.ai[1] = dog.whoAmI + 1;
			}
		}
		public override bool? CanHitNPC(NPC target) {
			if (Projectile.timeLeft == 0 && !Projectile.IsNPCIndexImmuneToProjectileType(Type, target.whoAmI)) return false;
			return null;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Projectile.perIDStaticNPCImmunity[Type][target.whoAmI] = Main.GameUpdateCount + 1;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			StickEnemy = -2;
			stickPos = Projectile.Center;
			stickRot = Projectile.rotation;
			return false;
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(stickPos.X);
			writer.Write(stickPos.Y);
			writer.Write(stickRot);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			stickPos.X = reader.ReadSingle();
			stickPos.Y = reader.ReadSingle();
			stickRot = reader.ReadSingle();
		}
		public override void OnKill(int timeLeft) {
			Player player = Main.player[Projectile.owner];
			ArmorShaderData shader = GameShaders.Armor.GetSecondaryShader(player.cMinion, player);
			SoundEngine.PlaySound(SoundID.NPCDeath1, Projectile.Center);
			for (int i = 0; i < 10; i++) {
				Dust.NewDustDirect(
					Projectile.position,
					Projectile.width,
					Projectile.height,
					DustID.Blood
				).shader = shader;
			}
		}
		public void OnHurt(int damage) {
			Player player = Main.player[Projectile.owner];
			ArmorShaderData shader = GameShaders.Armor.GetSecondaryShader(player.cMinion, player);
			if (Life > 0) SoundEngine.PlaySound(SoundID.NPCHit1, Projectile.Center);
			for (int i = 0; i < 5; i++) {
				Dust.NewDustDirect(
					Projectile.position,
					Projectile.width,
					Projectile.height,
					DustID.Blood
				).shader = shader;
			}
		}
	}
	public class Fresh_Meat_Dog : ModProjectile {
		public bool OnGround {
			get {
				return Projectile.localAI[1] > 0;
			}
			set {
				Projectile.localAI[1] = value ? 2 : 0;
			}
		}
		public sbyte CollidingX {
			get {
				return (sbyte)Projectile.localAI[0];
			}
			set {
				Projectile.localAI[0] = value;
			}
		}
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			// Sets the amount of frames this minion has on its spritesheet
			//Main.projFrames[Type] = 11;
			// This is necessary for right-click targeting

			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[Type] = true;
			ID = Type;
		}

		public sealed override void SetDefaults() {
			Projectile.DamageType = DamageClass.Summon;
			Projectile.width = 34;
			Projectile.height = 22;
			Projectile.tileCollide = true;
			Projectile.friendly = false;
			Projectile.minion = true;
			Projectile.minionSlots = 0f;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
			Projectile.ignoreWater = false;
		}
		public override bool? CanCutTiles() => false;
		public override bool MinionContactDamage() => true;

		public override void AI() {
			Player player = Main.player[Projectile.owner];
			Projectile.timeLeft = 2;

			#region Active check
			if (Projectile.ai[2] < 0) {
				Projectile.Kill();
				return;
			}
			Projectile meat = Main.projectile[(int)Projectile.ai[2]];
			if (!meat.active || meat.type != Fresh_Meat_Artifact_P.ID || meat.ai[1] != Projectile.whoAmI + 1) {
				Projectile.Kill();
				return;
			}
			#endregion

			#region General behavior
			Vector2 idlePosition = meat.Center;

			// Teleport to player if distance is too big
			Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
			float distanceToIdlePosition = vectorToIdlePosition.Length();

			// If your minion is flying, you want to do this independently of any conditions
			float overlapVelocity = 0.04f;
			for (int i = 0; i < Main.maxProjectiles; i++) {
				// Fix overlap with other minions
				Projectile other = Main.projectile[i];
				if (i != Projectile.whoAmI && other.active && other.owner == Projectile.owner && other.type == Projectile.type && Math.Abs(Projectile.position.X - other.position.X) + Math.Abs(Projectile.position.Y - other.position.Y) < Projectile.width) {
					if (Projectile.position.X < other.position.X) Projectile.velocity.X -= overlapVelocity;
					else Projectile.velocity.X += overlapVelocity;

					if (Projectile.position.Y < other.position.Y) Projectile.velocity.Y -= overlapVelocity;
					else Projectile.velocity.Y += overlapVelocity;
				}
			}
			#endregion

			#region Find target
			// Starting search distance
			float distanceFromTarget = 2000f;
			Vector2 targetCenter = Projectile.position;
			int target = -1;
			void targetingAlgorithm(NPC npc, float targetPriorityMultiplier, bool isPriorityTarget, ref bool foundTarget) {
				if (!isPriorityTarget && distanceFromTarget > 700f) {
					distanceFromTarget = 700f;
				}
				if (npc.CanBeChasedBy()) {
					float between = Vector2.Distance(npc.Center, meat.Center);
					if (isPriorityTarget) between *= 0.5f;
					bool inRange = between < distanceFromTarget;
					bool lineOfSight = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height);
					// Additional check for this specific minion behavior, otherwise it will stop attacking once it dashed through an enemy while flying though tiles afterwards
					// The number depends on various parameters seen in the movement code below. Test different ones out until it works alright
					bool closeThroughWall = between < 100f;
					if ((inRange || !foundTarget) && (lineOfSight || closeThroughWall)) {
						distanceFromTarget = between;
						targetCenter = npc.Center;
						target = npc.whoAmI;
						foundTarget = true;
					}
				}
			}
			bool foundTarget = player.GetModPlayer<OriginPlayer>().GetMinionTarget(targetingAlgorithm, true);

			//projectile.friendly = foundTarget;
			#endregion

			#region Movement

			// Default movement parameters (here for attacking)
			float speed = 8f;
			float inertia = 6f;
			Rectangle targetHitbox;

			if (foundTarget) {
				targetHitbox = Main.npc[target].Hitbox;
			} else {
				targetHitbox = meat.Hitbox;
				targetCenter = idlePosition;
				distanceFromTarget = distanceToIdlePosition;
			}

			// Minion has a target: attack (here, fly towards the enemy)
			Vector2 direction = targetCenter - Projectile.Center;
			int directionX = Math.Sign(direction.X);
			Projectile.spriteDirection = directionX;
			bool wallColliding = CollidingX != 0 && CollidingX == directionX;
			float YRatio = direction.Y / ((direction.X * -directionX) + 0.1f);
			if (direction.Y < 160 && (wallColliding || YRatio > 1) && OnGround) {
				float jumpStrength = 6;
				if (wallColliding) {
					if (Collision.TileCollision(Projectile.position - new Vector2(18), Vector2.UnitX, Projectile.width, Projectile.height, false, false).X == 0) {
						jumpStrength++;
						if (Collision.TileCollision(Projectile.position - new Vector2(36), Vector2.UnitX, Projectile.width, Projectile.height, false, false).X == 0) {
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
				Projectile.velocity.Y = -jumpStrength;
			}
			if (distanceFromTarget > 32f) {
				direction.Normalize();
				Projectile.velocity.X = (Projectile.velocity.X * (inertia - 1) + direction.X * speed) / inertia;
			} else {
				Projectile.velocity.X = (Projectile.velocity.X * (inertia - 1)) / inertia;
			}
			Projectile.friendly = false;
			if (distanceFromTarget < 32) {
				if (++Projectile.localAI[2] >= 25 && Projectile.Hitbox.Intersects(targetHitbox)) {
					if (foundTarget) {
						Projectile.friendly = true;
					} else {
						meat.DamageArtifactMinion(Projectile.damage);
					}
					Projectile.localAI[2] = 0;
				}
			}

			#endregion

			//gravity
			Projectile.velocity.Y += 0.4f;

			#region Animation and visuals
			if (Projectile.ai[0] > 0) {
				Projectile.frame = 7 + (int)(Projectile.ai[0] / 6f);
				if (++Projectile.ai[0] > 30) {
					//Projectile.NewProjectile(projectile.Center, Vector2.Zero, ProjectileID.SolarWhipSwordExplosion, projectile.damage, 0, projectile.owner, 1, 1);
					Projectile.Kill();
				}
			} else if (OnGround) {
				Projectile.localAI[1]--;
				const int frameSpeed = 4;
				if (Math.Abs(Projectile.velocity.X) < 0.01f) {
					Projectile.velocity.X = 0f;
				}
				if ((Projectile.velocity.X != 0) ^ (Projectile.oldVelocity.X != 0)) {
					Projectile.frameCounter = 0;
				}
				if (Projectile.velocity.X != 0) {
					Projectile.frameCounter++;
					if (Projectile.frameCounter >= frameSpeed) {
						Projectile.frameCounter = 0;
						Projectile.frame++;
						if (Projectile.frame >= 6) {
							Projectile.frame = 0;
						}
					}
				} else {
					Projectile.frameCounter++;
					if (Projectile.frameCounter >= frameSpeed) {
						Projectile.frameCounter = 0;
						Projectile.frame = 0;
					}
				}
			} else if (Projectile.frame > 6) {
				Projectile.frame = 1;
			}

			// Some visuals here
			if (Projectile.frame < 7) {
				Lighting.AddLight(Projectile.Center, Color.Green.ToVector3() * 0.18f);
			} else if (Projectile.frame < 9) {
				Lighting.AddLight(Projectile.Center, Color.Red.ToVector3() * 0.24f);
			}
			#endregion
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (oldVelocity.Y > Projectile.velocity.Y) {
				OnGround = true;
			} else {
				if (Collision.SlopeCollision(Projectile.position, new Vector2(0, 4), Projectile.width, Projectile.height).Y != 4) {
					OnGround = true;
				}
			}
			if (oldVelocity.X > Projectile.velocity.X) {
				CollidingX = (sbyte)(1 - Collision.TileCollision(Projectile.position, Vector2.UnitX, Projectile.width, Projectile.height, false, false).X);
			} else if (oldVelocity.X < Projectile.velocity.X) {
				CollidingX = (sbyte)(-1 - Collision.TileCollision(Projectile.position, -Vector2.UnitX, Projectile.width, Projectile.height, false, false).X);
			} else {
				CollidingX = 0;
			}
			return true;
		}
		public override bool PreDraw(ref Color lightColor) {
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Main.EntitySpriteDraw(
				texture,
				Projectile.Center - Main.screenPosition,
				new Rectangle(0, Projectile.frame * 52, 56, 50),
				Projectile.GetAlpha(lightColor),
				Projectile.rotation,
				new Vector2(28, 25),
				Projectile.scale,
				Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
				0);
			Main.EntitySpriteDraw(
				Mod.Assets.Request<Texture2D>("Items/Weapons/Summoner/Minions/Happy_Boi_Glow").Value,
				Projectile.Center - Main.screenPosition,
				new Rectangle(0, Projectile.frame * 52, 56, 50),
				Color.White,
				Projectile.rotation,
				new Vector2(28, 25),
				Projectile.scale,
				Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
				0);
			return false;
		}
	}
}
