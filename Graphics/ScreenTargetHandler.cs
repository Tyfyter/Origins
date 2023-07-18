using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Threading;
using Terraria;
using Terraria.ModLoader;

// originally StarlightRiver.Core.Systems.ScreenTargetSystem
// was told "This might help you avoid some caveats, feel free to use it" by Scalar
namespace Origins.Graphics
{
	internal class ScreenTargetHandler : ModSystem
	{
		public static List<ScreenTarget> targets = new();
		public static Semaphore targetSem = new(1, 1);

		private static int firstResizeTime = 0;

		public override void Load()
		{
			Terraria.On_Main.CheckMonoliths += RenderScreens;
			Main.OnResolutionChanged += ResizeScreens;
		}

		public override void Unload()
		{
			Terraria.On_Main.CheckMonoliths -= RenderScreens;
			Main.OnResolutionChanged -= ResizeScreens;

			Main.QueueMainThreadAction(() =>
			{
				targets.ForEach(n => n.RenderTarget.Dispose());
				targets.Clear();
				targets = null;
			});
		}

		/// <summary>
		/// Registers a new screen target and orders it into the list. Called automatically by the constructor of ScreenTarget!
		/// </summary>
		/// <param name="toAdd"></param>
		public static void AddTarget(ScreenTarget toAdd)
		{
			targetSem.WaitOne();

			targets.Add(toAdd);
			targets.Sort((a, b) => a.order.CompareTo(b.order));

			targetSem.Release();
		}

		/// <summary>
		/// Removes a screen target from the targets list. Should not normally need to be used.
		/// </summary>
		/// <param name="toRemove"></param>
		public static void RemoveTarget(ScreenTarget toRemove)
		{
			targetSem.WaitOne();

			targets.Remove(toRemove);
			targets.Sort((a, b) => a.order - b.order > 0 ? 1 : -1);

			targetSem.Release();
		}

		public static void ResizeScreens(Vector2 obj)
		{
			if (Main.gameMenu || Main.dedServ)
				return;

			targetSem.WaitOne();

			targets.ForEach(n =>
			{
				Vector2? size = obj;

				if (n.onResize != null)
					size = n.onResize(obj);

				if (size != null)
				{
					n.RenderTarget?.Dispose();
					n.RenderTarget = new RenderTarget2D(Main.instance.GraphicsDevice, (int)size?.X, (int)size?.Y, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
				}
			});

			targetSem.Release();
		}

		private void RenderScreens(Terraria.On_Main.orig_CheckMonoliths orig)
		{
			orig();

			if (Main.gameMenu || Main.dedServ)
				return;

			RenderTargetBinding[] bindings = Main.graphics.GraphicsDevice.GetRenderTargets();

			targetSem.WaitOne();

			foreach (ScreenTarget target in targets)
			{
				if (target.drawFunct is null) //allows for RTs which dont draw in the default loop, like the lighting tile buffers
					continue;

				Main.spriteBatch.Begin();
				Main.graphics.GraphicsDevice.SetRenderTarget(target.RenderTarget);
				Main.graphics.GraphicsDevice.Clear(Color.Transparent);

				if (target.activeFunct())
					target.drawFunct(Main.spriteBatch);

				Main.spriteBatch.End();
			}

			Main.graphics.GraphicsDevice.SetRenderTargets(bindings);

			targetSem.Release();
		}

		public override void PostUpdateEverything()
		{
			if (Main.gameMenu)
				firstResizeTime = 0;
			else
				firstResizeTime++;

			if (firstResizeTime == 20)
				ResizeScreens(new Vector2(Main.screenWidth, Main.screenHeight));
		}
	}
}
