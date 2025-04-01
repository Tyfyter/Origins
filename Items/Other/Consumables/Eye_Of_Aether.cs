using PegasusLib;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod.NPCs;

namespace Origins.Items.Other.Consumables {
	public class Eye_Of_Aether : ModItem {
		public override void SetDefaults() {
			Item.DefaultToThrownWeapon(ModContent.ProjectileType<Eye_Of_Aether_P>(), 20, 4);
			Item.width = 16;
			Item.height = 16;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.DamageType = DamageClass.Default;
		}
	}
	public class Eye_Of_Aether_P : ModProjectile {
		public override void SetDefaults() {
			Projectile.width = 22;
			Projectile.height = 20;
			Projectile.tileCollide = false;
		}
		public override void AI() {
			const int float_up = 30;
			const int float_in_place = float_up + 60;
			const int shoot = float_in_place + 60;
			const float spin_speed = 0.5f;
			Projectile.ai[0]++;
			if (Projectile.ai[0] < float_up) {
				Projectile.velocity *= 0.93f;
				Projectile.velocity.Y -= 0.4f;
			} else if (Projectile.ai[0] < float_in_place) {
				Projectile.velocity *= 0.93f;
				Projectile.rotation += spin_speed;
			} else if (Projectile.ai[1] == 0) {
				if (ModContent.GetInstance<OriginSystem>().shimmerPosition is Vector2 shimmerPosition) {
					SoundEngine.PlaySound(SoundID.Item15.WithPitch(-1).WithPitchVarience(0) with { MaxInstances = 0 }, Projectile.Center);
					SoundEngine.PlaySound(SoundID.Item15.WithPitch(0).WithPitchVarience(0) with { MaxInstances = 0 }, Projectile.Center);
					Projectile.ai[1] = shimmerPosition.X * 16;
					Projectile.ai[2] = shimmerPosition.Y * 16;
				} else {
					SoundEngine.PlaySound(SoundID.Shatter, Projectile.Center);
					// could use some dusts or gores here
					Projectile.Kill();
				}
			} else if (Projectile.ai[0] < shoot) {
				Projectile.velocity = Vector2.Zero;
				float targetAngle = (new Vector2(Projectile.ai[1], Projectile.ai[2]) - Projectile.Center).ToRotation() - MathHelper.PiOver4 * 0.5f;
				float angle = GeometryUtils.AngleDif(Projectile.rotation, targetAngle, out int dir);
				if (dir != 1 || angle > spin_speed) Projectile.rotation += spin_speed;
				else Projectile.rotation = targetAngle;
			} else if (Projectile.timeLeft > 15) {
				Vector2 diff = new Vector2(Projectile.ai[1], Projectile.ai[2]) - Projectile.Center;
				Vector2 pos = Projectile.Center;
				float dist = diff.Length();
				const float speed = 8;
				Vector2 dir = diff * speed / dist;
				Rectangle area = new((int)Main.screenPosition.X, (int)Main.screenPosition.Y, Main.screenWidth, Main.screenHeight);
				area.Inflate(16 * 50, 16 * 50);
				while (dist > 0) {
					if (area.Contains(pos)) {
						ParticleOrchestrator.RequestParticleSpawn(clientOnly: true, ParticleOrchestraType.ChlorophyteLeafCrystalShot, new ParticleOrchestraSettings {
							PositionInWorld = pos,
							MovementVector = dir,
							UniqueInfoPiece = (byte)(0.727f * 255f)
						});
					}
					pos += dir;
					dist -= speed;
				}
				Projectile.timeLeft = 15;
			}
			if (Projectile.timeLeft < 15) {
				SoundEngine.PlaySound(SoundID.Item15.WithPitch(-1).WithPitchVarience(0) with { MaxInstances = 0 }, Projectile.Center);
				Projectile.Opacity = Projectile.timeLeft / 15f;
			}
		}
	}
}
