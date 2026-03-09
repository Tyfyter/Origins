using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Core {
	class ReplaceAITypeSounds : ILoadable {
		void ILoadable.Load(Mod mod) {
			try {
				IL_NPC.AI_003_Fighters += IL_NPC_AI_003_Fighters;
			} catch (Exception ex) {
				if (Origins.LogLoadingILError(GetType().Name, ex)) throw;
			}
		}
		void ILoadable.Unload() { }
		static void IL_NPC_AI_003_Fighters(ILContext il) {
			ILCursor c = new(il);
			ILLabel skip = default;
			c.GotoNext(MoveType.After,
				i => i.MatchLdarg0(),//IL_483f: ldarg.0
				i => i.MatchLdfld<NPC>(nameof(NPC.shimmerTransparency)),//IL_4840: ldfld float32 Terraria.NPC::shimmerTransparency
				i => i.MatchLdcR4(1),//IL_4845: ldc.r4 1
				i => i.MatchBgeUn(out skip)//IL_484a: bge.un IL_4c9d
			);
			c.EmitLdarg0();
			c.EmitDelegate<Predicate<NPC>>(npc => npc.ModNPC is IReplaceAITypeSounds replaceSounds && replaceSounds.PlaySound());
			c.EmitBrtrue(skip);
		}
	}
	public interface IReplaceAITypeSounds {
		/// <summary>
		/// Return true to skip the vanilla sound
		/// </summary>
		bool PlaySound();
	}
}
