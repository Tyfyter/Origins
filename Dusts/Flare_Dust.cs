using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Origins.Dusts {
	public class Flare_Dust : ModDust {
		public override bool Update(Dust dust) {
			dust.fadeIn--;
			if (!dust.noLight && !dust.noLightEmittence) {
				float scale = dust.scale;
				if (scale > 1f) scale = 1f;
				Lighting.AddLight(dust.position, dust.color.ToVector3() * scale);
			}
			if (dust.noGravity) {
				dust.velocity *= 0.93f;
				if (dust.fadeIn == 0f) {
					dust.scale += 0.0025f;
				}
			} else {
				dust.velocity *= 0.95f;
				dust.scale -= 0.0025f;
			}
			if (WorldGen.SolidTile(Framing.GetTileSafely(dust.position)) && !dust.noGravity) {
				dust.scale *= 0.9f;
				dust.velocity *= 0.25f;
			}
			return true;
		}
		public override bool MidUpdate(Dust dust) {
			return true;
		}
		public override Color? GetAlpha(Dust dust, Color lightColor) {
			return dust.color.MultiplyRGB(lightColor) with { A = 25 };
		}
		public override bool PreDraw(Dust dust) {
			float trail = Math.Abs(dust.velocity.X) + Math.Abs(dust.velocity.Y);
			trail *= 0.3f;
			trail *= 10f;
			if (trail > 10f) trail = 10f;
			if (trail > -dust.fadeIn) trail = -dust.fadeIn;
			Vector2 origin = new(4f, 4f);
			Color color = dust.GetAlpha(Lighting.GetColor((int)(dust.position.X + 4f) / 16, (int)(dust.position.Y + 4f) / 16));
			for (int k = 0; k < trail; k++) {
				Vector2 pos = dust.position - dust.velocity * k;
				float scale = dust.scale * (1f - k / 10f);
				Main.spriteBatch.Draw(Texture2D.Value, pos - Main.screenPosition, dust.frame, color, dust.rotation, origin, scale, SpriteEffects.None, 0f);
			}
			return false;
		}
	}
}