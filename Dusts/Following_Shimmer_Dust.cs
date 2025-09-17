using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Dusts {
	public class Following_Shimmer_Dust : ModDust {
		public override string Texture => "Terraria/Images/Dust";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void OnSpawn(Dust dust) {
			dust.velocity.Y = Main.rand.Next(-10, 6) * 0.1f;
			dust.velocity.X *= 0.3f;
			dust.scale *= 0.7f;

			dust.frame.X += DustID.ShimmerTorch * 10 - 1000 * (DustID.ShimmerTorch / 100);
			dust.frame.Y += 30 * (DustID.ShimmerTorch / 100);
		}
		public override bool Update(Dust dust) {
			if (dust.customData is FollowingDustSettings settings) {
				dust.position += settings.Followee.velocity * settings.FollowAmount;
			}
			if (!dust.noGravity) {
				dust.velocity.Y += 0.05f;
			}
			if (!dust.noLight && !dust.noLightEmittence) {
				float lightAmount = dust.scale * 1.4f;
				if (lightAmount > 1f) lightAmount = 1f;
				Lighting.AddLight((int)(dust.position.X / 16f), (int)(dust.position.Y / 16f), 23, lightAmount);
			}
			return true;
		}
		public override bool MidUpdate(Dust dust) => dust.customData is not FollowingDustSettings settings || settings.MidUpdate;
		public override Color? GetAlpha(Dust dust, Color lightColor) => new Color(lightColor.R, lightColor.G, lightColor.B, 25);
		public record struct FollowingDustSettings(Entity Followee, float FollowAmount = 0.25f, bool MidUpdate = true);
	}
}