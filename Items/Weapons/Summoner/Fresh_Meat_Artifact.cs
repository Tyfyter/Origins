using Microsoft.Xna.Framework;
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
using Origins.Gores;
using System.Collections.Generic;
using PegasusLib;

namespace Origins.Items.Weapons.Summoner {
	public class Fresh_Meat_Artifact : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Artifact",
			"Minion"
		];
		public override void SetStaticDefaults() {
			ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // This lets the player target anywhere on the whole screen while using a controller
			ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
		}
		public override void SetDefaults() {
			Item.damage = 25;
			Item.DamageType = DamageClass.Summon;
			Item.mana = 17;
			Item.shootSpeed = 9f;
			Item.knockBack = 1f;
			Item.width = 24;
			Item.height = 38;
			Item.useTime = 24;
			Item.useAnimation = 24;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noUseGraphic = true;
			Item.value = Item.sellPrice(gold: 1, silver: 50);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1;
			Item.buffType = Fresh_Meat_Buff.ID;
			Item.shoot = Fresh_Meat_Artifact_P.ID;
			Item.noMelee = true;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			player.AddBuff(Item.buffType, 2);
			damage = (int)Math.Ceiling(player.GetTotalDamage(Item.DamageType).CombineWith(player.OriginPlayer().artifactDamage).GetInverse().ApplyTo(damage));
			Projectile projectile = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI);
			projectile.originalDamage = damage;
			return false;
		}
	}
}
namespace Origins.Buffs {
	public class Fresh_Meat_Buff : MinionBuff {
		public static int ID { get; private set; }
		public override IEnumerable<int> ProjectileTypes() => [
			Fresh_Meat_Artifact_P.ID
		];
		public override bool IsArtifact => true;
	}
}

namespace Origins.Items.Weapons.Summoner.Minions {
	public class Fresh_Meat_Artifact_P : ModProjectile, IArtifactMinion {
		public static int ID { get; private set; }
		public override string Texture => typeof(Fresh_Meat_Artifact).GetDefaultTMLName();
		Vector2 stickPos = default;
		float stickRot = 0;
		public int MaxLife { get; set; }
		public float Life { get; set; }
		public override void SetStaticDefaults() {
			// Sets the amount of frames this minion has on its spritesheet
			// This is necessary for right-click targeting
			ProjectileID.Sets.MinionTargettingFeature[Type] = true;

			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[Type] = true;
			// This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned
			ProjectileID.Sets.MinionSacrificable[Type] = true;
			ProjectileID.Sets.NeedsUUID[Type] = true;
			Origins.ForceFelnumShockOnShoot[Type] = true;
			ID = Type;
		}
		public override void SetDefaults() {
			MaxLife = 60 * 15;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.aiStyle = ProjAIStyleID.ThrownProjectile;
			Projectile.width = 22;
			Projectile.height = 22;
			Projectile.tileCollide = true;
			Projectile.friendly = false;
			Projectile.minion = true;
			Projectile.minionSlots = 1f;
			Projectile.penetrate = -1;
			Projectile.ignoreWater = false;
			Projectile.netImportant = true;
		}
		public int StickEnemy {
			get => (int)Projectile.ai[2] - 1;
			set {
				Projectile.ai[2] = value + 1;
			}
		}
		public override void OnSpawn(IEntitySource source) {
			Projectile.ai[1] = -1;
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			SetupDog();
			if (StickEnemy != -2 && !Main.npc.IndexInRange(StickEnemy)) StickEnemy = -1;

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
						stickPos = (Projectile.Center + Projectile.velocity - npc.Center).RotatedBy(-npc.rotation);
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
				Life--;
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
		}
		void SetupDog() {
			if (Projectile.ai[1] == -1) {
				Projectile dog = Projectile.NewProjectileDirect(
					Projectile.GetSource_FromThis(),
					Projectile.Center,
					Vector2.Zero,
					Fresh_Meat_Dog.ID,
					Projectile.originalDamage,
					Projectile.knockBack,
					Projectile.owner,
					ai2: Projectile.identity
				);
				dog.originalDamage = Projectile.originalDamage;
				Projectile.ai[1] = dog.identity;
				Projectile.netUpdate = true;
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
			stickPos = Projectile.Center + Projectile.velocity;
			stickRot = Projectile.rotation;
			Projectile.netUpdate = true;
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
		public void OnHurt(int damage, bool fromDoT) {
			if (fromDoT) return;
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
			};
		}
	}
	public class Fresh_Meat_Dog : ModProjectile, IShadedProjectile {
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
		public int Shader => Main.player[Projectile.owner].cMinion;
		public override void SetStaticDefaults() {
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Type] = 15;
			// This is necessary for right-click targeting

			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[Type] = true;
			ProjectileID.Sets.NeedsUUID[Type] = true;
			ProjectileID.Sets.MinionShot[Type] = true;
			ID = Type;
		}

		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Summon;
			Projectile.width = 76;
			Projectile.height = 40;
			Projectile.tileCollide = true;
			Projectile.friendly = false;
			/*Projectile.minion = true;
			Projectile.minionSlots = 0f;*/
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
			Projectile.ignoreWater = false;
			Projectile.netImportant = true;
		}
		public override bool? CanCutTiles() => false;
		public override bool MinionContactDamage() => true;
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			Projectile.timeLeft = 2;

