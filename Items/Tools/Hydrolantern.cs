using Origins.Dev;
using PegasusLib;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Tools {
	public class Hydrolantern : ModItem, ICustomWikiStat {
		public override void SetStaticDefaults() {
			ItemID.Sets.ShimmerTransformToItem[ItemID.Glowstick] = Type;
			Item.ResearchUnlockCount = 8;
		}
		public override void SetDefaults() {
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.maxStack = 8;
			Item.shoot = ModContent.ProjectileType<Hydrolantern_Use>();
			Item.shootSpeed = 8;
			Item.noUseGraphic = true;
			Item.UseSound = SoundID.Item7;
		}
		public override bool AltFunctionUse(Player player) => true;
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.altFunctionUse == 2) {
				OriginExtensions.FadeOutOldProjectilesAtLimit([type], 1, 52);
				return false;
			}
			OriginExtensions.FadeOutOldProjectilesAtLimit([type], Item.stack, 52);
			return true;
		}
	}
	public class Hydrolantern_Use : ModProjectile {
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 5;
			Hydrolantern_Force_Global.ProjectileTypes.Add(Type);
		}
		public override void SetDefaults() {
			Projectile.aiStyle = 0;
			Projectile.width = 18;
			Projectile.height = 18;
			Projectile.timeLeft = 18000;
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
			Lighting.AddLight(Projectile.Center, 1 * fade, 1 * fade, 1 * fade);
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
	public class Hydrolantern_Force_Global : GlobalProjectile {
		public static HashSet<int> ProjectileTypes = [];
		public override void Unload() => ProjectileTypes = null;
		public override void AI(Projectile projectile) {
			foreach (Projectile other in Main.ActiveProjectiles) {
				if (projectile.whoAmI != other.whoAmI && ProjectileTypes.Contains(other.type) && ProjectileLoader.ShouldUpdatePosition(projectile)) {
					Hydrolantern_Use.GetMovedBy(other, projectile, projectile.ignoreWater ? 1f : 0.5f);
				}
			}
		}
	}
}
