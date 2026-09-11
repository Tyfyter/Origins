using MonoMod.Cil;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Core;
class FixUpsideDownConveyors : ILoadable {
	void ILoadable.Load(Mod mod) {
		Origins.DoILEdit(Collision.StepConveyorBelt, static il => {
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

			c = new(il);
			c.GotoNext(MoveType.After, i => i.MatchLdcR4(0.0001f));
			c.EmitDelegate((float padding) => float.Max(padding, 0.01f));
		});
	}
	void ILoadable.Unload() { }
}
