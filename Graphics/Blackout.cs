using Origins.Misc;
using PegasusLib;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Terraria.Graphics.Light;
using Terraria.ModLoader;
using Terraria;
using ReLogic.Threading;

namespace Origins.Graphics {
	public class BlackoutSystem : ModSystem {
		readonly List<IBlackout> blackouts = [];
		public static void Add(IBlackout blackout) => ModContent.GetInstance<BlackoutSystem>().blackouts.Add(blackout);
		public override void OnWorldUnload() {
			blackouts.Clear();
		}
		public override void Unload() {
			lightingEngine = null;
			workingProcessedArea = null;
			lightMap = null;
		}
		readonly FastFieldInfo<LightingEngine, LightMap> _workingLightMap = new("_workingLightMap", BindingFlags.NonPublic);
		readonly FastFieldInfo<LightingEngine, Rectangle> _workingProcessedArea = new("_workingProcessedArea", BindingFlags.NonPublic);
		readonly FastStaticFieldInfo<Lighting, ILightingEngine> _activeEngine = new("_activeEngine", BindingFlags.NonPublic);

		static FrameCachedValue<LightingEngine> lightingEngine = new(() => ModContent.GetInstance<BlackoutSystem>()._activeEngine.Value as LightingEngine);
		static FrameCachedValue<Rectangle> workingProcessedArea = new(() => ModContent.GetInstance<BlackoutSystem>()._workingProcessedArea.GetValue(lightingEngine.GetValue()));
		static FrameCachedValue<LightMap> lightMap = new(() => ModContent.GetInstance<BlackoutSystem>()._workingLightMap.GetValue(lightingEngine.GetValue()));
		public static void Blackout(int x, int y, int halfWidth, int halfHeight, int radius) {
			if (lightingEngine.Value is not null) {
				Rectangle workingProcessedArea = BlackoutSystem.workingProcessedArea.Value;
				LightMap lightMap = BlackoutSystem.lightMap.Value;
				x -= workingProcessedArea.X;
				y -= workingProcessedArea.Y;
				radius *= radius;
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				static float LenSQ(int x, int y) {
					return x * x + y * y;
				}
				FastParallel.For(-halfWidth, halfWidth + 1, (min, max, _) => {
					for (int i = min; i < max; i++) {
						int lightX = i + x;
						if (lightX < 0 || lightX >= lightMap.Width) continue;
						for (int j = -halfHeight; j <= halfHeight; j++) {
							int lightY = j + y;
							if (lightY < 0 || lightY >= lightMap.Height) continue;
							if (LenSQ(lightX - x, lightY - y) <= radius) {
								lightMap.SetMaskAt(lightX, lightY, LightMaskMode.Solid);
								lightMap[lightX, lightY] = Vector3.Zero;
							}
						}
					}
				});
			}
		}
		public override void ModifyLightingBrightness(ref float scale) {
			for (int index = blackouts.Count - 1; index >= 0; index--) {
				IBlackout flick = blackouts[index];
				if (flick.Finished) {
					blackouts.RemoveAt(index);
					continue;
				}
				flick.Update();
				if (flick.Finished) blackouts.RemoveAt(index);
			}
		}
	}
	public interface IBlackout {
		bool Finished { get; }
		void Update();
	}
}
