using Mono.Cecil;
using MonoMod.Cil;
using MonoMod.Utils;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Misc;
file class FixAiActionInLoading : ILoadable {
	public void Load(Mod mod) {
		DynamicMethodDefinition reader = new(typeof(NPCLoader).GetMethod("FinishSetup", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static));
		ILCursor c = new(new ILContext(reader.Definition));
		MethodReference _SetDefaults = default;
		c.GotoNext(i => i.MatchLdftn(out _SetDefaults));
		MethodBase SetDefaults = _SetDefaults.ResolveReflection();
		MonoModHooks.Modify(SetDefaults, 
			il => new ILCursor(il).EmitLdarg0().EmitLdfld(SetDefaults.DeclaringType.GetFields()[0]).EmitLdcI4(0).EmitStfld(typeof(NPC).GetField(nameof(NPC.aiAction)))
		);
	}
	public void Unload() {}
}
