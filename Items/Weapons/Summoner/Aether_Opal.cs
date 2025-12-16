using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Items.Weapons.Summoner;
using Origins.Items.Weapons.Summoner.Minions;
using Origins.NPCs.MiscB.Shimmer_Construct;
using Origins.Projectiles;
using PegasusLib;
using PegasusLib.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Liquid;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Summoner {
	public class Aether_Opal : ModItem, ICustomWikiStat {
		public override void SetDefaults() {
			Item.damage = 21;
			Item.DamageType = DamageClass.Summon;
			Item.knockBack = 4;
			Item.mana = 10;
			Item.width = 28;
			Item.height = 28;
			Item.useTime = 36;
			Item.useAnimation = 36;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Orange;
			Item.UseSound = SoundID.Item44;
			Item.buffType = Shimmer_Guardian_Buff.ID;
			Item.shoot = Shimmer_Guardian_Counter.ID;
			Item.noMelee = true;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			player.AddBuff(Item.buffType, 2);
			player.SpawnMinionOnCursor(source, player.whoAmI, type, Item.damage, Item.knockBack);
			return false;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Aetherite_Bar>(), 8)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
namespace Origins.Buffs {
	[ReinitializeDuringResizeArrays]
	public class Shimmer_Guardian_Buff : MinionBuff {
		internal static int?[] prefixValueCache = PrefixID.Sets.Factory.CreateCustomSet<int?>(null);
		public static int ID { get; private set; }
		public override IEnumerable<int> ProjectileTypes() => [
			Shimmer_Guardian_Counter.ID
		];
		static (Projectile projectile, int prefix) GetMostValuablePrefixOwner(int owner) {
			int minionCounterType = Shimmer_Guardian_Counter.ID;
			Item evaluator = new(ModContent.ItemType<Aether_Opal>());
			int GetPrefixValue(int prefix) {
				evaluator.ModItem?.SetDefaults();
				evaluator.Prefix(prefix);
				return evaluator.value;
			}
			int bestValue = int.MinValue;
			int bestValuePrefix = 0;
			Projectile bestValueOwner = null;
			foreach (Projectile projectile in Main.ActiveProjectiles) {
				if (projectile.owner == owner && projectile.type == minionCounterType) {
					int prefix = projectile.GetGlobalProjectile<OriginGlobalProj>().Prefix;
					int value = prefixValueCache[prefix] ??= GetPrefixValue(prefix);
					if (value > bestValue) {
						bestValueOwner = projectile;
						bestValuePrefix = prefix;
						bestValue = value;
					}
				}
			}
			return (bestValueOwner, bestValuePrefix);
		}
		public override void Update(Player player, ref int buffIndex) {
			int oldBuffIndex = buffIndex;
			base.Update(player, ref buffIndex);
			player.OriginPlayer().shimmerGuardianMinion = buffIndex == oldBuffIndex;
			if (player.whoAmI == Main.myPlayer) {
				int actualMinionType = Shimmer_Guardian.ID;
				if (buffIndex != oldBuffIndex) {
					foreach (Projectile projectile in Main.ActiveProjectiles) {
						if (projectile.owner == player.whoAmI && projectile.type == actualMinionType) {
							projectile.Kill();
						}
					}
				} else {
					(Projectile prefixerProj, int prefix) = GetMostValuablePrefixOwner(player.whoAmI);
					if (player.ownedProjectileCounts[actualMinionType] < 1) {
						Projectile.NewProjectileDirect(
							prefixerProj.GetSource_FromThis("ShimmerGuardianTierSwap"),
							player.Center,
							Vector2.Zero,
							actualMinionType,
							prefixerProj.originalDamage,
							prefixerProj.knockBack,
							player.whoAmI
						);
					} else {
						foreach (Projectile projectile in Main.ActiveProjectiles) {
							if (projectile.owner == player.whoAmI && projectile.type == actualMinionType) {
								projectile.GetGlobalProjectile<OriginGlobalProj>().Prefix = prefix;
								projectile.netUpdate = true;
								projectile.originalDamage = prefixerProj.originalDamage;
								projectile.OriginalArmorPenetration = prefixerProj.OriginalArmorPenetration;
								projectile.knockBack = prefixerProj.knockBack;
								break;
							}
						}
					}
				}
			}
		}
	}
}

namespace Origins.Items.Weapons.Summoner.Minions {
	public class Shimmer_Guardian : ModProjectile {
		public static int GetModifiedDamage(int baseDamage, int extraSlotsUsed) {
			return (int)(baseDamage * (1 + 0.5f * extraSlotsUsed));
		}
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			// This is necessary for right-click targeting
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;

			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[Projectile.type] = true;
			// This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
			ID = Type;
		}

		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Summon;
			Projectile.width = 54;
			Projectile.height = 58;
			Projectile.tileCollide = true;
			Projectile.friendly = false;
			Projectile.minion = true;
			Projectile.minionSlots = 0f;
			Projectile.penetrate = -1;
			Projectile.netImportant = true;
		}

		// Here you can decide if your minion breaks things like grass or pots
		public override bool? CanCutTiles() {
			return false;
		}

		public override void AI() {
			const float attack_time = 10;
			Player player = Main.player[Projectile.owner];

			#region Active check
			// This is the "active check", makes sure the minion is alive while the player is alive, and despawns if not
			if (player.dead || !player.active) {
				player.OriginPlayer().shimmerGuardianMinion = false;
			} else if (player.OriginPlayer().shimmerGuardianMinion) {
				Projectile.timeLeft = 2;
			}
			int extraSlotsUsed = player.ownedProjectileCounts[Shimmer_Guardian_Counter.ID] - 1;
			Projectile.damage = GetModifiedDamage(Projectile.damage, extraSlotsUsed);
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
			float distanceFromTarget = 25f * 16f;
			Vector2 targetCenter = default;
			int target = -1;
			void targetingAlgorithm(NPC npc, float targetPriorityMultiplier, bool isPriorityTarget, ref bool foundTarget) {
				if (!isPriorityTarget && distanceFromTarget > 15f * 16f) {
					distanceFromTarget = 15f * 16f;
				}
				if (npc.CanBeChasedBy()) {
					float between = Vector2.Distance(player.Center, npc.Center);
					bool closest = Vector2.Distance(player.Center, targetCenter) > between;
					bool inRange = between < distanceFromTarget;
					bool lineOfSight = isPriorityTarget || Collision.CanHitLine(player.position + new Vector2(1, 4), player.width - 2, player.height - 8, npc.position, npc.width, npc.height);
					// Additional check for this specific minion behavior, otherwise it will stop attacking once it dashed through an enemy while flying though tiles afterwards
					// The number depends on various parameters seen in the movement code below. Test different ones out until it works alright
					bool closeThroughWall = between < 100f;
					if (closest && inRange && (lineOfSight || closeThroughWall)) {
						distanceFromTarget = between;
						targetCenter = npc.Center;
						target = npc.whoAmI;
						foundTarget = true;
					}
				}
			}
			bool foundTarget = player.GetModPlayer<OriginPlayer>().GetMinionTarget(targetingAlgorithm);
			Projectile.ai[1] = target;
			if (player.ownedProjectileCounts[Shimmer_Guardian_Shard.ID] > 0) {
				Projectile.Center = idlePosition;
				Projectile.hide = true;
				return;
			}
			Projectile.hide = false;
			#endregion

			Vector2 direction = targetCenter - Projectile.Center;
			float projDistanceFromTarget = direction.Length();
			direction /= projDistanceFromTarget;

			int originalDamage = GetModifiedDamage(Projectile.originalDamage, extraSlotsUsed);
			if (foundTarget) {
				if (++Projectile.ai[0] >= attack_time) {
					int mode = projDistanceFromTarget > 16 * 10 ? 0 : 1;
					SoundEngine.PlaySound(SoundID.Item9.WithPitchRange(-1f, -0.2f), Projectile.Center);
					SoundEngine.PlaySound(SoundID.Item60.WithPitchRange(-1f, -0.2f), Projectile.Center);
					int count = 6;
					for (int i = count; i > 0; i--) {
						Projectile.SpawnProjectile(
							Projectile.GetSource_FromThis(),
							Projectile.Center,
							direction.RotatedBy(1f * ((i / (float)count - 0.5f) + Main.rand.NextFloat(-0.1f, 0.1f))) * 12,
							Shimmer_Guardian_Shard.ID,
							originalDamage,
							Projectile.knockBack
						);
					}
				}
			} else if (Projectile.ai[0] > 0) {
				Projectile.ai[0] -= 0.25f;
			}

			#region Movement

			// Default movement parameters (here for attacking)
			float speed = 12f;
			float inertia = 8f;
			vectorToIdlePosition = vectorToIdlePosition.SafeNormalize(default);
			if (foundTarget) {
				float projDistanceFromPlayer = (Projectile.Center - player.Center).Length();
				Projectile.tileCollide = true;
				Vector2 realDirection = direction;
				Projectile.spriteDirection = Math.Sign(direction.X);
				if (projDistanceFromPlayer > 10f * 16f) {
					realDirection = vectorToIdlePosition;
					speed = 2f;
					inertia = 24f;
				} else if (projDistanceFromTarget > 72f) {
					speed = 6f;
					inertia = 32f;
				} else if (projDistanceFromTarget > 48f) {
					speed = 0f;
					inertia = 32f;
				} else {
					speed = -6f;
					inertia = 18f;
				}
				// Minion has a target: attack (here, fly towards the enemy)
				Projectile.velocity = (Projectile.velocity * (inertia - 1) + realDirection * speed) / inertia;
				Projectile.localAI[2] *= 0.9f;
			} else {
				Projectile.tileCollide = false;
				Projectile.spriteDirection = Math.Sign(vectorToIdlePosition.X);
				if (distanceToIdlePosition > 600f) {
					speed = 16f;
					inertia = 36f;
				} else if (distanceToIdlePosition < 16) {
					speed = 0f;
					inertia = 24f;
					Projectile.spriteDirection = player.direction;
				}

				Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToIdlePosition * speed) / inertia;
				if (Projectile.velocity.LengthSquared() < 0.01f) {
					// If there is a case where it's not moving at all, give it a little "poke"
					Projectile.localAI[2] += MathF.Sin(Main.GlobalTimeWrappedHourly) * 4;
				} else {
					Projectile.localAI[2] *= 0.9f;
				}
			}
			#endregion

			#region Animation and visuals
			// So it will lean slightly towards the direction it's moving
			Projectile.rotation = Projectile.velocity.X * 0.05f;

			// This is a simple "loop through all frames from top to bottom" animation
			int frameSpeed = 5;
			if (++Projectile.frameCounter >= frameSpeed) {
				Projectile.frameCounter = 0;
				if (++Projectile.frame >= Main.projFrames[Projectile.type]) Projectile.frame = 0;
			}

			// Some visuals here
			Lighting.AddLight(Projectile.Center, Color.SandyBrown.ToVector3() * 0.18f);
			#endregion
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			width -= 2;
			height -= 8;
			return true;
		}
	}
	public class Shimmer_Guardian_Shard : ModProjectile {
		public override string Texture => typeof(Shimmer_Drone).GetDefaultTMLName();
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailingMode[Type] = 3;
			ProjectileID.Sets.TrailCacheLength[Type] = 30;
			// This is necessary for right-click targeting
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;

			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[Projectile.type] = true;
			// This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Summon;
			Projectile.width = 24;
			Projectile.height = 24;
			Projectile.tileCollide = false;
			Projectile.friendly = true;
			Projectile.minion = true;
			Projectile.minionSlots = 0f;
			Projectile.extraUpdates = 1;
			Projectile.penetrate = -1;
			Projectile.netImportant = true;
			Projectile.localNPCHitCooldown = 120;
			Projectile.usesLocalNPCImmunity = true;
		}
		public override bool MinionContactDamage() => true;
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			if (player.ownedProjectileCounts[Shimmer_Guardian.ID] <= 0) {
				Projectile.Kill();
				return;
			}
			Projectile owner = null;
			foreach (Projectile projectile in Main.ActiveProjectiles) {
				if (projectile.owner == player.whoAmI && projectile.type == Shimmer_Guardian.ID) {
					owner = projectile;
					break;
				}
			}
			if (owner is null) {
				Projectile.Kill();
				return;
			}
			bool beDrone = owner.ai[1] != -1 && !Main.npc[(int)owner.ai[1]].Center.IsWithin(owner.Center, 16 * 25);
			switch ((int)Projectile.ai[0]) {
				case 0: {
					if (Projectile.velocity == Vector2.Zero) {
						if (++Projectile.ai[1] > 20) {
							if (beDrone && owner.ai[1] != -1) {
								Projectile.ai[0] = 1;
								Projectile.ai[2] = owner.ai[1];
								Projectile.ai[1] = (Main.npc[(int)Projectile.ai[2]].Center - Projectile.Center).ToRotation();
							} else {
								Projectile.ai[0] = 2;
								Projectile.ai[1] = 0;
							}
						}
					} else {
						Vector2 newVelocity = Collision.TileCollision(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height, true, true);
						if (newVelocity != Projectile.velocity) {
							Projectile.position += newVelocity + Projectile.velocity.WithMaxLength(8);
							Projectile.velocity = Vector2.Zero;
							Projectile.ai[1] = 0;
						} else if (++Projectile.ai[1] > 50) {
							if (beDrone && owner.ai[1] != -1) {
								Projectile.ai[0] = 1;
								Projectile.ai[2] = owner.ai[1];
								Projectile.localAI[0] = (Main.npc[(int)Projectile.ai[2]].Center - Projectile.Center).ToRotation();
							} else if ((Projectile.velocity *= 0.85f).LengthSquared() < 1) {
								Projectile.velocity = Vector2.Zero;
								Projectile.ai[1] = 0;
							}
						} else {
							Projectile.velocity *= 0.97f;
						}
						Projectile.rotation += Projectile.direction * 0.05f * Projectile.velocity.Length();
					}
					break;
				}

				case -1: {
					Vector2 targetPos = owner.Center;

					Vector2 diffToOwner = targetPos - Projectile.Center;
					float dist = diffToOwner.Length();
					if (dist > 4000) {
						Projectile.Kill();
						return;
					}
					bool passed = dist < 16 * 4 && (dist <= 0 || Vector2.Dot(diffToOwner / dist, Projectile.ai[1].ToRotationVector2()) < 0);
					if (passed) {
						Projectile.ai[2] = owner.ai[1];
						if (!beDrone || player.ownedProjectileCounts[Type] < 3 || Projectile.ai[2] == -1) {
							Projectile.ai[0] = 2;
						} else {
							Projectile.ai[0] = 1;
							Projectile.ai[1] = (Main.npc[(int)Projectile.ai[2]].Center - Projectile.Center).ToRotation();
						}
						Projectile.ResetLocalNPCHitImmunity();
					} else {
						Projectile.velocity += Projectile.DirectionTo(targetPos) * 0.5f;
						Projectile.velocity = Projectile.velocity.SafeNormalize(default) * 8;
					}
					Projectile.rotation = (-Projectile.velocity).ToRotation() - MathHelper.PiOver2;
					Projectile.spriteDirection = Math.Sign(-Projectile.velocity.X);
					break;
				}
				case 1: {
					if (Projectile.ai[2] == -1) {
						Projectile.ai[0] = -1;
						goto case -1;
					}
					Vector2 targetPos = Main.npc[(int)Projectile.ai[2]].Center;

					if (Vector2.Dot(Projectile.DirectionTo(targetPos), Projectile.ai[1].ToRotationVector2()) < 0) {
						Projectile.ai[0] = -1;
						Projectile.ai[1] = (targetPos - Projectile.Center).ToRotation();
					}
					Projectile.velocity += Projectile.DirectionTo(targetPos) * 0.5f;
					Projectile.velocity = Projectile.velocity.SafeNormalize(default) * 8;
					Projectile.rotation = (-Projectile.velocity).ToRotation() - MathHelper.PiOver2;
					Projectile.spriteDirection = Math.Sign(-Projectile.velocity.X);
					break;
				}

				case 2: {
					Vector2 directionToOwner = owner.Center - Projectile.Center;
					float distance = directionToOwner.Length();
					if (distance < 32) {
						Projectile.Kill();
					} else {
						directionToOwner /= distance;
						Projectile.velocity = (Projectile.velocity + directionToOwner * MathF.Pow(Projectile.velocity.LengthSquared() + 0.1f, 0.125f) / 2).WithMaxLength(distance);
						PolarVec2 movement = (PolarVec2)Projectile.velocity;
						movement.Theta = directionToOwner.ToRotation();
						Projectile.velocity = (Vector2)movement;
					}
					Projectile.rotation += Projectile.direction * 0.05f * Projectile.velocity.Length();
					break;
				}
			}
		}
		private static VertexStrip _vertexStrip = new();
		public override bool PreDraw(ref Color lightColor) {
			if (Projectile.ai[0] == 0 || (Projectile.ai[0] == 2 && Projectile.velocity == Vector2.Zero)) {
				MiscShaderData miscShaderData = GameShaders.Misc["RainbowRod"];
				miscShaderData.UseSaturation(-2.8f);
				miscShaderData.UseOpacity(4f);
				miscShaderData.Apply();
				_vertexStrip.PrepareStripWithProceduralPadding(Projectile.oldPos, Projectile.oldRot, StripColors, StripWidth, -Main.screenPosition + Projectile.Size / 2f);
				_vertexStrip.DrawTrail();
				Main.pixelShader.CurrentTechnique.Passes[0].Apply();
				Color StripColors(float progressOnStrip) {
					if (float.IsNaN(progressOnStrip)) return Color.Transparent;
					Vector2 pos = Projectile.oldPos[(int)(progressOnStrip * (Projectile.oldPos.Length - 1))];
					return new(LiquidRenderer.GetShimmerBaseColor(pos.X, pos.Y));
				}
				float StripWidth(float progressOnStrip) {
					float num = 1f;
					float lerpValue = Utils.GetLerpValue(0f, 0.2f, progressOnStrip, clamped: true);
					num *= 1f - (1f - lerpValue) * (1f - lerpValue);
					return MathHelper.Lerp(0f, 32f * Utils.GetLerpValue(0f, 32f, Projectile.position.Distance(Projectile.oldPos[12]), clamped: true), num);
				}
			}
			if (Projectile.localAI[1] == 0) {
				Projectile.localAI[1] = Mod.GetGoreSlot("Gores/NPCs/Shimmer_Thing_Medium" + (Main.rand.Next(2) + 1));
			}
			Texture2D texture = TextureAssets.Gore[(int)Projectile.localAI[1]].Value;
			Main.EntitySpriteDraw(
				texture,
				Projectile.Center - Main.screenPosition,
				null,
				lightColor,
				Projectile.rotation,
				texture.Size() * 0.5f,
				1,
				default
			);
			return false;
		}
		public static AutoLoadingAsset<Texture2D> normalTexture = typeof(Shimmer_Drone).GetDefaultTMLName();
		public static AutoLoadingAsset<Texture2D> afTexture = typeof(Shimmer_Drone).GetDefaultTMLName() + "_AF";
		public override void PostDraw(Color lightColor) {
			if (OriginsModIntegrations.CheckAprilFools()) {
				TextureAssets.Projectile[Type] = afTexture;
			} else {
				TextureAssets.Projectile[Type] = normalTexture;
			}
		}
	}
	public class Shimmer_Guardian_Counter : ModProjectile {
		public override string Texture => base.Texture.Replace("_Counter", null); // this one actually doesn't need a texture
		public override void SetStaticDefaults() {
			// This is necessary for right-click targeting
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;

			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[Projectile.type] = true;
			// This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
			ID = Type;
		}
		public static int ID { get; private set; }
		public override void SetDefaults() {
			Projectile.netImportant = true;
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.penetrate = -1;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			Projectile.friendly = true;
			Projectile.minion = true;
			Projectile.minionSlots = 1f;
			Projectile.timeLeft = 60;
			Projectile.aiStyle = ProjAIStyleID.DesertTigerBall;
			Projectile.hide = true;
		}
		public override bool PreAI() {
			Player player = Main.player[Projectile.owner];
			if (player.ownedProjectileCounts[Projectile.type] > 1 && Projectile.localAI[0] == 0f) {
				Projectile.localAI[0] = 1f;
				SoundEngine.PlaySound(in SoundID.AbigailUpgrade, Projectile.Center);
			}
			ref bool shimmerGuardianMinion = ref player.OriginPlayer().shimmerGuardianMinion;
			if (player.dead) {
				shimmerGuardianMinion = false;
			}
			if (shimmerGuardianMinion) {
				Projectile.timeLeft = 2;
			}
			/*if (++Projectile.frameCounter >= 4) {
				Projectile.frameCounter = 0;
				if (++Projectile.frame >= 6) {
					Projectile.frame = 0;
				}
			}*/
			return true;
		}
	}
}
