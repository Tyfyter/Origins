﻿using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Accessories;
using Origins.Items.Weapons.Summoner.Minions;
using Origins.Projectiles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.UI;
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
		public bool CanDie => Projectile.localAI[0] <= 0;
		public float SacrificeAvoidance {
			get {
				if (Projectile.localAI[0] <= 0) {
					return (Life / MaxLife) * Projectile.minionSlots;
				}
				GetBottom(out int count);
				return 1 + 1f / count;
			}
		}
		public bool DrawHealthBar(Vector2 position, float light, bool inBuffList) {
			if (Projectile.localAI[0] > 0) return false;
			if (!inBuffList) {
				Projectile bottom = GetBottom(out _);
				position = bottom.Bottom + new Vector2(0, bottom.gfxOffY + 2);
			}
			if (Life > 0) {
				Main.instance.DrawHealthBar(
					position.X, position.Y,
					(int)Life,
					MaxLife,
					light,
					0.85f
				);
			} else {
				Main.spriteBatch.Draw(
					TextureAssets.Hb2.Value,
					position - Main.screenPosition,
					null,
					Color.Gray * light,
					0f,
					new Vector2(18f, 0),
					0.85f,
					SpriteEffects.None,
				0);
			}
			return true;
		}
		Projectile GetBottom(out int count) {
			Projectile current = Projectile;
			HashSet<Projectile> walked = [current];
			Projectile last = current;
			while (current is not null) {
				last = current;
				current = current.GetRelatedProjectile(1);
				if (current is null) break;
				if (current.owner != Projectile.owner || current.type != Type) break;
				if (!walked.Add(current)) break;
			}
			count = walked.Count;
			return last;
		}
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
		public virtual int GetMask() {
			GetBottom(out int count);
			switch (count) {
				case 1:
				case 2:
				case 5:
				return ModContent.ProjectileType<Terratotem_Mask_Small>();

				case 3:
				case 6:
				case 7:
				return ModContent.ProjectileType<Terratotem_Mask_Medium>();

				case 4:
				default:
				return ModContent.ProjectileType<Terratotem_Mask_Big>();
			}
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];

			if (player.dead || !player.active) {
				player.ClearBuff(Terratotem_Buff.ID);
			} else if (player.HasBuff(Terratotem_Buff.ID)) {
				Projectile.timeLeft = 2;
			}

			Projectile.localAI[0].Cooldown();
			Projectile below = Projectile.GetRelatedProjectile(1);
			if (below?.active ?? false) {
				Projectile bottom = GetBottom(out int count);
				Projectile.position = bottom.position - Vector2.UnitY * Projectile.height * (count - 1);
				if (bottom.whoAmI > Projectile.whoAmI) Projectile.position += player.velocity;
				Projectile.velocity = Vector2.Zero;
				below.localAI[0] = 2;
			} else {
				Vector2 idlePosition = player.Bottom;
				idlePosition.X -= 48f * player.direction;

				Vector2 vectorToIdlePosition = idlePosition - Projectile.Bottom;
				Projectile.position += vectorToIdlePosition;
				Projectile.velocity = Vector2.Zero;
			}
			if (Projectile.GetRelatedProjectile(0) is Projectile mask && mask.active) {
				if (mask.ai[1] == -1) {
					mask.Center = Projectile.Center;
				}
			} else if (Projectile.IsLocallyOwned()) {
				mask = Projectile.NewProjectileDirect(
					Projectile.GetSource_FromAI(),
					Projectile.position,
					Vector2.Zero,
					GetMask(),
					Projectile.damage,
					Projectile.knockBack,
					ai0: Projectile.identity
				);
				mask.originalDamage = Projectile.originalDamage;
				Projectile.ai[0] = mask.identity;
				Projectile.netUpdate = true;
			}
			Lighting.AddLight(Projectile.Center, 0.2f, 0.8f, 0.2f);
			if (Projectile.localAI[0] <= 0) Life -= 0.25f;
		}
		public override void PostDraw(Color lightColor) {
			Projectile maskProj = Projectile.GetRelatedProjectile(0);
			if ((maskProj?.active ?? false) && Projectile.whoAmI > maskProj.whoAmI && maskProj.ModProjectile is Terratotem_Mask_Base mask) mask.Draw(lightColor);
		}
		public override void OnKill(int timeLeft) {
			if (Projectile.GetRelatedProjectile(0) is Projectile mask && mask.active) {
				mask.Kill();
			}
		}
	}
	public abstract class Terratotem_Mask_Base : ModProjectile {
		public AutoLoadingAsset<Texture2D> sideTexture;
		public AutoLoadingAsset<Texture2D> sideGlowTexture;
		public virtual int FrameCount => 1;
		public virtual bool CanPickupItems => true;
		public Terratotem_Mask_Base() {
			sideTexture = GetType().GetDefaultTMLName() + "_Side";
			sideGlowTexture = GetType().GetDefaultTMLName() + "_Side_Glow";
		}
		public override void SetStaticDefaults() {
			ProjectileID.Sets.MinionShot[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Summon;
			Projectile.width = 20;
			Projectile.height = 20;
			Projectile.tileCollide = false;
			Projectile.friendly = true;
			Projectile.minionSlots = 0f;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 20;
			Projectile.ignoreWater = true;
			Projectile.netImportant = true;
			Projectile.extraUpdates = 1;
			if (FrameCount > 1) Projectile.frame = Main.rand.Next(FrameCount);
		}
		protected TargetData targetData = new(TargetType.Slot, 0);
		public override void OnSpawn(IEntitySource source) {
			Projectile.ai[1] = -1;
		}
		readonly int[] othersTargetingCounts = new int[Main.maxNPCs];
		readonly int[] othersTargetingItemsCounts = new int[Main.maxNPCs];
		public override void AI() {
			Projectile slot = Projectile.GetRelatedProjectile(0);
			if (slot?.active != true) {
				Projectile.Kill();
				return;
			}
			Projectile.timeLeft = 5;
			Array.Clear(othersTargetingCounts);
			Array.Clear(othersTargetingItemsCounts);
			foreach (Projectile other in Main.ActiveProjectiles) {
				if (other.whoAmI == Projectile.whoAmI) continue;
				if (other.owner == Projectile.owner && other.ModProjectile is Terratotem_Mask_Base otherMask) {
					switch (otherMask.targetData.TargetType) {
						case TargetType.NPC:
						othersTargetingCounts[otherMask.targetData.Index]++;
						break;

						case TargetType.Item:
						othersTargetingItemsCounts[otherMask.targetData.Index]++;
						break;
					}
				}
			}
			float distanceFromTarget = 2000f;
			bool hasPriorityTarget = false;
			int sharingCount = int.MaxValue;
			void targetingAlgorithm(NPC npc, float targetPriorityMultiplier, bool isPriorityTarget, ref bool foundTarget) {
				bool isCurrentTarget = targetData.TargetType == TargetType.NPC && npc.whoAmI == targetData.Index;
				if ((isCurrentTarget || isPriorityTarget || !hasPriorityTarget) && npc.CanBeChasedBy()) {
					Vector2 pos = Projectile.position;
					float between = Vector2.Distance(npc.Center, pos);
					between *= isCurrentTarget ? 0 : 1;
					bool closer = distanceFromTarget > between;
					bool lineOfSight = Collision.CanHitLine(pos, 8, 8, npc.position, npc.width, npc.height);
					if ((closer || sharingCount > othersTargetingCounts[npc.whoAmI] || !foundTarget) && lineOfSight) {
						sharingCount = othersTargetingCounts[npc.whoAmI];
						distanceFromTarget = between;
						targetData.Index = npc.whoAmI;
						targetData.TargetType = TargetType.NPC;
						foundTarget = true;
						hasPriorityTarget = isPriorityTarget;
					}
				}
			}
			Player player = Main.player[Projectile.owner];
			TargetData oldTargetData = targetData;
			bool foundTarget = player.GetModPlayer<OriginPlayer>().GetMinionTarget(targetingAlgorithm);
			if (targetData.TargetType == TargetType.Slot && CanPickupItems) {
				float bestPickupPriority = 0;
				for (int i = 0; i < Main.maxItems; i++) {
					Item item = Main.item[i];
					if (item.active) {
						bool isCurrentTarget = targetData.TargetType == TargetType.Item && i == targetData.Index;
						if (isCurrentTarget || targetData.TargetType != TargetType.Item) {
							Vector2 pos = Projectile.position;
							float between = Vector2.Distance(item.Center, pos);
							between *= isCurrentTarget ? 0 : 1;
							bool closer = distanceFromTarget > between;
							bool lineOfSight = Collision.CanHitLine(pos, 8, 8, item.position, item.width, item.height);
							float pickupPriority = GetItemPickupPriority(player, item);
							if (pickupPriority <= bestPickupPriority) closer = false;
							if (lineOfSight && (closer || pickupPriority > bestPickupPriority) && othersTargetingItemsCounts[i] <= 0 && !Terrarian_Voodoo_Doll.PreventItemPickup(item, player)) {
								distanceFromTarget = between;
								targetData.Index = i;
								targetData.TargetType = TargetType.Item;
							}
						}
					}
				}
			}
			if (targetData != oldTargetData) {
				Projectile.netUpdate = true;
			}
			if (targetData.TargetType != TargetType.Slot) Projectile.ai[1] = 0;
			if (Projectile.ai[1] == -1) {
				Projectile.Center = slot.Center;
				Projectile.velocity = Vector2.Zero;
				return;
			}
			DoMaskBehavior();
		}
		public virtual float GetItemPickupPriority(Player player, Item item) {
			if (item.IsACoin) return float.Pow(2, item.type - ItemID.CopperCoin);
			if (item.IsCurrency) return 2;
			if (item.type is ItemID.Heart or ItemID.CandyApple or ItemID.CandyCane) return (1 - player.statLife / (float)player.statLifeMax2) * RarityLoader.RarityCount * 2;
			return item.OriginalRarity;
		}
		public virtual void DoMaskBehavior() {
			Rectangle targetRect = targetData.GetPosition(Projectile);
			Vector2 targetPos = targetRect.Center();
			float speed = 8f;
			float inertia = 12f;
			Vector2 directionToTarget = (targetPos - Projectile.Center).Normalized(out _);
			if (Projectile.Hitbox.Intersects(targetRect)) {
				switch (targetData.TargetType) {
					case TargetType.Slot:
					if (Projectile.Center.WithinRange(targetPos, 16)) Projectile.ai[1] = -1;
					break;

					case TargetType.NPC:
					directionToTarget = directionToTarget.RotatedByRandom(3);
					break;
				}
			}
			Projectile.velocity = (Projectile.velocity * (inertia - 1) + directionToTarget * speed) / inertia;
		}
		public void Draw(Color lightColor) {
			if (Projectile.ai[1] == -1) {
				Texture2D texture = TextureAssets.Projectile[Type].Value;
				Rectangle frame = texture.Frame(FrameCount, frameX: Projectile.frame);
				if (FrameCount > 1) frame.Width -= 2;
				Main.EntitySpriteDraw(
					texture,
					Projectile.Bottom - Main.screenPosition,
					frame,
					lightColor,
					Projectile.rotation,
					frame.Size() * new Vector2(0.5f, 1),
					Projectile.scale,
					SpriteEffects.None
				);
			} else {
				SpriteEffects effects = Projectile.direction > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
				Rectangle frame = sideTexture.Frame(FrameCount, frameX: Projectile.frame);
				if (FrameCount > 1) frame.Width -= 2;
				Main.EntitySpriteDraw(
					sideTexture,
					Projectile.Bottom - Main.screenPosition,
					frame,
					lightColor,
					Projectile.rotation,
					frame.Size() * new Vector2(0.5f, 1),
					Projectile.scale,
					effects
				);
				Main.EntitySpriteDraw(
					sideGlowTexture,
					Projectile.Bottom - Main.screenPosition,
					frame,
					Color.White,
					Projectile.rotation,
					frame.Size() * new Vector2(0.5f, 1),
					Projectile.scale,
					effects
				);
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			if (Projectile.whoAmI > Projectile.GetRelatedProjectile(0).whoAmI) Draw(lightColor);
			return false;
		}
		public override void SendExtraAI(BinaryWriter writer) {
			targetData.Write(writer);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			targetData = TargetData.Read(reader);
		}
		public record struct TargetData(TargetType TargetType, int Index) {
			public readonly void Write(BinaryWriter writer) {
				writer.Write((byte)TargetType);
				if (TargetType != TargetType.Slot) writer.Write(Index);
			}
			public static TargetData Read(BinaryReader reader) {
				TargetData result = new() {
					TargetType = (TargetType)reader.ReadByte()
				};
				if (result.TargetType != TargetType.Slot) result.Index = reader.ReadInt32();
				return result;
			}
			public Rectangle GetPosition(Projectile projectile) {
				switch (TargetType) {
					case TargetType.NPC:
					if (!Main.npc[Index].active || !Main.npc[Index].CanBeChasedBy(projectile)) {
						TargetType = TargetType.Slot;
						goto case TargetType.Slot;
					}
					return Main.npc[Index].Hitbox;

					case TargetType.Item:
					if (!Main.item[Index].active) {
						TargetType = TargetType.Slot;
						goto case TargetType.Slot;
					}
					return Main.item[Index].Hitbox;

					default:
					case TargetType.Slot:
					return projectile.GetRelatedProjectile(0)?.Hitbox ?? projectile.Hitbox;
				}
			}
		}
		public enum TargetType : byte {
			Slot,
			NPC,
			Item
		}
	}
	public class Terratotem_Mask_Small : Terratotem_Mask_Base {
		public override int FrameCount => 4;
		public override bool CanPickupItems => false;
	}
	public class Terratotem_Mask_Medium : Terratotem_Mask_Base {
		public override void DoMaskBehavior() {
			if (Projectile.ai[2] > 0) {
				Projectile.ai[2]--;
				if (targetData.TargetType == TargetType.Item) {
					Item item = Main.item[targetData.Index];
					if (!item.active) return;
					Player player = Main.player[Projectile.owner];
					if (!Terrarian_Voodoo_Doll.PreventItemPickup(item, player) && Projectile.Hitbox.Intersects(Main.item[targetData.Index].Hitbox)) {
						item.Center = player.Center;
					}
				}
				return;
			}
			Rectangle targetRect = targetData.GetPosition(Projectile);
			Vector2 targetPos = targetRect.Center();
			if (targetData.TargetType != TargetType.Slot) {
				Vector2 GetPosition(bool left) {
					return left ? targetRect.Left() - new Vector2(32, 0) : targetRect.Right() + new Vector2(32, 0);
				}
				bool left = Projectile.Center.X < targetRect.Center.X;
				targetPos = GetPosition(left);
				targetPos.Y -= 24;
				if (Projectile.Center.WithinRange(targetPos, 16)) {
					Projectile.velocity = (targetRect.Center() - Projectile.Center).Normalized(out _) * 12;
					Projectile.ai[2] = (GetPosition(!left).X - Projectile.Center.X) / Projectile.velocity.X;
				}
			}
			float speed = 8f;
			float inertia = 12f;
			Vector2 directionToTarget = (targetPos - Projectile.Center).Normalized(out _);
			if (Projectile.Hitbox.Intersects(targetRect) && targetData.TargetType == TargetType.Slot) {
				if (Projectile.Center.WithinRange(targetPos, 16)) Projectile.ai[1] = -1;
			}
			Projectile.velocity = (Projectile.velocity * (inertia - 1) + directionToTarget * speed) / inertia;
		}
	}
	public class Terratotem_Mask_Big : Terratotem_Mask_Base {
	}
}
