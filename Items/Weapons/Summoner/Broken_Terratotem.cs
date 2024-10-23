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
using Origins.Items.Weapons.Magic;

namespace Origins.Items.Weapons.Summoner {
	public class Broken_Terratotem : ModItem, ICustomWikiStat {
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
			Item.width = 24;
			Item.height = 38;
			Item.useTime = 24;
			Item.useAnimation = 24;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noUseGraphic = true;
			Item.value = Item.sellPrice(gold: 1, silver: 50);
			Item.rare = ItemRarityID.Pink;
			Item.UseSound = SoundID.Item44;
			Item.buffType = Terratotem_Buff.ID;
			Item.shoot = Broken_Terratotem_Orb.ID;
			Item.noMelee = true;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			player.AddBuff(Item.buffType, 2);
			damage = Item.damage;
			float kb = knockback;
			CombinedHooks.ModifyShootStats(player, Item, ref position, ref velocity, ref type, ref damage, ref kb);
			Projectile projectile = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI, player.itemAnimation);
			projectile.originalDamage = Item.damage;
			return false;
		}
	}
}
namespace Origins.Buffs {
	public class Terratotem_Buff : ModBuff {
		public override string Texture => "Origins/Items/Weapons/Summoner/Minions/Terratotem_Orb";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			ID = Type;
		}

		public override void Update(Player player, ref int buffIndex) {
			if (player.ownedProjectileCounts[Broken_Terratotem_Orb.ID] > 0) {
				player.buffTime[buffIndex] = 18000;
			} else {
				player.DelBuff(buffIndex);
				buffIndex--;
			}
		}
	}
}

namespace Origins.Items.Weapons.Summoner.Minions {
	public class Broken_Terratotem_Orb : ModProjectile, IArtifactMinion {
		public override string Texture => "Origins/Items/Weapons/Summoner/Minions/Terratotem_Orb";
		public static int ID { get; private set; }
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
			Projectile.DamageType = DamageClass.Summon;
			Projectile.width = 24;
			Projectile.height = 24;
			Projectile.tileCollide = true;
			Projectile.friendly = false;
			Projectile.minion = true;
			Projectile.minionSlots = 1f;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
			Projectile.ignoreWater = false;
			MaxLife = 60 * 45;
		}
		public override bool? CanCutTiles() => false;
		public override bool MinionContactDamage() => true;

		public override void AI() {
			Player player = Main.player[Projectile.owner];
			Projectile.timeLeft = 2;

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
				if (++Projectile.ai[1] > Projectile.ai[0] * 0.5f) {
					Projectile.ai[1] = Projectile.ai[0] * -0.5f;
					Projectile.NewProjectile(
						Projectile.GetSource_FromThis(),
						Projectile.Center,
						Projectile.DirectionTo(targetCenter) * 8,
						ModContent.ProjectileType<Laser_Tag_Laser>(),
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
			Life--;
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
}