using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Dusts {
	public class Tintable_Torch_Dust : ModDust {
		public override string Texture => "Terraria/Images/Dust";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void OnSpawn(Dust dust) {
			dust.frame.X = 10 * DustID.WhiteTorch;
			dust.frame.Y = 10 * Main.rand.Next(3);
			dust.velocity.Y = Main.rand.Next(-10, 6) * 0.1f;
			dust.velocity.X *= 0.3f;
			dust.scale *= 0.7f;
		}
		public override bool MidUpdate(Dust dust) {
			if (!dust.noGravity) {
				dust.velocity.Y += 0.05f;
			}
			if (!dust.noLight && !dust.noLightEmittence) {
				float lightScale = dust.scale * 1.4f;
				if (lightScale > 0.6f) lightScale = 0.6f;
				lightScale /= 255f;
				Lighting.AddLight((int)(dust.position.X / 16f), (int)(dust.position.Y / 16f), lightScale * dust.color.R, lightScale * dust.color.G, lightScale * dust.color.B);
			}
			return true;
		}
		public override Color? GetAlpha(Dust dust, Color lightColor) => dust.color;
	}
}