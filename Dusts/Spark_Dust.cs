using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Drawing;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Origins.Dusts {
	public class Spark_Dust : ModDust {
		public override string Texture => "Terraria/Images/Dust";
		public override void OnSpawn(Dust dust) {
			dust.frame.X = 60;
			dust.scale *= 0.7f;
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
			}
			dust.position += dust.velocity;
			if (!dust.noLight && !dust.noLightEmittence) {
				float brightness = dust.scale * 1.4f;
				Min(ref brightness, 0.6f);
				Lighting.AddLight((int)(dust.position.X / 16f), (int)(dust.position.Y / 16f), brightness, brightness * 0.65f, brightness * 0.4f);
			}
			dust.velocity.X *= 0.99f;
			dust.rotation += dust.velocity.X * 0.5f;
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
			if (dust.position.Y > Main.screenPosition.Y + Main.screenHeight) dust.active = false;
			return false;
		}
		public override bool MidUpdate(Dust dust) {
			return true;
		}
		public override Color? GetAlpha(Dust dust, Color lightColor) => Color.Lerp(Color.White, dust.color, dust.color.A / 255f) with { A = 25 };
	}
}