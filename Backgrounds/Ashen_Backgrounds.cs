using Microsoft.Xna.Framework.Graphics;
using PegasusLib.Graphics;
using System.Linq;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace Origins.Backgrounds {
	public class Ashen_Surface_Background : ModSurfaceBackgroundStyle {
		public override int ChooseFarTexture() {
			return BackgroundTextureLoader.GetBackgroundSlot("Origins/Backgrounds/Ashen_Background3");
		}

		public override int ChooseMiddleTexture() {
			return BackgroundTextureLoader.GetBackgroundSlot("Origins/Backgrounds/Ashen_Background2");
		}

		public override int ChooseCloseTexture(ref float scale, ref double parallax, ref float a, ref float b) {
			return BackgroundTextureLoader.GetBackgroundSlot("Origins/Backgrounds/Ashen_Background");
		}

		public override void ModifyFarFades(float[] fades, float transitionSpeed) {
			for (int i = 0; i < fades.Length; i++) MathUtils.LinearSmoothing(ref fades[i], (i == Slot).ToInt(), transitionSpeed);
		}
	}
	public class Ashen_Sky : CustomSky, ILoadable, IBrokenContent {
		readonly AutoLoadingTexture[] clouds = Enumerable.Repeat(1, 3).Select<int, AutoLoadingTexture>((o, i) => "Origins/Backgrounds/Ashen_Background_Clouds" + (o + i)).ToArray();
		bool isActive;
		Vector2 position;
		string IBrokenContent.BrokenReason => "Temp Y parallax behavior, instead use a position chosen in worldgen";
		public override void Activate(Vector2 position, params object[] args) {
			if (isActive.TrySet(true)) Opacity = 0;
			this.position = position;
		}
		public override void Deactivate(params object[] args) {
			isActive = false;
		}
		public override bool IsActive() => Opacity > 0;
		public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth) {
			Vector2 pos = default;
			double space = Main.worldSurface * 0.35 * 16;
			double surface = position.Y;
			float parallaxYBase = (float)Utils.GetLerpValue(surface, space, Main.screenPosition.Y, false);
			Color color = Main.ColorOfTheSkies * Opacity;
			for (int i = clouds.Length - 1; i >= 0; i--) {
				Vector2 size = clouds[i].Value.Size();
				float parallaxX = 0.10f * (i + 1);
				float parallaxY = float.Lerp(parallaxYBase, 0.5f, i / 6f);
				pos.Y = parallaxY * Main.screenHeight - size.Y * 0.5f;
				for (pos.X = -(Main.screenPosition.X * parallaxX + position.X / (1 + i)) % size.X; pos.X < Main.screenWidth; pos.X += size.X) {
					spriteBatch.Draw(clouds[i], pos, color);
				}
			}
		}
		public override void Reset() { }
		public override Color OnTileColor(Color inColor) => Color.Lerp(inColor, inColor.MultiplyRGB(Color.DarkGray), Opacity);
		public override void Update(GameTime gameTime) {
			MathUtils.LinearSmoothing(ref Opacity, isActive.ToInt(), 1f / 60);
			position.X += Main.windSpeedCurrent * 5;
		}
		public void Load(Mod mod) {
			SkyManager.Instance["Origins:ZoneAshen"] = this;
		}
		public void Unload() { }
	}
	public class Ashen_Underground_Background : ModUndergroundBackgroundStyle {
		public override void FillTextureArray(int[] textureSlots) {
			for (int i = 0; i < 4; i++) {
				textureSlots[i] = ModContent.GetModBackgroundSlot(GetType().GetDefaultTMLName() + i);
			}
			textureSlots[4] = 128 + Main.hellBackStyle;
		}
	}
}