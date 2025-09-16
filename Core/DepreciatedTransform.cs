using MonoMod.Cil;
using PegasusLib;
using System;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Core {
	[ReinitializeDuringResizeArrays]
	internal class DepreciatedTransform : ILoadable {
		public static int[] Set { get; } = ItemID.Sets.Factory.CreateIntSet(-1);
		public void Load(Mod mod) {
			try {
				IL_Item.SetDefaults_int_bool_ItemVariant += il => new ILCursor(il)
				.EmitLdarg1()
				.EmitCall(((Func<int, int>)DoTransform).Method)
				.EmitStarg(1);
			} catch (Exception e) {
				if (Origins.LogLoadingILError(nameof(DepreciatedTransform), e)) throw;
			}
		}
		static int DoTransform(int type) {
			int newType = Set.GetIfInRange(type, -1);
			if (newType != -1) return newType;
			return type;
		}
		public void Unload() {}
	}
}
