using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Weapons.Summoner.Minions;
using Origins.Projectiles;
using System;
using System.Collections;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Summoner {
	public class Terratotem : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Artifact",
			"Minion"
		];
		public override void SetStaticDefaults() {
			ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // This lets the player target anywhere on the whole screen while using a controller
			ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
		}
		public override void SetDefaults() {
			Item.damage = 70;
			Item.DamageType = DamageClass.Summon;
			Item.knockBack = 1f;
			Item.mana = 48;
			Item.shootSpeed = 9f;
			Item.width = 24;
			Item.height = 38;
			Item.useTime = 24;
			Item.useAnimation = 24;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noUseGraphic = true;
			Item.value = Item.sellPrice(gold: 20);
			Item.rare = ItemRarityID.Yellow;
			Item.UseSound = SoundID.Item44;
			Item.buffType = Terratotem_Buff.ID;
			Item.shoot = Terratotem_Tab.ID;
			Item.noMelee = true;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			player.AddBuff(Item.buffType, 2);
			Projectile projectile = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI, player.itemAnimation);
			projectile.originalDamage = Item.damage;
			return false;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Broken_Terratotem>()
			.AddIngredient(ItemID.BabyBirdStaff)
			.AddIngredient(ItemID.ImpStaff)
			.AddIngredient(ItemID.SanguineStaff)
			.AddIngredient(ItemID.Smolstar)
			.AddIngredient(ItemID.OpticStaff)
			.AddIngredient(ItemID.TempestStaff)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
}

