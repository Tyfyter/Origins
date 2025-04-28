using MonoMod.Cil;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Origins.Dusts {
	public class Deprioritized_Dust : ILoadable {
		public static int[] Set = new int[ChildSafety.SafeDust.Length];
		public void Load(Mod mod) {
			try {
				IL_Dust.NewDust += IL_Dust_NewDust;
			} catch (Exception e) {
				if (Origins.LogLoadingILError(nameof(IL_Dust_NewDust), e)) throw;
			}
		}
		void IL_Dust_NewDust(ILContext il) {
			ILCursor c = new(il);
			int loc = -1;
			c.GotoNext(MoveType.After,
				i => i.MatchLdloc(out loc),
				i => i.MatchLdfld<Dust>(nameof(Dust.active)),
				i => i.MatchBrtrue(out _)
			);
			c.Index--;
			c.EmitLdloc(loc);
			c.EmitLdarg3();
			c.EmitDelegate((bool active, Dust oldDust, int newType) => active && Set[oldDust.type] <= Set[newType]);
		}
		public void Unload() { }
	}
}
