using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace Origins.Core {
	[Obsolete("Not functional yet", true)]
	public class TilesOverEntities() : Overlay(EffectPriority.High, RenderLayers.Entities), ILoadable {
		public bool IsLoadingEnabled(Mod mod) => false;
		public static TileBatch TileBatch { get; private set; }
		public RenderTarget2D tileTarget;
		public override void Activate(Vector2 position, params object[] args) {
			Opacity = 1;
			Mode = OverlayMode.Active;
		}

		public override void Deactivate(params object[] args) {
		}

		public override void Draw(SpriteBatch spriteBatch) {
			if (Main.gameMenu) return;
			spriteBatch.Draw(tileTarget, Main.sceneTile2Pos - Main.screenPosition, Color.White);
		}

		public override bool IsVisible() => true;

		public void Load(Mod mod) {
			Overlays.Scene[GetType().FullName] = this;
			Main.QueueMainThreadAction(() => {
				TileBatch = new(Main.graphics.GraphicsDevice);
				SetupRenderTargets();
			});
			Main.OnResolutionChanged += Resize;
			Overlays.Scene.Activate(GetType().FullName, default);
			On_Main.RenderTiles2 += On_Main_RenderTiles2;
		}

		void On_Main_RenderTiles2(On_Main.orig_RenderTiles2 orig, Main self) {
			Main.graphics.GraphicsDevice.SetRenderTarget(tileTarget);
			Main.graphics.GraphicsDevice.Clear(Color.Transparent);
			Main.graphics.GraphicsDevice.SetRenderTarget(null);
			TileBatch.Begin();
			orig(self);
			Main.graphics.GraphicsDevice.SetRenderTarget(tileTarget);
			TileBatch.End();
		}

		public void Unload() {
			Main.QueueMainThreadAction(() => {
				TileBatch.Dispose();
				TileBatch = null;
				tileTarget.Dispose();
			});
			Main.OnResolutionChanged -= Resize;
		}

		public override void Update(GameTime gameTime) {
			if (Overlays.Scene[GetType().FullName].Mode != OverlayMode.Active) {
				Overlays.Scene.Activate(GetType().FullName, default);
			}
		}
		public void Resize(Vector2 _) {
			if (Main.dedServ) return;
			tileTarget.Dispose();
			SetupRenderTargets();
		}
		void SetupRenderTargets() {
			if (tileTarget is not null && !tileTarget.IsDisposed) return;
			tileTarget = new RenderTarget2D(Main.instance.GraphicsDevice, Main.screenWidth + Main.offScreenRange * 2, Main.screenHeight + Main.offScreenRange * 2, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
		}
	}
}
