using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Dusts {
	public class Solution_D : ModDust {
		public override bool Update(Dust dust) {
			float lightScale = dust.scale * 0.1f;
			if (lightScale > 1f) {
				lightScale = 1f;
			}
			Lighting.AddLight((int)(dust.position.X / 16f), (int)(dust.position.Y / 16f), lightScale * dust.color.R / 255f, lightScale * dust.color.G / 255f, lightScale * dust.color.B / 255f);
			return true;
		}
		public override bool MidUpdate(Dust dust) {
			return false;
		}
		public override Color? GetAlpha(Dust dust, Color lightColor) {
			return (dust.color * 0.4f) with { A = 0 };
		}
	}
}