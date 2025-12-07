using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Origins.Dusts {
	public class Spark_Dust : ModDust {
		public override string Texture => "Terraria/Images/Dust";
		public override void OnSpawn(Dust dust) {
			dust.frame.X = 60;
			dust.scale *= 0.7f;
		}
		public override bool Update(Dust dust) {
			if (!dust.noGravity) dust.velocity.Y += 0.05f;
			Vector4 slopeCollision = Collision.SlopeCollision(dust.position, dust.velocity, 0, 0);
			Vector2 position = slopeCollision.XY();
			Vector2 velocity = slopeCollision.ZW();
			velocity = Collision.TileCollision(position, velocity, 0, 0);
			if (velocity != dust.velocity || position != dust.position) {
				dust.scale *= 0.9f;
				dust.velocity *= 0f;
			}
			dust.position += dust.velocity;
			if (!dust.noLight && !dust.noLightEmittence) {
				float brightness = dust.scale * 1.4f;
				Min(ref brightness, 0.6f);
				Lighting.AddLight((int)(dust.position.X / 16f), (int)(dust.position.Y / 16f), brightness, brightness * 0.65f, brightness * 0.4f);
			}
			dust.velocity.X *= 0.99f;
			dust.rotation += dust.velocity.X * 0.5f;
			if (dust.fadeIn > 0f && dust.fadeIn < 100f) {
				dust.scale += 0.03f;
				if (dust.scale > dust.fadeIn) {
					dust.fadeIn = 0f;
				}
			}
			if (dust.noGravity) {
				dust.velocity *= 0.92f;
				if (dust.fadeIn == 0f) dust.scale -= 0.04f;
			}
			if (dust.position.Y > Main.screenPosition.Y + Main.screenHeight) dust.active = false;
			return false;
		}
		public override bool MidUpdate(Dust dust) {
			return true;
		}
		public override Color? GetAlpha(Dust dust, Color lightColor) => Color.Lerp(Color.White, dust.color, dust.color.A / 255f) with { A = 25 };
	}
	public class _Spark_Dust : ModDust{
		public override bool Update(Dust dust) {
			dust.rotation = dust.velocity.ToRotation() - MathHelper.PiOver2;
			dust.scale *= 0.80f;
			dust.position += dust.velocity;
			dust.velocity *= 0.925f;
			if(dust.scale <= 0.01f)
				dust.active = false;
			return false;
		}
		public override bool PreDraw(Dust dust) {
			Main.spriteBatch.Draw(Texture2D.Value,dust.position - Main.screenPosition,null,dust.color,dust.rotation,Texture2D.Size() / 2,new Vector2(.05f,dust.scale),Microsoft.Xna.Framework.Graphics.SpriteEffects.None,0f);
			return false;
		}

	}
}
