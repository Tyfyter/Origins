using MonoMod.Cil;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Core {
	public class FixNonMergingLiquidBombs : ILoadable {
		void ILoadable.Load(Mod mod) {
			IL_DelegateMethods.SpreadWater += AllowIfLiquid;
			IL_DelegateMethods.SpreadHoney += AllowIfLiquid;
			IL_DelegateMethods.SpreadLava += AllowIfLiquid;
		}
		static void AllowIfLiquid(ILContext context) {
			ILCursor c = new(context);
			if (!c.TryGotoNext(
				i => i.MatchLdcI4(1),
				i => i.MatchRet()
			)) return;
			if (!c.TryGotoNext(MoveType.After,
				i => i.MatchLdcI4(0),
				i => i.MatchRet()
			)) return;
			c.Index--;
			c.EmitPop();
			c.EmitLdarg0();
			c.EmitLdarg1();
			c.EmitDelegate(SucceedWithoutMerge);
		}
		public static bool SucceedWithoutMerge(int i, int j) => Main.tile[i, j].LiquidAmount > 0;
		void ILoadable.Unload() {}
	}
}
