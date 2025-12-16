using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Items.Weapons.Summoner;
using Origins.Items.Weapons.Summoner.Minions;
using Origins.Projectiles;
using PegasusLib;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Summoner {
	public class Ocean_Amulet : ModItem {
		public override void SetStaticDefaults() {
			ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // This lets the player target anywhere on the whole screen while using a controller
			ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
		}
		public override void SetDefaults() {
			Item.damage = 12;
			Item.knockBack = 1f;
			Item.DamageType = DamageClass.Summon;
			Item.mana = 14;
			Item.shootSpeed = 9f;
			Item.width = 24;
			Item.height = 38;
			Item.useTime = 24;
			Item.useAnimation = 24;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noUseGraphic = true;
			Item.value = Item.sellPrice(silver: 1, copper: 50);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item44;
			Item.buffType = Ocean_Buff.ID;
			Item.shoot = School_Fish.ID;
			Item.noMelee = true;
		}
		public override void AddRecipes() {
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			player.AddBuff(Item.buffType, 2);
			for (int i = 0; i < 8; i++) {
				Main.projectile[player.SpawnMinionOnCursor(source, player.whoAmI, type, Item.damage, knockback)].ai[2] = i % 5;
			}
			return false;
		}
	}
}
namespace Origins.Buffs {
	public class Ocean_Buff : MinionBuff {
		public static int ID { get; private set; }
		public override IEnumerable<int> ProjectileTypes() => [
			School_Fish.ID
		];
		public override bool IsArtifact => true;
	}
}

