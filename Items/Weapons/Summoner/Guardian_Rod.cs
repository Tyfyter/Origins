using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.CrossMod;
using Origins.Dev;
using Origins.Items.Weapons.Magic;
using Origins.Items.Weapons.Summoner;
using Origins.Items.Weapons.Summoner.Minions;
using Origins.NPCs.Felnum;
using PegasusLib;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Summoner {
	public class Guardian_Rod : ModItem {
		public override void SetStaticDefaults() {
			ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // This lets the player target anywhere on the whole screen while using a controller
			ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
			Origins.DamageBonusScale[Type] = 1.5f;
			CritType.SetCritType<Felnum_Crit_Type>(Type);
			OriginsSets.Items.FelnumItem[Type] = true;
		}
		public override void SetDefaults() {
			Item.damage = 27;
			Item.DamageType = DamageClass.Summon;
			Item.mana = 10;
			Item.shootSpeed = 9f;
			Item.knockBack = 1f;
			Item.width = 24;
			Item.height = 38;
			Item.useTime = 24;
			Item.useAnimation = 24;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.value = Item.sellPrice(gold: 1, silver: 50);
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.Item44;
			Item.buffType = Guardian_Rod_Buff.ID;
			Item.shoot = Friendly_Guardian.ID;
			Item.noMelee = true;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			player.AddBuff(Item.buffType, 2);
			Projectile projectile = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI, player.itemAnimation);
			projectile.originalDamage = Item.damage;
			return false;
		}
	}
}
namespace Origins.Buffs {
	public class Guardian_Rod_Buff : MinionBuff {
		public override string Texture => "Origins/Buffs/Felnum_Guardian_Buff";
		public static int ID { get; private set; }
		public override IEnumerable<int> ProjectileTypes() => [
			Friendly_Guardian.ID
		];
	}
}

namespace Origins.Items.Weapons.Summoner.Minions {
	public class Friendly_Guardian : ModProjectile {
		AutoLoadingAsset<Texture2D> glowTexture = typeof(Friendly_Guardian).GetDefaultTMLName() + "_Glow";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			// Sets the amount of frames this minion has on its spritesheet
			// This is necessary for right-click targeting
			ProjectileID.Sets.MinionTargettingFeature[Type] = true;
			Main.projFrames[Type] = 4;

			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[Type] = true;
			// This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned
			ProjectileID.Sets.MinionSacrificable[Type] = true;
			OriginsSets.Projectiles.UsesTypeSpecificMinionPos[Type] = true;
			ID = Type;
		}

		public override void SetDefaults() {
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
			Projectile.netImportant = true;
		}
		public override bool? CanCutTiles() => false;
		public override bool MinionContactDamage() => true;
		SlotId soundSlot;
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			// This is the "active check", makes sure the minion is alive while the player is alive, and despawns if not
			if (player.dead || !player.active) {
				player.ClearBuff(Guardian_Rod_Buff.ID);
			}
			if (player.HasBuff(Guardian_Rod_Buff.ID)) {
				Projectile.timeLeft = 2;
			}
			Projectile.damage = (int)player.GetTotalDamage(Projectile.DamageType).Scale(1.5f).ApplyTo(Projectile.originalDamage);

			#region General behavior
			Vector2 idlePosition = player.Top;
			idlePosition.X -= (48f + Projectile.minionPos * 32) * player.direction;

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
				float speed = 12f;
				float inertia = 32f;
				const int charge_time = 9 * 3;
				Vector2 vectorToTargetPosition = targetCenter - Projectile.Center;
				Projectile.spriteDirection = Math.Sign(vectorToTargetPosition.X);
				float dist = vectorToTargetPosition.Length();
				vectorToTargetPosition /= dist;
				const float hover_range = 16 * 13;
				if (dist < hover_range - 32) {
					speed *= -1;
				} else if (dist < hover_range + 32) {
					speed = 0;
					Projectile.velocity += (dist - hover_range) * vectorToTargetPosition * 0.01f * -Vector2.Dot(Projectile.velocity.SafeNormalize(default), vectorToTargetPosition);
					if (Projectile.velocity.LengthSquared() < 0.05f) {
						// If there is a case where it's not moving at all, give it a little "poke"
						Projectile.velocity += vectorToTargetPosition.RotatedBy(Main.rand.NextBool() ? -MathHelper.PiOver2 : MathHelper.PiOver2) * 2;
					}
				}

