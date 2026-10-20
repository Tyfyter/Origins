using MonoMod.Cil;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Misc;
file class FixAiActionInLoading : ILoadable {
	public void Load(Mod mod) {
		IL_NPC.SetDefaults += il => new ILCursor(il).EmitLdarg0().EmitLdcI4(0).EmitStfld(typeof(NPC).GetField(nameof(NPC.aiAction)));
	}
	public void Unload() {}
}
