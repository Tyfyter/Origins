#pragma warning disable CS0649
#pragma warning disable IDE0044
using PegasusLib;
using PegasusLib.Reflection;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;

namespace Origins.Reflection {
	public class WiringMethods : ReflectionLoader {
		[ReflectionParentType(typeof(Wiring)), ReflectionMemberName("HitWireSingle")]
		static Action<int, int> _hitWireSingle;
		[ReflectionParentType(typeof(Wiring))]
		public static FastStaticFieldInfo<Dictionary<Point16, bool>> _wireSkip;
		public static void HitWireSingle(int i, int j) => _hitWireSingle(i, j);
	}
}