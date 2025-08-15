using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using ReLogic.Content;
using Origins.Walls;
using PegasusLib.Graphics;

namespace Origins.Backgrounds {
	public class Fiberglass_Background() : Overlay(EffectPriority.VeryHigh, RenderLayers.Background) {
		public override void Activate(Vector2 position, params object[] args) { }
		public override void Deactivate(params object[] args) { }
		public override void Draw(SpriteBatch spriteBatch) {
			if (!Lighting.NotRetro) return;
			if (Fiberglass_Wall.BackgroundMaskTarget is not null) {
				Color drawColor = Color.White * Opacity;
				Texture2D farTexture = TextureAssets.Background[BackgroundTextureLoader.GetBackgroundSlot("Origins/Backgrounds/Fiberglass_Background3")].Value;
				Texture2D midTexture = TextureAssets.Background[BackgroundTextureLoader.GetBackgroundSlot("Origins/Backgrounds/Fiberglass_Background")].Value;
				Texture2D nearTexture = TextureAssets.Background[BackgroundTextureLoader.GetBackgroundSlot("Origins/Backgrounds/Fiberglass_Background_Overlay")].Value;
				float value = (float)((double)(Main.screenPosition.Y - (float)(Main.screenHeight / 2) + 200f) - Main.rockLayer * 16.0) / 300f;
				value = MathHelper.Clamp(value, 0f, 1f);
				Vector2 vector = Vector2.Zero;//!Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
				RenderTargetBinding[] oldRenderTargets = Main.graphics.GraphicsDevice.GetRenderTargets();

				Origins.shaderOroboros.Capture();
				Main.spriteBatch.Draw(
					farTexture,
					new Rectangle(0, 0, Main.screenWidth, Main.screenHeight),
					drawColor
				);
				Main.spriteBatch.Restart(Main.spriteBatch.GetState(), transformMatrix: Main.GameViewMatrix.ZoomMatrix);
				DrawLayer(midTexture, Main.screenWidth, Main.screenHeight * 2, new(Main.caveParallax * 0.5f, Main.caveParallax * 0.25f));
				DrawLayer(nearTexture, nearTexture.Width, nearTexture.Height, Vector2.One);
				void DrawLayer(Texture2D texture, int width, int height, Vector2 parallax) {
					int bgStartX = (int)(0 - Math.IEEERemainder(width + Main.screenPosition.X * parallax.X, width) - (width / 2));
					int bgStartY = (int)(0 - Math.IEEERemainder(height + Main.screenPosition.Y * parallax.Y, height) - (height / 2));
					Rectangle midPosition = new(bgStartX, 0, width, height);
					Rectangle midFrame = new(0, 0, texture.Width, texture.Height);
					for (int i = 0; i < (Main.screenWidth / (float)width + 1); i++) {
						midPosition.X = bgStartX + width * i;
						for (int j = 0; j < (Main.screenHeight / (float)height + 1); j++) {
							midPosition.Y = bgStartY + height * j;
							Main.spriteBatch.Draw(
								texture,
								midPosition,
								midFrame,
								drawColor
							);
						}
					}
				}
				Main.graphics.GraphicsDevice.Textures[1] = Fiberglass_Wall.BackgroundMaskTarget;
				Matrix matrix = Matrix.Identity;
				if (OriginsModIntegrations.FancyLighting is not null) {
					matrix = Matrix.Invert(Main.GameViewMatrix.ZoomMatrix);
				}
				Fiberglass_Wall.MaskShader.Shader.Parameters["uImageMatrix1"]?.SetValue(matrix);
				Fiberglass_Wall.MaskShader.Shader.Parameters["uImageSize1"]?.SetValue(new Vector2(Fiberglass_Wall.BackgroundMaskTarget.Width, Fiberglass_Wall.BackgroundMaskTarget.Height));
				Fiberglass_Wall.MaskShader.Shader.Parameters["uOffset"].SetValue(Main.sceneWallPos - Main.screenPosition);
				Fiberglass_Wall.MaskShader.Shader.Parameters["uTargetPosition"].SetValue(new Vector2(Main.offScreenRange));
				Origins.shaderOroboros.Stack(Fiberglass_Wall.MaskShader, Main.GameViewMatrix.EffectMatrix);
				Origins.shaderOroboros.Release();
			}
		}
		public override bool IsVisible() => Fiberglass_Wall.AnyWallsVisible;
		public override void Update(GameTime gameTime) {
			if (!Lighting.NotRetro) return;
			Opacity = Fiberglass_Wall.AnyWallsVisible.ToInt();
		}
	}
}