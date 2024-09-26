using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.Graphics.Shaders;
using Terraria.ID;

namespace Origins.Graphics {
	internal class ShaderLayerTargetHandler : IUnloadable {
		internal RenderTarget2D renderTarget;
		internal RenderTarget2D oldRenderTarget;
		SpriteBatchState spriteBatchState;
		SpriteBatch spriteBatch;
		bool capturing = false;
		bool spriteBatchWasRunning = false;
		public bool Capturing {
			get => capturing;
			private set {
				if (value == capturing) return;
				if (value) {
					Main.OnPostDraw += Reset;
				} else {
					Main.OnPostDraw -= Reset;
				}
				capturing = value;
			}
		}
		public void Capture(SpriteBatch spriteBatch = null) {
			if (Main.dedServ) return;
			Capturing = true;
			this.spriteBatch = spriteBatch ??= Main.spriteBatch;
			if (SpriteBatchMethods.beginCalled.GetValue(this.spriteBatch)) {
				spriteBatchWasRunning = true;
				spriteBatchState = this.spriteBatch.GetState();
				this.spriteBatch.Restart(spriteBatchState, SpriteSortMode.Immediate);
			} else {
				spriteBatchWasRunning = false;
				spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
				spriteBatchState = this.spriteBatch.GetState();
			}
			Main.graphics.GraphicsDevice.SetRenderTarget(renderTarget);
			Main.graphics.GraphicsDevice.Clear(Color.Transparent);
		}
		public void Stack(ArmorShaderData shader, Entity entity = null) {
			if (Main.dedServ) return;
			Utils.Swap(ref renderTarget, ref oldRenderTarget);
			Main.graphics.GraphicsDevice.SetRenderTarget(renderTarget);
			Main.graphics.GraphicsDevice.Clear(Color.Transparent);
			spriteBatch.Restart(spriteBatchState, transformMatrix: Matrix.Identity);
			DrawData data = new(oldRenderTarget, Vector2.Zero, null, Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None);
			shader.Apply(entity, data);
			data.Draw(spriteBatch);
		}
		public void Release() {
			if (Main.dedServ) return;
			Capturing = false;
			spriteBatch.Restart(spriteBatchState, transformMatrix: Matrix.Identity);
			RenderTargetUsage renderTargetUsage = Origins.currentScreenTarget?.RenderTargetUsage ?? Main.graphics.GraphicsDevice.PresentationParameters.RenderTargetUsage;
			try {
				if (Origins.currentScreenTarget is not null) {
					GraphicsMethods.SetRenderTargetUsage(Origins.currentScreenTarget, RenderTargetUsage.PreserveContents);
				} else {
					Main.graphics.GraphicsDevice.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
				}
				Main.graphics.GraphicsDevice.SetRenderTarget(Origins.currentScreenTarget);
			} finally {
				if (Origins.currentScreenTarget is not null) {
					GraphicsMethods.SetRenderTargetUsage(Origins.currentScreenTarget, renderTargetUsage);
				} else {
					Main.graphics.GraphicsDevice.PresentationParameters.RenderTargetUsage = renderTargetUsage;
				}
			}
			spriteBatch.Draw(renderTarget, Vector2.Zero, null, Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
			spriteBatch.Restart(spriteBatchState);
			if (!spriteBatchWasRunning) spriteBatch.End();
		}
		public void Reset(GameTime _) {
			if (Main.dedServ) return;
			Capturing = false;
			if (spriteBatchWasRunning) {
				spriteBatch.Restart(spriteBatchState);
			} else {
				spriteBatch.End();
			}
			Main.graphics.GraphicsDevice.SetRenderTarget(Origins.currentScreenTarget);
		}
		public ShaderLayerTargetHandler() {
			if (Main.dedServ) return;
			this.RegisterForUnload();
			Main.QueueMainThreadAction(SetupRenderTargets);
			Main.OnResolutionChanged += Resize;
		}
		public void Resize(Vector2 _) {
			if (Main.dedServ) return;
			renderTarget.Dispose();
			oldRenderTarget.Dispose();
			SetupRenderTargets();
		}
		void SetupRenderTargets() {
			renderTarget = new RenderTarget2D(Main.instance.GraphicsDevice, Main.screenWidth, Main.screenHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
			oldRenderTarget = new RenderTarget2D(Main.instance.GraphicsDevice, Main.screenWidth, Main.screenHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
		}
		public void Unload() {
			Main.QueueMainThreadAction(() => {
				renderTarget.Dispose();
				oldRenderTarget.Dispose();
			});
			Main.OnResolutionChanged -= Resize;
		}
	}
}
