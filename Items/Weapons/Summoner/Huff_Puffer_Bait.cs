using Origins.Dev;
using Origins.Items.Materials;
using Origins.Items.Tools;
using Origins.Items.Weapons.Summoner.Minions;
using PegasusLib;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using ThoriumMod.NPCs;

namespace Origins.Items.Weapons.Summoner {
	public class Huff_Puffer_Bait : ModItem, ICustomWikiStat {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.damage = 70;
			Item.knockBack = 1;
			Item.shoot = ModContent.ProjectileType<Huff_Puffer>();
			Item.bait = 193;
			Item.consumable = false;
		}
		public override bool? CanConsumeBait(Player player) => false;
		public override bool AltFunctionUse(Player player) => true;
	}
	public class Huff_Puffer_Bait_Player : ModPlayer {
		public override void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition) {
			if (attempt.playerFishingConditions.Bait.ModItem is Huff_Puffer_Bait) {
				itemDrop = attempt.playerFishingConditions.BaitItemType;
				sonar.Text = Language.GetOrRegister("Mods.Origins.Projectiles.Huff_Puffer.DisplayName").Value;
				sonar.Color = Color.White;
				sonar.DurationInFrames = 60;
				sonar.Velocity.Y = -7;
			}
		}
	}
	public class Huff_Puffer_Bait_Global_Projectile : GlobalProjectile {
		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) => entity.bobber;
		public override void PostAI(Projectile projectile) {
			if (projectile.ai[1] > 0) {
				Player player = Main.player[projectile.owner];
				Item item = player.GetFishingConditions().Bait;
				if (item.ModItem is not Huff_Puffer_Bait) return;
				projectile.ai[1] = ItemID.None;
				Projectile.NewProjectile(
					player.GetSource_ItemUse(item),
					projectile.Center,
					Vector2.Zero,
					item.shoot,
					player.GetWeaponDamage(item),
					player.GetWeaponKnockback(item)
				);
			}
		}
	}
	namespace Minions {
		public class Huff_Puffer : ModProjectile {
			public override void SetStaticDefaults() {
				Main.projFrames[Type] = 5;
				Hydrolantern_Force_Global.ProjectileTypes.Add(Type);
			}
			public override void SetDefaults() {
				Projectile.DamageType = DamageClass.Summon;
				Projectile.width = 18;
				Projectile.height = 18;
				Projectile.tileCollide = true;
				Projectile.friendly = false;
				Projectile.sentry = true;
				Projectile.penetrate = -1;
				Projectile.timeLeft = Projectile.SentryLifeTime;
				Projectile.aiStyle = 0;
				Projectile.timeLeft = 18000;
				Projectile.netImportant = true;
			}
			float rotationSpeed = 0;
			float Fade => Math.Min(Projectile.timeLeft / 52f, 1);
			public override void AI() {
				Projectile.velocity *= Projectile.wet ? 0.96f : 0.99f;
				if (Projectile.wet) {
					float waveFactor = MathF.Sin(++Projectile.localAI[1] * 0.01f);
					Projectile.velocity.Y += waveFactor * 0.01f;
					Projectile.rotation += rotationSpeed;
					Projectile.rotation = MathF.Atan2(MathF.Sin(Projectile.rotation), MathF.Cos(Projectile.rotation));
					rotationSpeed -= Projectile.rotation * 0.001f;
					rotationSpeed *= 0.99f;
					rotationSpeed += Projectile.velocity.X * 0.0002f;
					foreach (Player player in Main.ActivePlayers) GetMovedBy(Projectile, player);
					foreach (NPC npc in Main.ActiveNPCs) GetMovedBy(Projectile, npc);
				} else {
					Projectile.velocity.Y += 0.12f;
					Projectile.rotation += rotationSpeed;
					rotationSpeed *= 0.99f;
					rotationSpeed += Projectile.velocity.X * 0.0002f;
				}
				float fade = Fade;
				Lighting.AddLight(Projectile.Center, 1 * fade, 1 * fade, 0.2f * fade);
			}
			public override Color? GetAlpha(Color lightColor) => lightColor * Fade;
			public static void GetMovedBy(Projectile projectile, Entity entity, float speedMult = 1f) {
				if (!ProjectileID.Sets.CanDistortWater[projectile.type] || ProjectileID.Sets.NoLiquidDistortion[projectile.type]) return;
				if (!entity.wet && !Collision.WetCollision(projectile.position, projectile.width, projectile.height)) return;
				float distSQ = projectile.Center.Clamp(entity.Hitbox).DistanceSQ(projectile.Center);
				const float max_range = 16 * 6;
				if (distSQ < max_range * max_range) {
					float sizeFactor = (entity.width * entity.height) / 840f;
					projectile.velocity += entity.velocity.WithMaxLength(8) * (1 - distSQ / (max_range * max_range)) * 0.03f * speedMult * sizeFactor;
				}
			}
			public override bool OnTileCollide(Vector2 oldVelocity) {
				const float bounce = 0.4f;
				const float friction = 0.9f;
				float spin = 0;
				Vector2 newOldVelocity = Projectile.velocity;
				if (newOldVelocity.X != oldVelocity.X) {
					Projectile.velocity.X = oldVelocity.X * -bounce;
					spin -= (Projectile.velocity.Y - Projectile.velocity.Y * friction) * Math.Sign(oldVelocity.X);
					Projectile.velocity.Y = Projectile.velocity.Y * friction;
				}
				if (newOldVelocity.Y != oldVelocity.Y) {
					Projectile.velocity.Y = oldVelocity.Y * -bounce;
					spin += (Projectile.velocity.X - Projectile.velocity.X * friction) * Math.Sign(oldVelocity.Y);
					Projectile.velocity.X = Projectile.velocity.X * friction;
				}
				spin *= 0.5f;
				if (Math.Abs(spin) > 0.01f) {
					MathUtils.LinearSmoothing(ref rotationSpeed, spin, 0.2f);
				} else {
					float lowestDiff = MathHelper.TwoPi;
					float lowest = 0;
					for (int i = 0; i < 4; i++) {
						float current = GeometryUtils.AngleDif(Projectile.rotation, MathHelper.PiOver2 * i, out _);
						if (current < lowestDiff) {
							lowestDiff = current;
							lowest = MathHelper.PiOver2 * i;
						}
					}
					Vector2 mov = Vector2.Zero;
					mov.X = Math.Min(GeometryUtils.AngleDif(Projectile.rotation, lowest, out int dir), 0.2f) * dir * 2;
					Vector4 slopeCollision = Collision.SlopeCollision(Projectile.position, mov, Projectile.width, Projectile.height);
					Projectile.position = slopeCollision.XY();
					mov = slopeCollision.ZW();
					mov = Collision.TileCollision(Projectile.position, mov, Projectile.width, Projectile.height);
					Projectile.position += mov;
					rotationSpeed = mov.X * 0.05f;
				}
				return false;
			}
		}
	}
}
