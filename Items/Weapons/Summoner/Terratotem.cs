using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Accessories;
using Origins.Items.Weapons.Summoner.Minions;
using Origins.Projectiles;
using ReLogic.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Summoner {
	public class Terratotem : ModItem, ICustomWikiStat {
		public static int MaxCount => 20;
		public string[] Categories => [
			"Artifact",
			"Minion"
		];
		public override void SetStaticDefaults() {
			ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // This lets the player target anywhere on the whole screen while using a controller
			ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
		}
		public override void SetDefaults() {
			Item.damage = 27;
			Item.DamageType = DamageClass.Summon;
			Item.knockBack = 1f;
			Item.mana = 48;
			Item.shootSpeed = 9f;
			Item.width = 24;
			Item.height = 38;
			Item.useTime = 24;
			Item.useAnimation = 24;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noUseGraphic = false;
			Item.value = Item.sellPrice(gold: 20);
			Item.rare = ItemRarityID.Yellow;
			Item.UseSound = SoundID.Item44;
			Item.buffType = Terratotem_Buff.ID;
			Item.shoot = Terratotem_Tab.ID;
			Item.noMelee = true;
		}
		public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Terratotem_Tab.ID] + player.ownedProjectileCounts[Broken_Terratotem_Tab.ID] < MaxCount;
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			player.AddBuff(Item.buffType, 2);
			int totalCount = player.ownedProjectileCounts[Terratotem_Tab.ID] + player.ownedProjectileCounts[Broken_Terratotem_Tab.ID] + 1;
			Projectile projectile = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI, player.itemAnimation);
			projectile.originalDamage = Item.damage;
			for (float i = projectile.minionSlots; i < 1; i += projectile.minionSlots) {
				if (++totalCount > MaxCount) break;
				projectile = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI, player.itemAnimation);
				projectile.originalDamage = Item.damage;
			}
			return false;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Broken_Terratotem>()
			.AddIngredient(ItemID.BabyBirdStaff)
			.AddIngredient(ItemID.ImpStaff)
			.AddIngredient(ItemID.OpticStaff)
			.AddIngredient(ItemID.PygmyStaff)
			.AddIngredient(ItemID.SanguineStaff)
			.AddIngredient(ItemID.Smolstar)
			.AddIngredient(ItemID.TempestStaff)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
}

