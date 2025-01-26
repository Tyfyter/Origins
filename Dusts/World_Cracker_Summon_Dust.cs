using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using PegasusLib;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Origins.Dusts {
	public class World_Cracker_Summon_Dust : ModDust {
		public override string Texture => typeof(Flare_Dust).GetDefaultTMLName();
		public override bool Update(Dust dust) {
			dust.fadeIn--;
			if (!dust.noLight && !dust.noLightEmittence) {
				float scale = dust.scale;
				if (scale > 1f) scale = 1f;
				Lighting.AddLight(dust.position, dust.color.ToVector3() * scale);
			}
			if (dust.noGravity) {
				if (dust.scale < 0.7f) {
					dust.velocity *= 1.075f;
				} else if (Main.rand.NextBool(2)) {
					dust.velocity = dust.velocity.RotatedBy((1 + Main.rand.NextFloat(1)) * Main.rand.NextBool().ToDirectionInt()) * 0.95f;
				} else {
					dust.velocity = dust.velocity.RotatedByRandom(0.02f) * 1.05f;
				}
				dust.scale -= 0.03f;
			} else {
				dust.scale += 0.005f;
				dust.alpha++;
				dust.scale *= 1 - dust.alpha * 0.001f;
				dust.velocity *= 0.9f;
				dust.velocity.X += Main.rand.Next(-10, 11) * 0.02f;
				dust.velocity.Y += Main.rand.Next(-10, 11) * 0.02f;
				if (Main.rand.NextBool(5)) {
					Dust newDust = Dust.NewDustDirect(dust.position, 4, 4, dust.type);
					newDust.noGravity = true;
					newDust.scale = dust.scale * 2.5f;
					newDust.color = Color.DeepSkyBlue;
				}
			}
			if (dust.scale > 10f || dust.scale <= 0) {
				dust.active = false;
			}
			dust.position += dust.velocity;
			return false;
		}
		public override bool MidUpdate(Dust dust) {
			return true;
		}
		public override Color? GetAlpha(Dust dust, Color lightColor) {
			return dust.color.MultiplyRGB(lightColor) with { A = 25 };
		}
		public override bool PreDraw(Dust dust) {
			Color newColor = Lighting.GetColor((int)dust.position.X / 16, (int)dust.position.Y / 16);
			Main.spriteBatch.Draw(
				Texture2D.Value,
				dust.position - Main.screenPosition,
				dust.frame,
				dust.GetColor(newColor),
				dust.rotation,
				dust.frame.Size() * 0.5f,
				dust.scale,
				SpriteEffects.None,
			0f);
			return false;
		}
	}
}