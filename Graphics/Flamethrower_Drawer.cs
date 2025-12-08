using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics.Primitives;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;

namespace Origins.Graphics {
	public static class Flamethrower_Drawer {
		private readonly static VertexRectangle rect = new();
		public static void Draw(Projectile projectile, float progress, Texture2D colorMap, Color smokeColor, float[] sizes, float progressColorExponent = 1, float brightnessColorExponent = 1, float? smokeAmount = null, Func<int, float> sizeProgressOverride = null, float alphaMultiplier = 0.5f, Func<int, Color> tint = null) {
			tint ??= _ => Color.White;
			MiscShaderData shaderData = GameShaders.Misc["Origins:FireShader"];
			shaderData.UseSecondaryColor(smokeColor);
			shaderData.UseImage1(TextureAssets.Extra[ExtrasID.MagicMissileTrailErosion]);
			Main.graphics.GraphicsDevice.Textures[2] = colorMap;
			Main.graphics.GraphicsDevice.SamplerStates[2] = SamplerState.LinearClamp;
			shaderData.Shader.Parameters["uImageSize2"]?.SetValue(new Vector2(colorMap.Width, colorMap.Height));
			shaderData.Shader.Parameters["uSmokeAmount"]?.SetValue(smokeAmount ?? progress);
			shaderData.Shader.Parameters["uFadeAmount"]?.SetValue(progress);
			shaderData.Shader.Parameters["uAlphaMultiplier"]?.SetValue(alphaMultiplier);
			for (int i = 0; i < projectile.oldPos.Length; i++) {
				shaderData.UseShaderSpecificData(new(progress, projectile.oldRot[i], progressColorExponent, brightnessColorExponent));
				if (sizeProgressOverride is not null) shaderData.Shader.Parameters["uFadeAmount"]?.SetValue(sizeProgressOverride(i));
				shaderData.Apply();

				rect.Draw(projectile.oldPos[i] - Main.screenPosition, tint(i), new(sizes[i] * 2), 0, projectile.oldPos[i]);
			}
		}
	}
}
