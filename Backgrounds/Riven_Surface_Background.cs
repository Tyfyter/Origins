using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins;
using Origins.Reflection;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Origins.Backgrounds {
	public class Riven_Surface_Background : ModSurfaceBackgroundStyle {
		Asset<Texture2D> glowTexture;
		public override int ChooseFarTexture() {
			int textureSlot = BackgroundTextureLoader.GetBackgroundSlot("Origins/Backgrounds/Riven_Background3");
			glowTexture ??= ModContent.Request<Texture2D>("Origins/Backgrounds/Riven_Background3_Glow");
			for (int i = 0; i < MainReflection.bgLoops.GetValue(Main.instance); i++) {
				Main.spriteBatch.Draw(
					glowTexture.Value,
					new Vector2(MainReflection.bgStartX.GetValue(Main.instance) + MainReflection.bgWidthScaled * i, MainReflection.bgTopY.GetValue(Main.instance)),
					new Rectangle(0, 0, Main.backgroundWidth[textureSlot], Main.backgroundHeight[textureSlot]),
					Color.White,
					0f,
					default(Vector2),
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

		/*public override void FillTextureArray(int[] textureSlots) {
			textureSlots[0] = mod.GetBackgroundSlot("Backgrounds/Defiled_Background");
			textureSlots[1] = mod.GetBackgroundSlot("Backgrounds/Defiled_Background2");
			textureSlots[2] = mod.GetBackgroundSlot("Backgrounds/Defiled_Background3");
			//textureSlots[3] = mod.GetBackgroundSlot("Backgrounds/Void_Background");
		}*/
	}
}