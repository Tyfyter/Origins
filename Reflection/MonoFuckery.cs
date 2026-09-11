using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;
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
		public static int SkipPrevArgumentAlt(ILCursor c) {
			int count = 0;
			int delta = 0;
			do {
				count++;
				ComputePopDelta(c.Prev, ref delta);
				ComputePushDelta(c.Prev, ref delta);
				c.Index--;
			} while (delta != 1);
			return count;
		}
		static void ComputePopDelta(Instruction inst, ref int stack_size) {
			switch (inst.OpCode.StackBehaviourPop) {
				case StackBehaviour.Pop1:
				case StackBehaviour.Popi:
				case StackBehaviour.Popref:
				stack_size--;
				break;
				case StackBehaviour.Pop1_pop1:
				case StackBehaviour.Popi_pop1:
				case StackBehaviour.Popi_popi:
				case StackBehaviour.Popi_popi8:
				case StackBehaviour.Popi_popr4:
				case StackBehaviour.Popi_popr8:
				case StackBehaviour.Popref_pop1:
				case StackBehaviour.Popref_popi:
				stack_size -= 2;
				break;
				case StackBehaviour.Popi_popi_popi:
				case StackBehaviour.Popref_popi_popi:
				case StackBehaviour.Popref_popi_popi8:
				case StackBehaviour.Popref_popi_popr4:
				case StackBehaviour.Popref_popi_popr8:
				case StackBehaviour.Popref_popi_popref:
				stack_size -= 3;
				break;
				case StackBehaviour.PopAll:
				stack_size = 0;
				break;
				case StackBehaviour.Varpop:
				if (inst.Operand is MethodReference method) stack_size -= method.Parameters.Count;
				break;
			}
		}
		static void ComputePushDelta(Instruction inst, ref int stack_size) {
			switch (inst.OpCode.StackBehaviourPush) {
				case StackBehaviour.Push1:
				case StackBehaviour.Pushi:
				case StackBehaviour.Pushi8:
				case StackBehaviour.Pushr4:
				case StackBehaviour.Pushr8:
				case StackBehaviour.Pushref:
				stack_size++;
				break;
				case StackBehaviour.Push1_push1:
				stack_size += 2;
				break;
				case StackBehaviour.Varpush:
				if (inst.Operand is MethodReference method && !method.ReturnType.Is(typeof(void))) stack_size++;
				break;
			}
		}
		public static ILLabel DefineLabel(ILContext il, out ILLabel label) {
			return label = il.DefineLabel();
		}
	}
}