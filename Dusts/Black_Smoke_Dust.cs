using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Dusts {
	public class Black_Smoke_Dust : ModDust {
		public override string Texture => "Terraria/Images/Dust";
		public override void OnSpawn(Dust dust) {
			dust.frame.X = 10 * DustID.Smoke;
			dust.scale *= 1.1f;
			if (dust.color == default) dust.color = Color.White;
			dust.color = dust.color.MultiplyRGBA(Color.DimGray);
		}
		public override bool Update(Dust dust) {
			if (!dust.noGravity) dust.velocity.Y += 0.05f;
			Vector4 slopeCollision = Collision.SlopeCollision(dust.position, dust.velocity, 0, 0);
			Vector2 position = slopeCollision.XY();
			Vector2 velocity = slopeCollision.ZW();
			velocity = Collision.TileCollision(position, velocity, 0, 0);
			if (velocity != dust.velocity || position != dust.position) {
				dust.scale *= 0.9f;
				dust.velocity *= 0f;
				dust.scale -= 0.001f;
			}
			dust.position += dust.velocity;
			if (!dust.noLight && !dust.noLightEmittence) {
				float brightness = dust.scale * 1.4f;
				Min(ref brightness, 0.6f);
				Lighting.AddLight((int)(dust.position.X / 16f), (int)(dust.position.Y / 16f), brightness, brightness * 0.65f, brightness * 0.4f);
			}
			dust.velocity *= 0.97f;
			dust.rotation += dust.velocity.X * 0.25f;
			if (dust.fadeIn > 0f && dust.fadeIn < 100f) {
				dust.scale += 0.03f;
				if (dust.scale > dust.fadeIn) {
					dust.fadeIn = 0f;
				}
			}
			if (dust.noGravity) {
				dust.velocity *= 0.92f;
				if (dust.fadeIn == 0f) dust.scale -= 0.04f;
			}
			if (dust.scale <= 0) dust.active = false;
			if (dust.position.Y > Main.screenPosition.Y + Main.screenHeight) dust.active = false;
			return false;
		}
		public override bool MidUpdate(Dust dust) {
			return true;
		}
		public override bool PreDraw(Dust dust) {
			Color newColor = Lighting.GetColor((int)dust.position.X / 16, (int)dust.position.Y / 16).MultiplyRGBA(dust.color);
			Main.spriteBatch.Draw(Texture2D.Value, dust.position - Main.screenPosition, dust.frame, newColor, dust.rotation, new Vector2(4f, 4f), dust.scale, SpriteEffects.None, 0f);
			return false;
		}
	}
}