namespace Origins.Items.Weapons.Summoner.Minions {
	public class Terratotem_Orb : ModProjectile, IArtifactMinion {
		public static int ID { get; private set; }
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
			Origins.ForceFelnumShockOnShoot[Type] = true;
			ID = Type;
		}

		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Summon;
			Projectile.width = 24;
			Projectile.height = 24;
			Projectile.tileCollide = true;
			Projectile.friendly = true;
			Projectile.minion = true;
			Projectile.minionSlots = 1f;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 20;
			Projectile.ignoreWater = true;
			Projectile.netImportant = true;
			MaxLife = 15 * 45;
		}
		public override bool? CanCutTiles() => false;
		public override bool MinionContactDamage() => true;

		public override void AI() {
			Player player = Main.player[Projectile.owner];

			#region Active check
			// This is the "active check", makes sure the minion is alive while the player is alive, and despawns if not
			if (player.dead || !player.active) {
				player.ClearBuff(Terratotem_Buff.ID);
			} else if (player.HasBuff(Terratotem_Buff.ID)) {
				Projectile.timeLeft = 2;
			}
			#endregion

			#region General behavior
			Vector2 idlePosition = player.Top;
			idlePosition.X -= 48f * player.direction;

			// Teleport to player if distance is too big
			Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
			float distanceToIdlePosition = vectorToIdlePosition.Length();
			if (Main.myPlayer == player.whoAmI && distanceToIdlePosition > 2000f) {
				// Whenever you deal with non-regular events that change the behavior or position drastically, make sure to only run the code on the owner of the projectile,
				// and then set netUpdate to true
				Projectile.position = idlePosition;
				Projectile.velocity *= 0.1f;
				Projectile.netUpdate = true;
			}

			// If your minion is flying, you want to do this independently of any conditions
			float overlapVelocity = 0.04f;
			for (int i = 0; i < Main.maxProjectiles; i++) {
				// Fix overlap with other minions
				Projectile other = Main.projectile[i];
				if (i != Projectile.whoAmI && other.active && other.owner == Projectile.owner && Math.Abs(Projectile.position.X - other.position.X) + Math.Abs(Projectile.position.Y - other.position.Y) < Projectile.width) {
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
			Vector2 targetCenter = default;
			int target = -1;
			void targetingAlgorithm(NPC npc, float targetPriorityMultiplier, bool isPriorityTarget, ref bool foundTarget) {
				if (!isPriorityTarget && distanceFromTarget > 700f) {
					distanceFromTarget = 700f;
				}
				if (npc.CanBeChasedBy()) {
					float between = Vector2.Distance(npc.Center, Projectile.Center);
					bool closest = Vector2.Distance(Projectile.Center, targetCenter) > between;
					bool inRange = between < distanceFromTarget;
					bool lineOfSight = isPriorityTarget || Collision.CanHitLine(Projectile.position + new Vector2(1, 4), Projectile.width - 2, Projectile.height - 8, npc.position, npc.width, npc.height);
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
			bool foundTarget = player.GetModPlayer<OriginPlayer>().GetMinionTarget(targetingAlgorithm);
			#endregion

			if (foundTarget) {
				if (Projectile.ai[1] > 0) {
					ArmorShaderData shaderData = GameShaders.Armor.GetSecondaryShader(player.cMinion, player);
					const float diameter = 16;
					Vector2 offset = Main.rand.NextVector2CircularEdge(diameter, diameter) * Main.rand.NextFloat(0.9f, 1f);
					Dust.NewDustPerfect(
						Projectile.Center - offset,
						DustID.TerraBlade,
						offset * 0.125f
					).shader = shaderData;
				}
				if (++Projectile.ai[1] > 0) {
					Projectile.ai[1] = -Projectile.ai[0];
					Projectile.NewProjectile(
						Projectile.GetSource_FromThis(),
						Projectile.Center,
						Projectile.DirectionTo(targetCenter) * 8,
						Terratotem_Laser.ID,
						Projectile.damage,
						Projectile.knockBack,
						Projectile.owner
					);
				}
			} else {
				if (Projectile.ai[1] < 0) Projectile.ai[1]++;
				else if (Projectile.ai[1] > 0) Projectile.ai[1]--;
			}

			#region Movement

			// Default movement parameters (here for attacking)
			float speed = 8f + (distanceToIdlePosition / 600f) * 8f;
			float inertia = 18f + (distanceToIdlePosition / 600f) * -9f;

			Projectile.tileCollide = false;
			if (distanceToIdlePosition > 12f) {
				// The immediate range around the player (when it passively floats about)

				// This is a simple movement formula using the two parameters and its desired direction to create a "homing" movement
				Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToIdlePosition.SafeNormalize(default) * speed) / inertia;
			} else if (Projectile.velocity == Vector2.Zero) {
				// If there is a case where it's not moving at all, give it a little "poke"
				Projectile.velocity.X = -0.15f;
				Projectile.velocity.Y = -0.05f;
			}
			#endregion

			#region Animation and visuals

			#endregion
			Life -= 0.25f;
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			return Projectile.Center.Clamp(targetHitbox).WithinRange(Projectile.Center, 64);
		}
		public override void OnKill(int timeLeft) {
			const float diameter = 16;
			Player owner = Main.player[Projectile.owner];
			ArmorShaderData shaderData = GameShaders.Armor.GetSecondaryShader(owner.cMinion, owner);
			for (int i = 0; i < 8; i++) {
				Vector2 offset = Main.rand.NextVector2CircularEdge(diameter, diameter) * Main.rand.NextFloat(0.2f, 1f);
				Dust.NewDustPerfect(
					Projectile.Center + offset,
					DustID.TerraBlade,
					offset * 0.125f
				).shader = shaderData;
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			lightColor = Color.Lerp(lightColor, new(1f, 1f, 1f, 0.8f), 0.7f);
			return true;
		}
	}
	public class Terratotem_Tab : ModProjectile, IArtifactMinion {
		public static int ID { get; private set; }
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
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Summon;
			Projectile.width = 20;
			Projectile.height = 20;
			Projectile.tileCollide = false;
			Projectile.friendly = true;
			Projectile.minion = true;
			Projectile.minionSlots = 1f;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 20;
			Projectile.ignoreWater = true;
			Projectile.netImportant = true;
			MaxLife = 15 * 45;
		}
		public override bool? CanCutTiles() => false;
		public override bool MinionContactDamage() => false;
		public override void OnSpawn(IEntitySource source) {
			Projectile.ai[0] = -1;
			Projectile.ai[1] = -1;
			List<int> indices = [];
			BitArray hasHat = new(Main.maxProjectiles);
			foreach (Projectile other in Main.ActiveProjectiles) {
				if (other.whoAmI == Projectile.whoAmI) continue;
				if (other.owner == Projectile.owner && other.type == Type) {
					if (other.ai[1] != -1) hasHat[(int)other.ai[1]] = true;
					indices.Add(other.identity);
				}
			}
			for (int i = 0; i < indices.Count; i++) {
				if (!hasHat[indices[i]]) {
					Projectile.ai[1] = indices[i];
					break;
				}
			}
			if (Projectile.GetRelatedProjectile(1)?.ModProjectile is Terratotem_Tab seat) {
				seat.Life = seat.MaxLife;
			}
			Projectile.netUpdate = true;
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			Projectile below = Projectile.GetRelatedProjectile(1);
			if (below?.active ?? false) {
				Projectile.position = below.position - Vector2.UnitY * below.height;
				Projectile.velocity = Vector2.Zero;
			} else {
				Vector2 idlePosition = player.Bottom;
				idlePosition.X -= 48f * player.direction;

				Vector2 vectorToIdlePosition = idlePosition - Projectile.Bottom;
				Projectile.position += vectorToIdlePosition;
				Projectile.velocity = Vector2.Zero;
			}
		}
	}
}
