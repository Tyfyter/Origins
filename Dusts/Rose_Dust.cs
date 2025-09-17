using Terraria;
using Terraria.ModLoader;

namespace Origins.Dusts {
	public class Rose_Dust : ModDust {
		public override void OnSpawn(Dust dust) {
			if (dust.color == Color.Transparent) dust.color = Color.White;
			dust.frame.Width += 2;
			dust.frame.Height += 2;
			dust.velocity.Y -= 1f;
		}
		public override bool Update(Dust dust) {
			dust.scale += 0.005f;
			dust.velocity.Y *= 0.94f;
			dust.velocity.X *= 0.94f;
			return true;
		}
		public override bool MidUpdate(Dust dust) {
			dust.velocity.Y -= 0.09f;
			return false;
		}
		public override Color? GetAlpha(Dust dust, Color lightColor) => lightColor.MultiplyRGBA(dust.color);
	}
}