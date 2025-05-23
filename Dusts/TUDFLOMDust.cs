using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Origins.Dusts {
	public class TUDFLOMDust : ModDust {
		public override void SetStaticDefaults() {
			Deprioritized_Dust.Set[Type] = -1;
		}
		public override bool Update(Dust dust) {
			if (dust.customData is TUDFLOMD_Subdust[] data) {
				bool active = false;
				for (int i = 0; i < data.Length; i++) {
					data[i].Update();
					active |= data[i].Active;
				}
				dust.active = active;
			} else {
				dust.active = false;
			}
			return false;
		}
		public override bool MidUpdate(Dust dust) {
			return true;
		}
		public override bool PreDraw(Dust dust) {
			if (dust.customData is TUDFLOMD_Subdust[] data) {
				Texture2D texture = Texture2D.Value;
				for (int i = 0; i < data.Length; i++) {
					data[i].Draw(texture);
				}
			}
			return false;
		}
	}
	public struct TUDFLOMD_Subdust(Vector2 position, Vector2 velocity, float scale, Color color) {
		Vector2 position = position, velocity = velocity;
		float scale = scale, fadeIn;
		Rectangle frame = new(0, Main.rand.Next(3) * 10, 10, 10);
		Color color = color;
		public readonly bool Active => scale > 0.1f;
		public void Update() {
			float lightScale = scale;
			if (lightScale > 1f) lightScale = 1f;
			Lighting.AddLight(position, color.ToVector3() * lightScale);
			fadeIn--;
			position += velocity;
			velocity *= 0.92f;
			if (fadeIn == 0f) {
				scale += 0.0025f;
			}
			scale -= 0.01f;
			velocity *= 0.92f;
			if (fadeIn == 0f) {
				scale -= 0.04f;
			}
		}
		public void Draw(Texture2D texture) {
			float trail = Math.Abs(velocity.X) + Math.Abs(velocity.Y);
			trail *= 0.3f;
			trail *= 10f;
			if (trail > 10f) trail = 10f;
			if (trail > -fadeIn) trail = -fadeIn;
			Vector2 origin = new(4f, 4f);
			Color color = this.color.MultiplyRGB(this.color) with { A = 25 };
			for (int k = 0; k < trail; k++) {
				Vector2 pos = position - velocity * k;
				float scale = this.scale * (1f - k / 10f);
				Main.spriteBatch.Draw(texture, pos - Main.screenPosition, frame, color, 0, origin, scale, SpriteEffects.None, 0f);
			}
		}
	}
}