using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Core {
	public interface IMultiTypeMultiTile {
		public bool IsValidTile(Tile tile);
	}
	internal class MultiTypeMultiTile : ILoadable {
		public void Load(Mod mod) {
			try {
				MonoModHooks.Modify(typeof(TileLoader).GetMethod(nameof(TileLoader.CheckModTile)), IL_MultiTypeMultiTile);
			} catch (Exception e) {
				if (Origins.LogLoadingILError(nameof(IL_MultiTypeMultiTile), e)) throw;
			}
		}
		static void IL_MultiTypeMultiTile(ILContext il) {
			ILCursor c = new(il);
			ILLabel label = default;
			int loc = -1;
			//IL_0181: ldloca.s 18
			//IL_0183: call instance uint16 & Terraria.Tile::get_type()
			//IL_0188: ldind.u2
			//IL_0189: ldarg.2
			//IL_018a: beq.s IL_0191
			c.GotoNext(MoveType.AfterLabel,
				i => i.MatchLdloca(out loc),
				i => i.MatchCall<Tile>($"get_type"),
				i => i.MatchLdindU2(),
				i => i.MatchLdarg2(),
				i => i.MatchBeq(out label)
			);
			c.EmitLdarg2();
			c.EmitLdloc(loc);
			c.EmitDelegate((int tileType, Tile tile) => {
				return TileLoader.GetTile(tileType) is IMultiTypeMultiTile multiTypeMultiTile && multiTypeMultiTile.IsValidTile(tile);
			});
			c.EmitBrtrue(label);
		}
		public void Unload() { }
	}
}
