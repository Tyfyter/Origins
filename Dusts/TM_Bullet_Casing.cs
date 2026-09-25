using Microsoft.Xna.Framework.Graphics;
using Origins.Gores;
using Origins.Graphics;
using Origins.Misc;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Dusts;
public class TM_Bullet_Casing : ModDust {
	public static int ID { get; private set; }
	public override void SetStaticDefaults() {
		this.SetIDProp();
		EfficientDust.UpdateDustCallback[Type] = DoUpdate;
		EfficientDust.DebugMode = true;
	}
	public override void OnSpawn(Dust dust) {
		dust.frame = new(0, 0, 12, 6);
		dust.fadeIn = 30;
		dust.scale = 1;
	}
	public static void DoUpdate(Dust dust) {
		dust.velocity.Y += 0.4f;
		DoCollision(dust);
		float rotationSpeed = dust.customData as float? ?? 0;
		dust.rotation += rotationSpeed;
		rotationSpeed *= 0.99f;
		rotationSpeed += dust.velocity.X * 0.0002f;
		dust.customData = rotationSpeed;
		if (dust.velocity.X > -0.01f && dust.velocity.X < 0.01f) dust.velocity.X = 0f;
		if (dust.velocity.Y > -0.01f && dust.velocity.Y < 0.01f) dust.velocity.Y = 0f;
		if (dust.fadeIn > 0) {
			dust.fadeIn -= 1;
		} else {
			dust.alpha += 1;
		}
		dust.position += dust.velocity;
		if (dust.alpha >= 255) dust.active = false;
	}
	static void DoCollision(Dust dust) {
		const float bounce = 0.3f;
		const float friction = 0.9f;
		float rotationSpeed = dust.customData as float? ?? 0;
		float spin = 0;
		int size = (int)(Math.Min(dust.frame.Width, dust.frame.Height) * 0.9f * dust.scale);
		Vector2 halfSize = new(size * 0.5f);
		Vector2 oldVelocity = dust.velocity;
		Vector4 slopeCollision = Collision.SlopeCollision(dust.position - halfSize, dust.velocity, size, size);
		dust.position = slopeCollision.XY() + halfSize;
		dust.velocity = slopeCollision.ZW();
		dust.velocity = Collision.TileCollision(dust.position - halfSize, dust.velocity, size, size);
		Vector2 newOldVelocity = dust.velocity;
		bool ping = false;
		if (newOldVelocity.X != oldVelocity.X) {
			dust.velocity.X = oldVelocity.X * -bounce;
			spin -= (dust.velocity.Y - dust.velocity.Y * friction) * Math.Sign(oldVelocity.X);
			dust.velocity.Y *= friction;
			rotationSpeed *= 0.5f;
			ping |= Math.Abs(oldVelocity.X) > 4;
		}
		if (newOldVelocity.Y != oldVelocity.Y) {
			dust.velocity.Y = oldVelocity.Y * -bounce;
			spin += (dust.velocity.X - dust.velocity.X * friction) * Math.Sign(oldVelocity.Y);
			dust.velocity.X *= friction;
			rotationSpeed *= 0.5f;
			ping |= Math.Abs(oldVelocity.Y) > 4;
		}
		if (ping) SoundEngine.PlaySound(SoundID.Item15.WithPitch(0.5f), dust.position);
		spin *= 5f;
		if (newOldVelocity != oldVelocity) {
			if (Math.Abs(spin) > 0.1f) {
				MathUtils.LinearSmoothing(ref rotationSpeed, spin, 1f);
			} else {
				float lowestDiff = MathHelper.TwoPi;
				float lowest = 0;
				for (int i = 0; i < 4; i++) {
					float current = GeometryUtils.AngleDif(dust.rotation, MathHelper.PiOver2 * i, out _);
					if (i % 2 == 1) current *= 8;
					if (current < lowestDiff) {
						lowestDiff = current;
						lowest = MathHelper.PiOver2 * i;
					}
				}
				Vector2 mov = Vector2.Zero;
				mov.X = Math.Min(GeometryUtils.AngleDif(dust.rotation, lowest, out int dir), 0.2f) * dir * 0.5f;
				Vector4 slopeCollision2 = Collision.SlopeCollision(dust.position, mov, size, size);
				dust.position = slopeCollision2.XY();
				mov = slopeCollision2.ZW();
				mov = Collision.TileCollision(dust.position, mov, size, size);
				dust.position += mov;
				rotationSpeed = mov.X * 0.2f;
			}
		}
		dust.customData = rotationSpeed;
	}
	public override bool Update(Dust dust) {
		DoUpdate(dust);
		return false;
	}
	public override bool PreDraw(Dust dust) {
		Point lightPos = dust.position.ToTileCoordinates();
		Main.spriteBatch.Draw(
			Texture2D.Value,
			dust.position - Main.screenPosition,
			dust.frame,
			Lighting.GetColor(lightPos) * ((255f - dust.alpha) / 255f),
			dust.rotation,
			dust.frame.Size() * 0.5f,
			dust.scale,
			SpriteEffects.None,
		0f);
		return false;
	}
}
public class TM_Shell_Casing : TM_Bullet_Casing {
	public static new int ID { get; private set; }
	public override void OnSpawn(Dust dust) {
		base.OnSpawn(dust);
		dust.frame.Width = 16;
		dust.frame.Height = 10;
	}
}