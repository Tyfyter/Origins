using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using PegasusLib;
using System;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Core {
	internal class NoBackfacesToCull : ILoadable {
		public void Load(Mod mod) {
			try {
				UnstableHooking.IL_Main_DoDraw += (il) => new ILCursor(il)
					.GotoNext(MoveType.Before, i => i.MatchLdsfld<Main>(nameof(Main.Rasterizer)))
					.EmitLdsfld(typeof(RasterizerState).GetField(nameof(RasterizerState.CullNone)))
					.EmitStsfld(typeof(Main).GetField(nameof(Main.Rasterizer)));
			} catch (Exception e) {
				if (Origins.LogLoadingILError(nameof(NoBackfacesToCull), e)) throw;
			}
		}
		public void Unload() { }
	}
}
