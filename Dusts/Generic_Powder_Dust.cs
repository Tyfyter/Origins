using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Dusts {
	public class Generic_Powder_Dust : ModDust {
		public override void OnSpawn(Dust dust) {
			//dust.noGravity = true;
		}
		public override bool Update(Dust dust) {
			dust.scale += 0.005f;
			dust.velocity.Y *= 0.94f;
			dust.velocity.X *= 0.94f;
			if (!dust.noLightEmittence) {
				float lightScale = dust.scale * 0.4f;
				Lighting.AddLight(dust.position, lightScale * dust.color.ToVector3());
			}
			return true;
		}
		public override bool MidUpdate(Dust dust) {
			dust.velocity.Y -= 0.1f;
			return false;
		}
		public override Color? GetAlpha(Dust dust, Color lightColor) {
			return lightColor.MultiplyRGBA(dust.color);// with { A = 25 }
		}
	}
}