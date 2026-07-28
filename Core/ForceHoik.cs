using MonoMod.Cil;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Core;
[ReinitializeDuringResizeArrays]
class ForceHoik : ILoadable {
	public static GetSlope[] ForceHoikDirection = TileID.Sets.Factory.CreateCustomSet<GetSlope>(null);
	public static Vector2 Position;
	public static int Width, Height;
	public delegate int GetSlope(int realSlope, Tile tile);
	void ILoadable.Load(Mod mod) {
		Origins.DoILEdit(Collision.SlopeCollision, Apply);
	}
	static void Apply(ILContext il) {
		ILCursor c = new(il);
		c.EmitLdarg0();
		c.EmitStsfld(GetField(nameof(Position)));
		c.EmitLdarg2();
		c.EmitStsfld(GetField(nameof(Width)));
		c.EmitLdarg3();
		c.EmitStsfld(GetField(nameof(Height)));
		c.GotoNext(i => i.MatchLdarg(5));
		int tile = -1;
		c.GotoNext(MoveType.After,
			i => i.MatchStloc(out tile),
			i => i.MatchLdloca(tile),
			i => i.MatchCall<Tile>("slope")
		);
		c.EmitLdloc(tile);
		c.EmitDelegate(static (int slope, Tile tile) => ForceHoikDirection[tile.TileType]?.Invoke(slope, tile) ?? slope);
	}
	void ILoadable.Unload() { }
	static FieldInfo GetField(string name) => typeof(ForceHoik).GetField(name);
}
