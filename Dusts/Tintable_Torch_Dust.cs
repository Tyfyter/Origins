using Origins.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Dusts {
	public class Tintable_Torch_Dust : ModDust {
		public override string Texture => "Terraria/Images/Dust";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
			EfficientDust.UpdateDustCallback[Type] = EfficientDust.TintableTorch;
		}
		public override void OnSpawn(Dust dust) {
			dust.frame.X = 10 * DustID.WhiteTorch;
			dust.frame.Y = 10 * Main.rand.Next(3);
			dust.velocity.Y = Main.rand.Next(-10, 6) * 0.1f;
			dust.velocity.X *= 0.3f;
			dust.scale *= 0.7f;
		}
		public override bool Update(Dust dust) {
			EfficientDust.Torch(dust, dust.color.ToVector3());
			return false;
		}
		public override Color? GetAlpha(Dust dust, Color lightColor) => dust.color;
	}
}