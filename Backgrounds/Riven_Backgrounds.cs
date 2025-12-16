using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Reflection;
using Origins.World.BiomeData;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace Origins.Backgrounds {
	public class Riven_Surface_Background : ModSurfaceBackgroundStyle {
		Asset<Texture2D> farGlowTexture;
		Asset<Texture2D> closeGlowTexture;
		public override int ChooseFarTexture() {
			int textureSlot = BackgroundTextureLoader.GetBackgroundSlot("Origins/Backgrounds/Riven_Background3");
			farGlowTexture ??= ModContent.Request<Texture2D>("Origins/Backgrounds/Riven_Background3_Glow");
			Color baseColor = MainReflection.ColorOfSurfaceBackgroundsModified;
			Color glowColor = Color.Lerp(baseColor, Color.White, Riven_Hive.NormalGlowValue.GetValue() * 0.5f + 0.25f);
			glowColor *= Main.bgAlphaFarBackLayer[Slot];
			glowColor.A = baseColor.A;
			for (int i = 0; i < MainReflection.bgLoops.GetValue(Main.instance); i++) {
				Main.spriteBatch.Draw(
					farGlowTexture.Value,
					new Vector2(MainReflection.bgStartX.GetValue(Main.instance) + MainReflection.bgWidthScaled * i, MainReflection.bgTopY.GetValue(Main.instance)),
					new Rectangle(0, 0, Main.backgroundWidth[textureSlot], Main.backgroundHeight[textureSlot]),
					glowColor,
					0f,
					default,
					MainReflection.bgScale,
					SpriteEffects.None,
				0f);
			}
			return textureSlot;
		}

		public override int ChooseMiddleTexture() {
			return BackgroundTextureLoader.GetBackgroundSlot("Origins/Backgrounds/Riven_Background2");
		}

		public override int ChooseCloseTexture(ref float scale, ref double parallax, ref float a, ref float b) {
			return BackgroundTextureLoader.GetBackgroundSlot("Origins/Backgrounds/Riven_Background");
		}
		public override bool PreDrawCloseBackground(SpriteBatch spriteBatch) {
			closeGlowTexture ??= ModContent.Request<Texture2D>("Origins/Backgrounds/Riven_Background_Glow");
			Color baseColor = MainReflection.ColorOfSurfaceBackgroundsModified;
			Color glowColor = Color.Lerp(baseColor, Color.White, Riven_Hive.NormalGlowValue.GetValue() * 0.5f + 0.25f);
			glowColor *= Main.bgAlphaFrontLayer[Slot];
			glowColor.A = baseColor.A;

			MainReflection.bgScale = 1.25f;
			double bgParallax = 0.37;
			float a = 1800f;
			float b = 1750f;

			float bgScale = MainReflection.bgScale;
			int textureSlot = ChooseCloseTexture(ref bgScale, ref bgParallax, ref a, ref b);
			Main.instance.LoadBackground(textureSlot);
			MainReflection.bgScale = bgScale * 2f;
			MainReflection.bgWidthScaled = (int)(Main.backgroundWidth[textureSlot] * MainReflection.bgScale);
			SkyManager.Instance.DrawToDepth(Main.spriteBatch, 1f / (float)bgParallax);
			MainReflection.Instance_bgStartX = (int)(-Math.IEEERemainder(Main.screenPosition.X * bgParallax, MainReflection.bgWidthScaled) - (MainReflection.bgWidthScaled / 2));
			MainReflection.Instance_bgTopY = (int)((-(Main.screenPosition.Y + MainReflection.Instance_screenOff / 2f)) / (Main.worldSurface * 16.0) * a + b)
				+ (int)MainReflection.Instance_scAdj;
			if (Main.gameMenu) {
				MainReflection.Instance_bgTopY = 320;
			}
			MainReflection.Instance_bgLoops = Main.screenWidth / MainReflection.bgWidthScaled + 2;
			if (Main.screenPosition.Y < Main.worldSurface * 16.0 + 16.0) {
				for (int i = 0; i < MainReflection.Instance_bgLoops; i++) {
					Vector2 position = new(MainReflection.Instance_bgStartX + MainReflection.bgWidthScaled * i, MainReflection.Instance_bgTopY);
					Rectangle frame = new(0, 0, Main.backgroundWidth[textureSlot], Main.backgroundHeight[textureSlot]);
					Main.spriteBatch.Draw(
						TextureAssets.Background[textureSlot].Value,
						position,
						frame,
						MainReflection.ColorOfSurfaceBackgroundsModified,
						0f,
						default,
						MainReflection.bgScale,
						SpriteEffects.None, 
					0f);
					Main.spriteBatch.Draw(
						closeGlowTexture.Value,
						position,
						frame,
						glowColor,
						0f,
						default,
						MainReflection.bgScale,
						SpriteEffects.None, 
					0f);
				}
			}
			return false;
		}
		public override void ModifyFarFades(float[] fades, float transitionSpeed) {
			for (int i = 0; i < fades.Length; i++) {
				if (i == Slot) {
					fades[i] += transitionSpeed;
					if (fades[i] > 1f) {
						fades[i] = 1f;
					}
				} else {
					fades[i] -= transitionSpeed;
					if (fades[i] < 0f) {
						fades[i] = 0f;
					}
				}
			}
		}
	}
	public class Riven_Underground_Background : ModUndergroundBackgroundStyle {
		public override void FillTextureArray(int[] textureSlots) {
			for (int i = 0; i < 4; i++) {
				textureSlots[i] = ModContent.GetModBackgroundSlot(GetType().GetDefaultTMLName() + i);
			}
			textureSlots[4] = 128 + Main.hellBackStyle;
		}
	}
}