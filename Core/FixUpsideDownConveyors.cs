using MonoMod.Cil;
using Origins.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Core;
class FixUpsideDownConveyors : ILoadable {
	void ILoadable.Load(Mod mod) {
		Origins.DoILEdit(Collision.StepConveyorBelt, FixPlayerHitbox);
		Origins.DoILEdit(Collision.StepConveyorBelt, FixBottomLine);
		Origins.DoILEdit(Collision.StepConveyorBelt, FixSlopeMovement);
	}
	static void FixPlayerHitbox(ILContext il) {
		ILCursor c = new(il);
		ILLabel label = default;
		c.GotoNext(MoveType.After,
			i => i.MatchIsinst<Player>(),
			i => i.MatchBrfalse(out label)
		);
		c.GotoNext(MoveType.After,
			i => i.MatchRet()
		);
		c.GotoNext(MoveType.Before,
			i => i.MatchLdarg0(),
			i => i.MatchLdflda<Entity>(nameof(Entity.position)),
			i => i.MatchLdflda<Vector2>(nameof(Vector2.Y))
		);
		c.MoveAfterLabels();
		c.EmitLdarg1();
		c.EmitDelegate((float gravDir) => gravDir < 0);
		c.EmitBrtrue(label);

		c.GotoNext(MoveType.After,
			i => i.MatchIsinst<Player>(),
			i => i.MatchBrfalse(out label)
		);
		c.GotoNext(MoveType.Before,
			i => i.MatchLdarg0(),
			i => i.MatchLdflda<Entity>(nameof(Entity.position)),
			i => i.MatchLdflda<Vector2>(nameof(Vector2.Y))
		);
		c.EmitLdarg1();
		c.EmitDelegate((float gravDir) => gravDir < 0);
		c.EmitBrtrue(label);
	}
	static void FixBottomLine(ILContext il) {
		ILCursor c = new(il);
		c.GotoNext(MoveType.After, i => i.MatchLdsfld(typeof(TileID.Sets), nameof(TileID.Sets.Platforms)));
		c.GotoNext(MoveType.Before, i => i.MatchCall<Collision>(nameof(Collision.CheckAABBvLineCollision2)));
		c.EmitDelegate(MoveBottomLine);
		MonoFuckery.SkipPrevArgumentAlt(c);
		c.EmitDelegate(MoveBottomLine);
	}
	static Vector2 MoveBottomLine(Vector2 bottomLine) => bottomLine + new Vector2(0, 1f);
	static void FixSlopeMovement(ILContext il) {
		ILCursor c = new(il);
		c.GotoNext(MoveType.After, i => i.MatchCall<Tile>("leftSlope"));
		c.GotoNext(MoveType.After, i => i.MatchLdarg1());
		c.EmitDelegate(IgnoreGravDir);
		c.GotoNext(MoveType.After, i => i.MatchLdarg1());
		c.EmitDelegate(IgnoreGravDir);
	}
	static float IgnoreGravDir(float gravDir) => 1;
	void ILoadable.Unload() { }
}
