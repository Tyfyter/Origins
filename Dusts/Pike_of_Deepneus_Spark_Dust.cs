using Terraria;
using Terraria.ModLoader;

namespace Origins.Dusts {
	public class Pike_of_Deepneus_Spark_Dust : ModDust {
		public override bool Update(Dust dust) {
			dust.rotation = dust.velocity.ToRotation() - MathHelper.PiOver2;
			dust.scale *= 0.80f;
			dust.position += dust.velocity;
			dust.velocity *= 0.925f;
			if (dust.scale <= 0.01f)
				dust.active = false;
			return false;
		}
		public override bool PreDraw(Dust dust) {
			Main.spriteBatch.Draw(Texture2D.Value, dust.position - Main.screenPosition, null, dust.color, dust.rotation, Texture2D.Size() / 2, new Vector2(.05f, dust.scale), Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);
			return false;
		}
	}
}
