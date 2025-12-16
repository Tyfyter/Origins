using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace Origins.Reflection {
	public class GraphicsMethods : ILoadable {
		public void Load(Mod mod) {
			DynamicMethod getterMethod = new($"{nameof(RenderTarget2D)}.set_{nameof(RenderTarget2D.RenderTargetUsage)}", typeof(void), [typeof(RenderTarget2D), typeof(RenderTargetUsage)], true);
			ILGenerator gen = getterMethod.GetILGenerator();

			gen.Emit(OpCodes.Ldarg_0);
			gen.Emit(OpCodes.Ldarg_1);
			gen.EmitCall(OpCodes.Call, typeof(RenderTarget2D).GetProperty(nameof(RenderTarget2D.RenderTargetUsage)).SetMethod, null);
			gen.Emit(OpCodes.Ret);

			setRenderTargetUsage = getterMethod.CreateDelegate<Action<RenderTarget2D, RenderTargetUsage>>();
		}
		public void Unload() {
			setRenderTargetUsage = null;
		}
		static Action<RenderTarget2D, RenderTargetUsage> setRenderTargetUsage;
		public static void SetRenderTargetUsage(RenderTarget2D self, RenderTargetUsage renderTargetUsage) => setRenderTargetUsage(self, renderTargetUsage);
	}
}
