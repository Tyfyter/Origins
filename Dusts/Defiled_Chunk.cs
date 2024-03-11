using Terraria;
using Terraria.ModLoader;

namespace Origins.Dusts {
	public class Defiled_Chunk : ModDust {
		public override void OnSpawn(Dust dust) {
			dust.noGravity = false;
			dust.noLight = true;
			//dust.scale *= 1.5f;
		}
		public override bool Update(Dust dust) {
			dust.position += dust.velocity;
			dust.rotation += dust.velocity.X * 0.15f;
			dust.scale *= 0.99f;
			if (dust.scale < 0.5f) {
				dust.active = false;
			}
			return false;
		}
	}
}