namespace Origins.Items.Weapons.Summoner.Minions {
	public class School_Fish : ModProjectile, IArtifactMinion {
		public int MaxLife { get; set; }
		public float Life { get; set; }
		public static int ID { get; private set; }
		public bool DrawHealthBar(Vector2 position, float light, bool inBuffList) => false;
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 3;
			// Sets the amount of frames this minion has on its spritesheet
			// This is necessary for right-click targeting
			ProjectileID.Sets.MinionTargettingFeature[Type] = true;

			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[Type] = true;
			// This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned
			ProjectileID.Sets.MinionSacrificable[Type] = true;
			ID = Type;
		}

		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Summon;
			Projectile.width = 14;
			Projectile.height = 14;
			Projectile.tileCollide = true;
			Projectile.friendly = true;
			Projectile.minion = true;
			Projectile.minionSlots = 1f / 8;
			Projectile.penetrate = -1;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 6;
			Projectile.ignoreWater = true;
			Projectile.manualDirectionChange = true;
			Projectile.netImportant = true;
			MaxLife = 30;
		}
		public override bool? CanCutTiles() => false;
		public override bool MinionContactDamage() => true;
		Vector2 RestingPoint {
			get {
				Player player = Main.player[Projectile.owner];
				return player.MountedCenter - new Vector2(player.direction * 32, Projectile.height * 0.5f);
			}
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];

			#region Active check
			// This is the "active check", makes sure the minion is alive while the player is alive, and despawns if not
			if (player.dead || !player.active) {
				player.ClearBuff(Ocean_Buff.ID);
			} else if (player.HasBuff(Ocean_Buff.ID)) {
				Projectile.timeLeft = 2;
			}
			#endregion
			if (Projectile.IsLocallyOwned() && Projectile.DistanceSQ(RestingPoint) > 2000 * 2000) {
				Projectile.Center = RestingPoint;
				Projectile.velocity = Main.rand.NextVector2Circular(1, 1);
				Projectile.netUpdate = true;
			}
			bool wet = Collision.WetCollision(Projectile.position, Projectile.width, Projectile.height);
			Projectile.ai[1] = (Projectile.ai[2].CycleUp(5) ? wet || (Main.raining && CollisionExtensions.CanRainReach(Projectile.Center)) : Projectile.ai[1] != 0).ToInt();
			if (Projectile.ai[1] == 0) {
				float rot = (Projectile.velocity * Projectile.spriteDirection).ToRotation();
				Projectile.velocity.Y += 0.4f;
				player.GetModPlayer<OriginPlayer>().GetMinionTarget(TargetingAlgorithm);
				Projectile.rotation += Projectile.localAI[0] + ((Projectile.velocity * Projectile.spriteDirection).ToRotation() - rot);
			} else {
				DoBoiding();
				if (wet) {
					const float jump_mult = 1.5f;
					if (!Collision.WetCollision(Projectile.position + Projectile.velocity * Vector2.UnitX, Projectile.width, Projectile.height)) Projectile.velocity.X *= jump_mult;
					if (!Collision.WetCollision(Projectile.position + Projectile.velocity * Vector2.UnitY, Projectile.width, Projectile.height)) Projectile.velocity.Y *= jump_mult;
				}
				if (Projectile.velocity != default) {
					OriginExtensions.AngularSmoothing(ref Projectile.rotation, (Projectile.velocity * Projectile.spriteDirection).ToRotation(), 0.5f);
				}
				Projectile.localAI[0] = 0;
			}
			Projectile.ai[0] = targetID;
			Projectile.frameCounter++;
			if (Projectile.frameCounter.CycleUp(5)) Projectile.frame.CycleUp(Main.projFrames[Type]);
			Projectile.spriteDirection = Projectile.direction = Math.Sign(Projectile.velocity.X);
			return;
			#region General behavior
			Vector2 idlePosition = player.Bottom - new Vector2(player.direction * (Projectile.minionPos + 1) * 32, Projectile.height * 0.5f);

			// die if distance is too big
			Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
			float distanceToIdlePosition = vectorToIdlePosition.Length();
			if (Main.myPlayer == player.whoAmI) {
				if (distanceToIdlePosition > 400) {
					if (distanceToIdlePosition > 2000) {
						Projectile.position = idlePosition;
						Projectile.velocity *= 0.1f;
						Projectile.netUpdate = true;
					} else if (Projectile.localAI[1] <= 0) {
						Projectile.ai[2] = 1;
						Projectile.netUpdate = true;
					}
				}
			}
			if (Projectile.ai[2] == 1) {
				Projectile.localAI[1] = 300;
				float speed = 8f;
				float inertia = 12f;
				Vector2 direction = vectorToIdlePosition * (speed / distanceToIdlePosition);
				Projectile.velocity = (Projectile.velocity * (inertia - 1) + direction) / inertia;
				Projectile.tileCollide = false;
				Projectile.direction = Math.Sign(vectorToIdlePosition.X);
				if (++Projectile.frameCounter >= 20) Projectile.frameCounter = 0;
				Projectile.frame = 9;
				if (distanceToIdlePosition > 64 || Projectile.Hitbox.OverlapsAnyTiles()) return;
				Rectangle floorbox = Projectile.Hitbox;
				floorbox.Offset(0, Projectile.height);
				floorbox.Height = 16 * 4;
				if (!floorbox.OverlapsAnyTiles(false)) return;
				Projectile.ai[2] = 0;
				Projectile.netUpdate = true;
			}
			Projectile.localAI[1]--;
			Projectile.tileCollide = true;
			foreach (Projectile other in Main.ActiveProjectiles) {
				if (other.type == Type && other.owner == Projectile.owner && other.Hitbox.Intersects(Projectile.Hitbox)) {
					Projectile.velocity.X += Math.Sign(Projectile.position.X - other.position.X) * 0.03f;
				}
			}
			#endregion

			#region Find target
			// Starting search distance
			bool foundTarget = player.GetModPlayer<OriginPlayer>().GetMinionTarget(TargetingAlgorithm);
			#endregion

			#region Movement
			if (Projectile.velocity.X != 0) Projectile.direction = Math.Sign(Projectile.velocity.X);
			if (foundTarget) {
				Projectile.ai[0] = targetID;
				Projectile.velocity += (targetCenter - Projectile.Center).Normalized(out _);
				Projectile.direction = Math.Sign(Projectile.velocity.X);
				Projectile.velocity *= 0.98f;
			} else {
				if (distanceToIdlePosition > 100) {
					Projectile.velocity += (vectorToIdlePosition - Projectile.Center).Normalized(out _);
				}
				Projectile.direction = Math.Sign(Projectile.velocity.X);
				Projectile.velocity *= 0.96f;
			}
			if (Projectile.velocity != default) OriginExtensions.AngularSmoothing(ref Projectile.rotation, Projectile.velocity.ToRotation(), 0.05f);
			#endregion

			#region Animation and visuals
			if (Math.Abs(Projectile.velocity.X) <= 0.1f) Projectile.velocity.X = 0;
			// This is a simple "loop through all frames from top to bottom" animation
			if (Projectile.velocity.X == 0 || foundTarget) {
				if (++Projectile.frameCounter >= 5) {
					Projectile.frameCounter = 0;
					if (++Projectile.frame >= 8) Projectile.frame = 0;
				}
			} else if (++Projectile.frameCounter >= 5) {
				Projectile.frameCounter = 0;
				if (++Projectile.frame >= Main.projFrames[Projectile.type]) Projectile.frame = 8;
			}
			#endregion

			if (Projectile.localAI[2] <= 0) {
				if (this.GetHurtByHostiles()) {
					// add sound
					Projectile.localAI[2] = 20;
				}
			} else {
				Projectile.localAI[2]--;
			}
			if (Projectile.lavaWet) {
				Projectile.localAI[2] = -1;
				this.DamageArtifactMinion(200);
			}
			if (Projectile.velocity.Y != 0) {
				Projectile.frameCounter = 0;
				Projectile.frame = 9;
			}
			Projectile.velocity.Y += 0.4f;
		}
		Vector2 targetCenter;
		float distanceFromTarget;
		int targetID;
		public OriginPlayer.Minion_Selector TargetingAlgorithm {
			get {
				Player player = Main.player[Projectile.owner];
				targetCenter = RestingPoint;
				distanceFromTarget = 2000f;
				targetID = -1;
				bool hasPriorityTarget = false;
				void targetingAlgorithm(NPC npc, float targetPriorityMultiplier, bool isPriorityTarget, ref bool foundTarget) {
					bool isCurrentTarget = npc.whoAmI == Projectile.ai[0];
					if ((isCurrentTarget || isPriorityTarget || !hasPriorityTarget) && npc.CanBeChasedBy()) {
						Vector2 pos = Projectile.position;
						int dir = Math.Sign(npc.Center.X - pos.X);
						float between = Vector2.Distance(npc.Center, Projectile.Center);
						between *= isCurrentTarget ? 0 : 1;
						if (distanceFromTarget > between) {
							distanceFromTarget = between;
							targetCenter = npc.Center;
							targetID = npc.whoAmI;
							foundTarget = true;
							hasPriorityTarget = isPriorityTarget;
						}
					}
				}
				return targetingAlgorithm;
			}
		}
		public void DoBoiding() {
			const float grouping_factor = 0.003f;
			const float spreading_factor = 0.1f;
			const float sheeping_factor = 0.01f;
			const float control_weight = 30f;
			const float anger_weight = 8f;
			const float helping_weight = 8f;

			Vector2 swarmCenter = default;
			Vector2 magnetism = default;
			Vector2 swarmVelocity = default;
			float totalWeight = 0;

			Player player = Main.player[Projectile.owner];
			Vector2 enemyDir = default;
			if (player.GetModPlayer<OriginPlayer>().GetMinionTarget(TargetingAlgorithm)) {
				swarmCenter += targetCenter * helping_weight;
				enemyDir = Projectile.DirectionTo(targetCenter);
				swarmVelocity += enemyDir * 4 * helping_weight;
				magnetism = enemyDir * 0.15f;
				totalWeight += helping_weight;
			} else {
				float controlWeight = control_weight * (1 + float.Min(Projectile.Distance(targetCenter) / (16 * 20), 1));
				swarmCenter = targetCenter * controlWeight;
				Vector2 dir = Projectile.DirectionTo(targetCenter);
				swarmVelocity = dir * 4 * control_weight;
				magnetism = dir * 0.15f;
				totalWeight += controlWeight;
			}
			const float i_must_bee_traveling_on_now = 8;
			foreach (Projectile other in Main.ActiveProjectiles) {
				if (other.type != Type || other.ai[0] != Projectile.ai[0]) continue;
				float distSQ = other.DistanceSQ(Projectile.Center);

				swarmCenter += other.Center;
				swarmVelocity += other.velocity;
				totalWeight += 1;
				if (distSQ <= i_must_bee_traveling_on_now * i_must_bee_traveling_on_now) magnetism += (Projectile.Center - other.Center) * spreading_factor;
				//float blockDist = CollisionExtensions.
			}

			if (totalWeight > 0) {
				float speed = 4;
				if (enemyDir != default) speed += 2 + float.Min(Projectile.Distance(targetCenter) / (16 * 20), 4);
				Projectile.velocity =
					(Projectile.velocity +
					((swarmCenter / totalWeight) - Projectile.Center) * grouping_factor +
					magnetism +
					(swarmVelocity / totalWeight) * sheeping_factor)
					.WithMaxLength(speed)
				;
			}
			if (enemyDir != default) {
				Vector2 norm = Projectile.velocity.SafeNormalize(default);
				float dot = Vector2.Dot(enemyDir, norm);
				if (dot < 0) dot *= -0.9f;
				Projectile.velocity -= Projectile.velocity * (1 - MathF.Pow(dot, 3)) * 0.05f;
			}
			if (Projectile.velocity.HasNaNs()) Projectile.velocity = default;
		}
		public bool CanWalkOnto(Vector2 position, int dir = 0) {
			List<Point> tiles = Collision.GetTilesIn(position + new Vector2(0, Projectile.height), position + Projectile.Size);
			bool[] solid = new bool[3];
			for (int i = 0; i < tiles.Count; i++) {
				solid[i] = Framing.GetTileSafely(tiles[i]).HasSolidTile();
			}

			if (tiles.Count == 3) {
				solid[1 - dir] = true;
				solid[1] = true;
				Tile centerTile = Framing.GetTileSafely(tiles[1]);
				if (centerTile.HasSolidTile()) {
					if (centerTile.IsHalfBlock || centerTile.Slope == SlopeType.SlopeDownLeft) return true;

					if (centerTile.IsHalfBlock || centerTile.Slope == SlopeType.SlopeDownRight) return true;
				}
				centerTile = Framing.GetTileSafely(tiles[0]);
				if (centerTile.HasSolidTile() && (centerTile.IsHalfBlock || centerTile.Slope == SlopeType.SlopeDownLeft)) return true;

				centerTile = Framing.GetTileSafely(tiles[2]);
				if (centerTile.HasSolidTile() && (centerTile.IsHalfBlock || centerTile.Slope == SlopeType.SlopeDownRight)) return true;
			}
			for (int i = 0; i < tiles.Count; i++) {
				if (!solid[i]) return false;
			}
			return true;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (Projectile.ai[1] == 0 && Projectile.velocity.Y != oldVelocity.Y) {
				Projectile.velocity.X = Main.rand.NextFloat(2, 3) * Math.Sign(targetCenter.X - Projectile.Center.X);
				Projectile.velocity.Y = -Main.rand.NextFloat(2, 4);
				Projectile.localAI[0] = Main.rand.NextBool().ToDirectionInt() * Main.rand.Next(1, 4) * 0.03f;
				if (Main.rand.NextBool(4)) {
					Projectile.velocity *= 1.5f;
					Projectile.netUpdate = true;
				}
			}
			return false;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Projectile.DamageArtifactMinion(MaxLife, noCombatText: true);
		}
		public override void OnKill(int timeLeft) {
			SoundEngine.PlaySound(SoundID.NPCDeath1, Projectile.Center);
		}
		public void OnHurt(int damage, bool fromDoT) {
			if (fromDoT) return;
			if (Life > 0) SoundEngine.PlaySound(SoundID.NPCHit1.WithPitch(1f).WithVolume(0.25f), Projectile.Center);
		}
	}
}
