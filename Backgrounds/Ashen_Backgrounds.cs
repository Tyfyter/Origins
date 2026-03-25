using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using PegasusLib.Graphics;
using ReLogic.Content;
using System;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace Origins.Backgrounds {
	public class Ashen_Surface_Background : ModSurfaceBackgroundStyle {
		int? far;
		int? mid;
		int? near;
		public override int ChooseFarTexture() {
			return far ??= BackgroundTextureLoader.GetBackgroundSlot("Origins/Backgrounds/Ashen_Background3");
		}

		public override int ChooseMiddleTexture() {
			return mid ??= BackgroundTextureLoader.GetBackgroundSlot("Origins/Backgrounds/Ashen_Background2");
		}

		public override int ChooseCloseTexture(ref float scale, ref double parallax, ref float a, ref float b) {
			return near ??= BackgroundTextureLoader.GetBackgroundSlot("Origins/Backgrounds/Ashen_Background");
		}

		public override void ModifyFarFades(float[] fades, float transitionSpeed) {
			for (int i = 0; i < fades.Length; i++) MathUtils.LinearSmoothing(ref fades[i], (i == Slot).ToInt(), transitionSpeed);
		}
	}
	public class Ashen_Desert_Background : ModSurfaceBackgroundStyle {
		int? far;
		int? mid;
		int? near;
		public override int ChooseFarTexture() {
			return far ??= BackgroundTextureLoader.GetBackgroundSlot("Origins/Backgrounds/Ashen_Desert_Background3");
		}

		public override int ChooseMiddleTexture() {
			return mid ??= BackgroundTextureLoader.GetBackgroundSlot("Origins/Backgrounds/Ashen_Desert_Background2");
		}

		public override int ChooseCloseTexture(ref float scale, ref double parallax, ref float a, ref float b) {
			return near ??= BackgroundTextureLoader.GetBackgroundSlot("Origins/Backgrounds/Ashen_Desert_Background");
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
		int? fog;
		int? sky;
		public override void Activate(Vector2 position, params object[] args) {
			if (isActive.TrySet(true)) Opacity = 0;
			this.position = position;
		}
		public override void Deactivate(params object[] args) {
			isActive = false;
		}
		public override bool IsActive() => Opacity > 0;
		public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth) {
			SpriteBatchState state = spriteBatch.GetState();
			spriteBatch.Restart(state, SpriteSortMode.Immediate, samplerState: SamplerState.LinearClamp);
			Rectangle destinationRectangle = new(0, (SkyColor.bgTopY + 60) * 4, Main.screenWidth, Math.Max(Main.screenHeight, 1400));
			Min(ref destinationRectangle.Y, 0);
			destinationRectangle.Height /= 2;
			Texture2D smogOverlay = TextureAssets.Background[fog ??= BackgroundTextureLoader.GetBackgroundSlot("Origins/Backgrounds/Ashen_Background_Sky_Fog")].Value;
			MiscShaderData shaderData = GameShaders.Misc["Origins:Muddle"]
			.UseSamplerState(SamplerState.LinearClamp)
			.UseImage1(perlin);
			shaderData.Apply(null, [
				new("uOffset", new Vector2(Main.screenWidth / (float)perlin.Value.Width, destinationRectangle.Height / (float)perlin.Value.Height)),
				new("uTargetPosition", new Vector2(0, 16 / (float)destinationRectangle.Height)),
				new("uScale", 0.1f),
				new("uScaleY", 0.1f),
				new("uWorldPosition", Main.screenPosition * 0.1f + position * 0.1f - new Vector2(0, Main.GlobalTimeWrappedHourly * 5f))
			]);
			Main.spriteBatch.Draw(smogOverlay, destinationRectangle, Main.ColorOfTheSkies * Opacity);

			Vector2 pos = default;
			double space = Main.worldSurface * 0.35 * 16;
			double surface = position.Y;
			Max(ref surface, space + 60 * 16);
			float parallaxYBase = (float)Utils.GetLerpValue(surface, space, Main.screenPosition.Y, false);
			Color color = Main.ColorOfTheSkies * Opacity * Opacity;
			shaderData.Apply(null, [
				new("uOffset", new Vector2(clouds[0].Value.Width / (float)perlin.Value.Width, clouds[0].Value.Height / (float)perlin.Value.Height)),
				new("uTargetPosition", new Vector2(4) / clouds[0].Value.Size()),
				new("uScale", 0.5f),
				new("uScaleY", 0.5f)
			]);
			for (int i = clouds.Length - 1; i >= 0; i--) {
				Vector2 size = clouds[i].Value.Size();
				float parallaxX = 0.10f * (i + 1);
				float parallaxY = float.Lerp(parallaxYBase, 0.5f, i / 6f);
				pos.Y = parallaxY * Main.screenHeight - size.Y * 0.5f;
				pos.X = -(Main.screenPosition.X * parallaxX + position.X / (1 + i)) % size.X;
				while (pos.X > 0) pos.X -= clouds[i].Value.Width;
				for (; pos.X < Main.screenWidth; pos.X += size.X) {
					spriteBatch.Draw(clouds[i], pos, color);
				}
			}
			spriteBatch.Restart(state);
			SkyColor.Activate(sky ??= BackgroundTextureLoader.GetBackgroundSlot("Origins/Backgrounds/Ashen_Background_Sky"));
		}
		public override void Reset() { }
		public override Color OnTileColor(Color inColor) => Color.Lerp(inColor, inColor.MultiplyRGB(new(112, 50, 18)), Opacity);
		public override void Update(GameTime gameTime) {
			MathUtils.LinearSmoothing(ref Opacity, isActive.ToInt(), 1f / 60);
			position.X += Main.windSpeedCurrent * 5;
		}
		public void Load(Mod mod) {
			if (Main.dedServ) return;
			SkyManager.Instance["Origins:ZoneAshen"] = this;
			On_Main.DrawStarsInBackground += On_Main_DrawStarsInBackground;
			perlin = ModContent.Request<Texture2D>("Terraria/Images/Misc/Perlin");
		}
		static Asset<Texture2D> perlin;
		void On_Main_DrawStarsInBackground(On_Main.orig_DrawStarsInBackground orig, Main self, Main.SceneArea sceneArea, bool artificial) {
			if (Opacity >= 1) return;
			float graveyardIntensity = Main.GraveyardVisualIntensity;
			Main.GraveyardVisualIntensity = float.Lerp(graveyardIntensity, 1, Opacity);
			try {
				orig(self, sceneArea, artificial);
			} catch (Exception) {
				Main.GraveyardVisualIntensity = graveyardIntensity;
				throw;
			}
			Main.GraveyardVisualIntensity = graveyardIntensity;
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