				if (++Projectile.ai[0] > 90 - charge_time) {
					float chargeFactor = (90 - Projectile.ai[0]) / charge_time;
					GeometryUtils.AngularSmoothing(ref Projectile.rotation, vectorToTargetPosition.ToRotation(), (chargeFactor + 0.05f) * 0.1f);
					speed *= chargeFactor;
					Projectile.velocity *= (chargeFactor + 149) / 150;
					float diameter = Projectile.width * 0.75f;
					Vector2 offset = Main.rand.NextVector2CircularEdge(diameter, diameter) * Main.rand.NextFloat(0.9f, 1f);
					Dust dust = Dust.NewDustPerfect(
						Projectile.Center - offset,
						DustID.Electric,
						offset * 0.125f
					);
					dust.scale -= chargeFactor;
					dust.velocity += Projectile.velocity;
					dust.noGravity = true;
					dust.shader = GameShaders.Armor.GetSecondaryShader(player.cMinion, player);
					if (SoundEngine.TryGetActiveSound(soundSlot, out ActiveSound sound)) {
						sound.Pitch = 1 - chargeFactor * 1f;
						sound.Position = Projectile.Center;
					} else {
						soundSlot = SoundEngine.PlaySound(Origins.Sounds.LightningCharging.WithVolumeScale(0.75f), Projectile.Center, soundInstance => Projectile.active);
					}
					if (Projectile.ai[0] > 90) {
						Projectile.ai[0] = 0;
						if (Main.netMode != NetmodeID.MultiplayerClient) {
							Projectile.NewProjectile(
								Projectile.GetSource_FromAI(),
								Projectile.Center,
								GeometryUtils.Vec2FromPolar(8, Projectile.rotation),
								ModContent.ProjectileType<Friendly_Guardian_P>(),
								Projectile.damage,
								Projectile.knockBack
							);
						}
						SoundEngine.PlaySound(Main.rand.Next(Origins.Sounds.LightningSounds).WithVolumeScale(0.5f) with { MaxInstances = 7 }, Projectile.Center);
						if (SoundEngine.TryGetActiveSound(soundSlot, out sound)) sound.Stop();
					}
				} else {
					Projectile.rotation = vectorToTargetPosition.ToRotation();
					if (SoundEngine.TryGetActiveSound(soundSlot, out ActiveSound sound)) sound.Stop();
				}
				vectorToTargetPosition *= speed;
				Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;
			} else {
				if (SoundEngine.TryGetActiveSound(soundSlot, out ActiveSound sound)) sound.Stop();
				float speed = 6f;
				float inertia = 24f;
				if (distanceToIdlePosition > 600f) {
					speed = 16f;
					inertia = 12f;
				}
				if (distanceToIdlePosition > 12f) {
					// The immediate range around the player (when it passively floats about)

					// This is a simple movement formula using the two parameters and its desired direction to create a "homing" movement
					vectorToIdlePosition.Normalize();
					vectorToIdlePosition *= speed;
					Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToIdlePosition) / inertia;
					if (distanceToIdlePosition > 24f) Projectile.rotation = Projectile.velocity.ToRotation();
				} else {
					if (Projectile.velocity.LengthSquared() < 0.01f) {
						// If there is a case where it's not moving at all, give it a little "poke"
						Projectile.velocity.Y =Main.rand.NextBool().ToDirectionInt() * 0.15f;
					} else {
						Projectile.velocity *= 0.97f;
					}
					Projectile.rotation = MathHelper.PiOver2 - MathHelper.PiOver2 * player.direction;
					Projectile.spriteDirection = player.direction;
				}
			}

			if (++Projectile.frameCounter >= 4) {
				Projectile.frameCounter = 0;
				if (++Projectile.frame >= Main.projFrames[Type]) Projectile.frame = 0;
			}
			Vector2 nextVel = Collision.TileCollision(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height, true, true);
			if (nextVel.X != Projectile.velocity.X) Projectile.velocity.X *= -0.7f;
			if (nextVel.Y != Projectile.velocity.Y) Projectile.velocity.Y *= -0.7f;
		}
		public override bool PreDraw(ref Color lightColor) {
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			SpriteEffects spriteEffects = SpriteEffects.FlipHorizontally;
			float rotation = Projectile.rotation;
			if (Projectile.spriteDirection != -1) {
				spriteEffects = SpriteEffects.None;
			} else {
				rotation += MathHelper.Pi;
			}
			Rectangle frame = texture.Frame(verticalFrames: Main.projFrames[Type], frameY: Projectile.frame);
			Vector2 origin = new Vector2(41, 29).Apply(spriteEffects, frame.Size());
			Main.EntitySpriteDraw(
				texture,
				Projectile.Center - Main.screenPosition,
				frame,
				lightColor,
				rotation,
				origin,
				Projectile.scale,
				spriteEffects
			);
			Main.EntitySpriteDraw(
				glowTexture,
				Projectile.Center - Main.screenPosition,
				frame,
				Color.White * (lightColor.A / 255f),
				rotation,
				origin,
				Projectile.scale,
				spriteEffects
			);
			return false;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Static_Shock_Debuff.Inflict(target, 60);
		}
	}
	public class Friendly_Guardian_P : Magnus_P {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ProjectileID.Sets.MinionShot[Type] = true;
		}
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.DamageType = DamageClass.Summon;
			startupDelay = 0;
		}
		public override bool? CanHitNPC(NPC target) {
			if (Felnum_Guardian.FriendlyNPCTypes.Contains(target.type)) return false;
			return null;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Static_Shock_Debuff.Inflict(target, Main.rand.Next(120, 210));
			int index = Math.Min((int)++Projectile.ai[1], Projectile.oldPos.Length);
			Projectile.oldPos[^index] = Projectile.Center + Projectile.velocity;
			Projectile.oldRot[^index] = Projectile.velocity.ToRotation();
			StopMovement();
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			Static_Shock_Debuff.Inflict(target, Main.rand.Next(240, 300));
		}
		public override bool PreDraw(ref Color lightColor) {
			Player owner = Main.player[Projectile.owner];
			if (owner.cMinion != 0) {
				Origins.shaderOroboros.Capture();
				base.PreDraw(ref lightColor);
				Origins.shaderOroboros.Stack(GameShaders.Armor.GetSecondaryShader(owner.cMinion, owner), Projectile);
				Origins.shaderOroboros.Release();
			} else {
				base.PreDraw(ref lightColor);
			}
			return false;
		}
	}
}
