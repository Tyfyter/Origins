using PegasusLib;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Dusts {
	public class Rocket_Launch : ModDust {
		public override void OnSpawn(Dust dust) {
			dust.frame = Texture2D.Frame(verticalFrames: 8);
			dust.rotation = dust.velocity.ToRotation();
			if (dust.color == default) dust.color = Color.WhiteSmoke;
		}
		public override bool Update(Dust dust) {
			if (dust.fadeIn.Warmup(3)) {
				dust.fadeIn = 0;
				dust.frame.Y += dust.frame.Height;
				if (dust.frame.Y / dust.frame.Height >= 8) dust.active = false;
			}
			dust.rotation = dust.velocity.ToRotation();
			return false;
		}
		public override bool MidUpdate(Dust dust) {
			return true;
		}
		public override bool PreDraw(Dust dust) {
			Vector2 visualPos = dust.position + GeometryUtils.Vec2FromPolar(dust.frame.Width * dust.scale * 0.5f, dust.rotation);
			Main.spriteBatch.Draw(
				Texture2D.Value,
				dust.position - Main.screenPosition,
				dust.frame,
				dust.color.MultiplyRGBA(Lighting.GetColor((int)visualPos.X / 16, (int)visualPos.Y / 16)),
				dust.rotation,
				dust.frame.Size() * new Vector2(1, 0.5f),
				dust.scale,
				0,
			0);
			return false;
		}
	}
}