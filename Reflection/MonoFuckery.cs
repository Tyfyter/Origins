using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Reflection;
using Terraria.ModLoader;

namespace Origins.Reflection {
	public class MonoFuckery : ILoadable {
		public delegate void _ComputeStackDelta(Instruction instruction, ref int stack_size);
		public static _ComputeStackDelta ComputeStackDelta { get; private set; }
		public void Load(Mod mod) {
			ComputeStackDelta = typeof(Code).Assembly.GetType("Mono.Cecil.Cil.CodeWriter").GetMethod(nameof(ComputeStackDelta), BindingFlags.NonPublic | BindingFlags.Static).CreateDelegate<_ComputeStackDelta>();
		}
		public void Unload() {
			ComputeStackDelta = null;
		}
		public static int SkipPrevArgument(ILCursor c) {
			int count = 0;
			int delta = 0;
			do {
				count++;
				ComputeStackDelta(c.Prev, ref delta);
				c.Index--;
			} while (delta != 1);
			return count;
		}
		public static ILLabel DefineLabel(ILContext il, out ILLabel label) {
			return label = il.DefineLabel();
		}
	}
}