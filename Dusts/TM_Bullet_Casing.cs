using Microsoft.Xna.Framework.Graphics;
using Origins.Gores;
using Origins.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Dusts;
public class TM_Bullet_Casing : ModDust {
	public static int ID { get; private set; }
	public override void SetStaticDefaults() {
		this.SetIDProp();
		DustsBehindTiles.Add(Type);
		EfficientDust.UpdateDustCallback[Type] = DoUpdate;
	}
	public override void OnSpawn(Dust dust) {
		dust.frame = new(0, 0, 12, 6);
		dust.fadeIn = Gore.goreTime;
	}
	public static void DoUpdate(Dust dust) {
		dust.rotation += dust.velocity.X * 0.1f;
		dust.velocity.Y += 0.4f;
		int size = (int)(Math.Min(dust.frame.Width, dust.frame.Height) * 0.9f * dust.scale);
		Vector2 halfSize = new(size * 0.5f);
		Vector4 slopeCollision = Collision.SlopeCollision(dust.position - halfSize, dust.velocity, size, size);
		dust.position = slopeCollision.XY() + halfSize;
		dust.velocity = slopeCollision.ZW();
		dust.velocity = Collision.TileCollision(dust.position - halfSize, dust.velocity, size, size);
		if (dust.velocity.Y == 0f) {
			dust.velocity.X *= 0.97f;
			if (dust.velocity.X > -0.01 && dust.velocity.X < 0.01) {
				dust.velocity.X = 0f;
			}
		}
		if (dust.fadeIn > 0) {
			dust.fadeIn -= 10;
		} else {
			dust.alpha += 1;
		}
		dust.position += dust.velocity;
		if (dust.alpha >= 255) dust.active = false;
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