namespace Origins.Items.Weapons.Summoner.Minions {
	#region balance
	public partial class Terratotem_Mask_Small {
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
		}
	}
	public partial class Terratotem_Mask_Medium {
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.SourceDamage *= 1.32f;
		}
	}
	public partial class Terratotem_Mask_Big {
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.SourceDamage *= 1.58f;
		}
		public override void ModifyDamageHitbox(ref Rectangle hitbox) {
			if (Projectile.ai[2] >= 10 && Projectile.ai[2] <= 20) {
				hitbox.Inflate(hitbox.Width * 2, hitbox.Height * 2);
			}
		}
	}
	#endregion balance
	public class Terratotem_Tab : SpeedModifierMinion, IArtifactMinion {
		public static int ID { get; private set; }
		public int MaxLife { get; set; }
		public float Life { get; set; }
		public bool CanDie {
			get {
				if (Projectile.localAI[0] > 0) return false;
				ArtifactMinionGlobalProjectile globalProjectile = Projectile.GetGlobalProjectile<ArtifactMinionGlobalProjectile>();
				ref bool isRespawned = ref globalProjectile.isRespawned;
				if (globalProjectile.CanRespawn(Projectile)) {
					isRespawned = true;
					Life = MaxLife;
					return false;
				}
				return true;
			}
		}
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
		Projectile bottomPart;
		int belowCount = -1;
		public Projectile GetBottom(out int count) {
			if (bottomPart is null || bottomPart.ModProjectile is not Terratotem_Tab) {
				Projectile current = Projectile;
				HashSet<Projectile> walked = [current];
				Projectile last = current;
				while (current is not null) {
					last = current;
					current = current.GetRelatedProjectile(1);
					if (current is null) break;
					if (current.owner != Projectile.owner || current.ModProjectile is not Terratotem_Tab) break;
					if (!walked.Add(current)) break;
				}
				belowCount = walked.Count;
				bottomPart = last;
			}
			count = belowCount;
			return bottomPart;
		}
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			// Sets the amount of frames this minion has on its spritesheet
			// This is necessary for right-click targeting
			ProjectileID.Sets.MinionTargettingFeature[Type] = true;

			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[Type] = true;
			// This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned
			ProjectileID.Sets.MinionSacrificable[Type] = true;
			OriginsSets.Projectiles.NoMildewSetTrail[Type] = true;
			if (GetType().GetProperty(nameof(ID)).GetSetMethod(true) is MethodInfo setID) setID.Invoke(null, [Type]);
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Summon;
			Projectile.width = 20;
			Projectile.height = 20;
			Projectile.tileCollide = false;
			Projectile.friendly = true;
			Projectile.minion = true;
			Projectile.minionSlots = 1;
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
				if (other.owner == Projectile.owner && other.ModProjectile is Terratotem_Tab) {
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
				seat.Projectile.netUpdate = true;
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
				if (bottom.whoAmI > Projectile.whoAmI) {
					Projectile.position += new Vector2(bottom.localAI[1], bottom.localAI[2]);
				}
				Projectile.velocity = Vector2.Zero;
				below.localAI[0] = 2;
			} else {
				Vector2 idlePosition = player.Bottom;
				idlePosition.X -= 48f * player.direction;

				Projectile.velocity = new Vector2(Projectile.localAI[1], Projectile.localAI[2]);
				Vector2 vectorToIdlePosition = (idlePosition - Projectile.Bottom).Normalized(out float dist);
				if (dist > 2000) {
					Projectile.Bottom = idlePosition;
					Projectile.velocity = Vector2.Zero;
					(Projectile.localAI[1], Projectile.localAI[2]) = Vector2.Zero;
					Projectile.netUpdate = true;
				} else {
					float speed = (dist > 16 * 50 ? 64 : 32) * SpeedModifier;
					(Projectile.localAI[1], Projectile.localAI[2]) = (Projectile.velocity + vectorToIdlePosition * Math.Min(dist, speed)) / 4;
				}
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
			Lighting.AddLight(Projectile.Center, 0.2f, 0.8f, 0.5f);
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
	public abstract class Terratotem_Mask_Base : SpeedModifierMinion {
		public AutoLoadingAsset<Texture2D> sideTexture;
		public AutoLoadingAsset<Texture2D> sideGlowTexture;
		public virtual int FrameCount => 1;
		public virtual bool CanPickupItems => true;
		public Terratotem_Mask_Base() {
			sideTexture = Texture + "_Side";
			sideGlowTexture = Texture + "_Side_Glow";
		}
		public override void SetStaticDefaults() {
			ProjectileID.Sets.MinionShot[Type] = true;
			base.SetStaticDefaults();
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
		readonly int[] othersTargetingItemsCounts = new int[Main.maxItems];
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
					int increase = 1;
					if (other.type == Type) increase++;
					switch (otherMask.targetData.TargetType) {
						case TargetType.NPC:
						othersTargetingCounts[otherMask.targetData.Index] += increase;
						break;

						case TargetType.Item:
						othersTargetingItemsCounts[otherMask.targetData.Index] += increase;
						break;
					}
				}
			}
			Player player = Main.player[Projectile.owner];
			const float max_distance = 2000f;
			float distanceFromTarget = max_distance;
			bool hasPriorityTarget = false;
			int sharingCount = int.MaxValue;
			void targetingAlgorithm(NPC npc, float targetPriorityMultiplier, bool isPriorityTarget, ref bool foundTarget) {
				bool isCurrentTarget = targetData.TargetType == TargetType.NPC && npc.whoAmI == targetData.Index;
				if ((isCurrentTarget || isPriorityTarget || !hasPriorityTarget) && npc.CanBeChasedBy() && player.Center.WithinRange(npc.Center, max_distance)) {
					Vector2 pos = Projectile.position;
					float between = Vector2.Distance(npc.Center, pos);
					between *= isCurrentTarget ? 0 : 1;
					bool closer = distanceFromTarget > between;
					bool lineOfSight = Collision.CanHitLine(pos, 8, 8, npc.position, npc.width, npc.height);
					switch (sharingCount.CompareTo(othersTargetingCounts[npc.whoAmI])) {
						case -1:
						closer = false;
						break;
						case 1:
						closer = true;
						break;
					}
					if ((closer || !foundTarget) && lineOfSight) {
						sharingCount = othersTargetingCounts[npc.whoAmI];
						distanceFromTarget = between;
						targetData.Index = npc.whoAmI;
						targetData.TargetType = TargetType.NPC;
						foundTarget = true;
						hasPriorityTarget = isPriorityTarget;
					}
				}
			}
			TargetData oldTargetData = targetData;
			targetData = default;
			bool foundTarget = player.GetModPlayer<OriginPlayer>().GetMinionTarget(targetingAlgorithm);
			if (targetData.TargetType == TargetType.Slot && CanPickupItems) {
				float bestPickupPriority = 0;
				for (int i = 0; i < Main.maxItems; i++) {
					Item item = Main.item[i];
					if (item.active) {
						bool isCurrentTarget = oldTargetData.TargetType == TargetType.Item && i == oldTargetData.Index;
						if (isCurrentTarget || oldTargetData.TargetType != TargetType.Item) {
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
			if (!player.ItemSpace(item).CanTakeItem) return 0;
			if (item.IsACoin) return float.Pow(2, item.type - ItemID.CopperCoin);
			if (item.IsCurrency) return 2;
			if (item.type is ItemID.Heart or ItemID.CandyApple or ItemID.CandyCane) return (1 - player.statLife / (float)player.statLifeMax2) * RarityLoader.RarityCount * 2;
			if (item.OriginalRarity < ItemRarityID.Gray) return item.value / 5000;
			return item.OriginalRarity;
		}
		public virtual void DoMaskBehavior() {
			Rectangle targetRect = targetData.GetPosition(Projectile);
			Vector2 targetPos = targetRect.Center();
			float speed = 8f * SpeedModifier;
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
					if (!Main.item[Index].active || !Main.player[projectile.owner].ItemSpace(Main.item[Index]).CanTakeItem) {
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
	public partial class Terratotem_Mask_Small : Terratotem_Mask_Base {
		public override bool? CanDamage() => Projectile.ai[1] != -1;
		public override int FrameCount => 4;
		public override bool CanPickupItems => false;
	}
	public partial class Terratotem_Mask_Medium : Terratotem_Mask_Base {
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.localNPCHitCooldown = -1;
		}
		public override bool? CanDamage() => Projectile.ai[2] > 0;
		public override void DoMaskBehavior() {
			if (Projectile.ai[2] > 0) {
				SoundEngine.PlaySound(SoundID.NPCDeath55.WithPitch(1.5f).WithVolume(0.8f), Projectile.Center);
				Projectile.ai[2]--;
				if (targetData.TargetType == TargetType.Item) {
					Item item = Main.item[targetData.Index];
					if (!item.active) return;
					Player player = Main.player[Projectile.owner];
					if (!Terrarian_Voodoo_Doll.PreventItemPickup(item, player) && Projectile.Hitbox.Intersects(Main.item[targetData.Index].Hitbox)) {
						item.Center = player.Center;
					}
				}
				if (Projectile.ai[2] <= 0) Projectile.ResetLocalNPCHitImmunity();
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
					Projectile.velocity = (targetRect.Center() - Projectile.Center).Normalized(out _) * 12 * SpeedModifier;
					Projectile.ai[2] = (GetPosition(!left).X - Projectile.Center.X) / Projectile.velocity.X;
					ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.ChlorophyteLeafCrystalShot, new ParticleOrchestraSettings {
						PositionInWorld = targetRect.Center(),
						UniqueInfoPiece = (byte)(0.2f * 255f),
						MovementVector = Projectile.velocity.SafeNormalize(default)
					}, Projectile.owner);
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
	public partial class Terratotem_Mask_Big : Terratotem_Mask_Base {
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.localNPCHitCooldown = -1;
		}
		public override bool? CanDamage() => Projectile.ai[2] >= 10 && Projectile.ai[2] <= 20;
		public override void AI() {
			base.AI();
			if (Projectile.numUpdates == -1 && Projectile.ai[2] > 0 && Projectile.ai[2].Warmup(30, SpeedModifier)) {
				Projectile.ai[2] = 0;
				Projectile.ResetLocalNPCHitImmunity();
			}
		}
		public override void DoMaskBehavior() {
			if (Projectile.ai[2] > 0) {
				if (Projectile.ai[2] >= 10 && Projectile.ai[2] <= 20 && targetData.TargetType == TargetType.Item) {
					Player player = Main.player[Projectile.owner];
					Rectangle hitbox = Projectile.Hitbox;
					ModifyDamageHitbox(ref hitbox);
					foreach (Item item in Main.ActiveItems) {
						if (item.active && !Terrarian_Voodoo_Doll.PreventItemPickup(item, player) && hitbox.Intersects(item.Hitbox)) {
							item.Center = player.Center;
						}
					}
				}
				Projectile.velocity *= 0.90f;
				return;
			}
			Rectangle targetRect = targetData.GetPosition(Projectile);
			Vector2 targetPos = targetRect.Center();
			float speed = 8f * SpeedModifier;
			float inertia = 12f;
			Vector2 directionToTarget = (targetPos - Projectile.Center).Normalized(out _);
			switch (targetData.TargetType) {
				case TargetType.Item:
				case TargetType.NPC:
				Rectangle hitbox = Projectile.Hitbox;
				ModifyDamageHitbox(ref hitbox);
				if (Projectile.Hitbox.Intersects(targetRect)) {
					SoundEngine.PlaySound(SoundID.Item130.WithPitch(-0.2f), Projectile.Center);
					SoundEngine.PlaySound(SoundID.Item90.WithPitch(1f), Projectile.Center);
					SoundEngine.PlaySound(SoundID.Item84.WithPitch(1f), Projectile.Center);
					speed = 0;
					Projectile.ai[2] = 1;
					Projectile.velocity *= 0.5f;
				}
				break;

				default:
				case TargetType.Slot:
				if (Projectile.Hitbox.Intersects(targetRect)) {
					if (Projectile.Center.WithinRange(targetPos, 16)) Projectile.ai[1] = -1;
				}
				break;
			}
			Projectile.velocity = (Projectile.velocity * (inertia - 1) + directionToTarget * speed) / inertia;
		}
		public override void PostDraw(Color lightColor) {
			float attackFactor = 1 - Math.Abs(Projectile.ai[2] - 15) / 10f;
			if (attackFactor <= 0) return;
			Color color = Projectile.GetAlpha(new Color(180, 255, 180)) * attackFactor * attackFactor * 0.8f;
			color.A = 0;
			Main.instance.LoadProjectile(ProjectileID.MedusaHeadRay);
			Texture2D texture = TextureAssets.Projectile[ProjectileID.MedusaHeadRay].Value;
			Vector2 origin = texture.Size() * new Vector2(0.5f, 1f);
			const float beams = 9f;

			Vector2 basePos = Projectile.Center + Vector2.UnitY * Projectile.gfxOffY - Main.screenPosition;
			for (int i = 0; i < beams; i++) {
				float rotation = (i / (float)beams) * MathHelper.TwoPi - Main.GlobalTimeWrappedHourly + float.Sin(Main.GlobalTimeWrappedHourly * i) * 0.5f;
				float scale = Projectile.scale * (0.15f + 0.6f * attackFactor);
				Main.EntitySpriteDraw(
					texture,
					basePos + Projectile.rotation.ToRotationVector2().RotatedBy(MathHelper.TwoPi * (1f / beams) * i + Main.GlobalTimeWrappedHourly) * 4f * Projectile.scale,
					null,
					color,
					rotation,
					origin,
					new Vector2(scale * 1.5f, scale),
					SpriteEffects.None
				);
			}
		}
	}
}
