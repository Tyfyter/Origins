#pragma warning disable CS0649
#pragma warning disable IDE0044
using System;
using Terraria;
using PegasusLib.Reflection;

namespace Origins.Reflection {
	public class WiringMethods : ReflectionLoader {
		[ReflectionParentType(typeof(Wiring)), ReflectionMemberName("HitWireSingle")]
		static Action<int, int> _hitWireSingle;
		public static void HitWireSingle(int i, int j) => _hitWireSingle(i, j);
	}
}