			#region Active check
			Projectile meat = Projectile.GetRelatedProjectile_Depreciated(2);
			if (meat is null || !meat.active || meat.type != Fresh_Meat_Artifact_P.ID || meat.ai[1] != Projectile.identity) {
				if (Projectile.owner == Main.myPlayer) Projectile.Kill();
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
				if (i != Projectile.identity && other.active && other.owner == Projectile.owner && other.type == Projectile.type && Math.Abs(Projectile.position.X - other.position.X) + Math.Abs(Projectile.position.Y - other.position.Y) < Projectile.width) {
					if (Projectile.position.X < other.position.X) Projectile.velocity.X -= overlapVelocity;
					else Projectile.velocity.X += overlapVelocity;

					if (Projectile.position.Y < other.position.Y) Projectile.velocity.Y -= overlapVelocity;
					else Projectile.velocity.Y += overlapVelocity;
				}
			}
			#endregion

			#region Find target
			// Starting search distance
			float distanceFromTarget = 700f;
			Vector2 targetCenter = Projectile.position;
			int target = -1;
			void targetingAlgorithm(NPC npc, float targetPriorityMultiplier, bool isPriorityTarget, ref bool foundTarget) {
				if (!isPriorityTarget && distanceFromTarget > 400f) {
					distanceFromTarget = 400f;
				}
				if (npc.active && npc.chaseable && !npc.immortal && !npc.dontTakeDamage && !npc.friendly && !(player.dontHurtCritters && NPCID.Sets.CountsAsCritter[npc.type])) {
					float between = Vector2.Distance(npc.Center, meat.Center);
					if (isPriorityTarget) between *= 0.5f;
					bool inRange = between < distanceFromTarget;
					if (inRange) {
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
			float inertia = 12f;
			const float gravity = 0.4f;
			Rectangle targetHitbox;

			if (foundTarget) {
				targetHitbox = Main.npc[target].Hitbox;
			} else {
				targetHitbox = meat.Hitbox;
				targetCenter = idlePosition;
				distanceFromTarget = distanceToIdlePosition;
			}

			// Minion has a target: attack (here, fly towards the enemy)
			Vector2 direction = targetHitbox.Bottom() - Projectile.Top;
			int directionX = Math.Sign(direction.X);
			Projectile.spriteDirection = directionX;
			bool wallColliding = CollidingX != 0 && CollidingX == directionX;
			float YRatio = (-direction.Y) / (Math.Abs(direction.X) + 0.1f);
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
			distanceFromTarget = targetCenter.Clamp(Projectile.Hitbox).Distance(Projectile.Center.Clamp(targetHitbox));
			Projectile.friendly = false;
			bool attac = distanceFromTarget < 16 || ((!OnGround || Projectile.localAI[2] >= 16) && Projectile.localAI[2] <= 14);
			if (!attac) {
				const int prediction = 12;
				Rectangle projHitbox = Projectile.Hitbox;
				Rectangle targHitbox = targetHitbox;
				Vector2 gravFactor = new(0, (prediction * (prediction + 1)) * 0.5f);
				projHitbox.Offset((Projectile.velocity * prediction + gravity * gravFactor).ToPoint());
				targHitbox.Offset(foundTarget ? (Main.npc[target].velocity * prediction + Main.npc[target].gravity * gravFactor).ToPoint() : default);
				if (projHitbox.Intersects(targetHitbox)) {
					attac = true;
				}
			}
			if (attac) {
				if (++Projectile.localAI[2] >= 14) {
					if (Projectile.localAI[2] == 16) {
						if (foundTarget) {
							Projectile.friendly = true;
						} else {
							meat.DamageArtifactMinion(Projectile.damage);
						}
					}
					if (Projectile.localAI[2] >= 18) Projectile.localAI[2] = 0;
				}
				if (OnGround) {
					speed = Projectile.localAI[2] >= 12 ? 16 : 4;
				}
			} else if (OnGround || Projectile.localAI[2] < 10) {
				Projectile.localAI[2] = 0;
			}
			if (distanceFromTarget > 0f) {
				Projectile.velocity.X = (Projectile.velocity.X * (inertia - 1) + MathHelper.Clamp(direction.X / 8, -1, 1) * speed) / inertia;
			} else {
				Projectile.velocity.X = (Projectile.velocity.X * (inertia - 1)) / inertia;
			}

			#endregion

			//gravity
			Projectile.velocity.Y += gravity;

			#region Animation and visuals
			if (OnGround) {
				Projectile.localAI[1]--;
				const int frameSpeed = 4;
				if (Math.Abs(Projectile.velocity.X) < 0.01f) {
					Projectile.velocity.X = 0f;
				}
				if ((Projectile.velocity.X != 0) != (Projectile.oldVelocity.X != 0)) {
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
						Projectile.frame = 6;
					}
				}
			} else if (Projectile.frame > 6) {
				Projectile.frame = 1;
			}
			if (Projectile.localAI[2] > 0) {
				Projectile.frame = (int)(Projectile.localAI[2] / 2f) + 6;
			}
			#endregion
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (oldVelocity.Y > Projectile.velocity.Y) {
				OnGround = true;
			} else {
				Rectangle hitbox = Projectile.Hitbox;
				hitbox.Offset(0, 1);
				if (hitbox.OverlapsAnyTiles()) {
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
			Rectangle frame = texture.Frame(verticalFrames: Main.projFrames[Type], frameY: Projectile.frame);
			Main.EntitySpriteDraw(
				texture,
				Projectile.Bottom - Main.screenPosition,
				frame,
				Projectile.GetAlpha(lightColor),
				Projectile.rotation,
				new Vector2(frame.Width / 2, frame.Height - 4),
				Projectile.scale,
				Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
				0);
			return false;
		}
		public override void OnKill(int timeLeft) {
			const float spread = 2;
			SoundEngine.PlaySound(SoundID.NPCDeath1, Projectile.Center);
			Player owner = Main.player[Projectile.owner];
			ArmorShaderData shaderData = GameShaders.Armor.GetSecondaryShader(owner.cMinion, owner);
			Dust.NewDustPerfect(
				Projectile.Center + new Vector2(22 * Projectile.direction, -4),
				ModContent.DustType<Meat_Dog_Gore1>(),
				Projectile.velocity + Main.rand.NextVector2Circular(spread, spread)
			).shader = shaderData;
			Dust.NewDustPerfect(
				Projectile.Center + new Vector2(-10 * Projectile.direction, 11),
				ModContent.DustType<Meat_Dog_Gore2>(),
				Projectile.velocity + Main.rand.NextVector2Circular(spread, spread)
			).shader = shaderData;
			Dust.NewDustPerfect(
				Projectile.Center + new Vector2(-18 * Projectile.direction, 19),
				ModContent.DustType<Meat_Dog_Gore3>(),
				Projectile.velocity + Main.rand.NextVector2Circular(spread, spread)
			).shader = shaderData;
			for (int i = Main.rand.Next(16, 25); i >= 0; i--) {
				Dust.NewDustDirect(
					Projectile.position + Vector2.One * 2,
					Projectile.width - 4,
					Projectile.height - 4,
					DustID.Blood,
					Projectile.velocity.X,
					Projectile.velocity.Y
				).shader = shaderData;
			}
		}
	}
}
