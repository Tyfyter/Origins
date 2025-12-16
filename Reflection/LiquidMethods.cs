using System;
using Terraria.Graphics.Shaders;
using PegasusLib;
using PegasusLib.Reflection;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria;

namespace Origins.Reflection {
	public class LiquidMethods : ReflectionLoader {
		private delegate void SettleWaterAt_Del(int tileX, int tileY);
		[ReflectionParentType(typeof(Liquid)), ReflectionMemberName("SettleWaterAt")]
#pragma warning disable CS0649 // Field is never assigned to
		private static SettleWaterAt_Del _SettleWaterAt;
#pragma warning restore CS0649 // Field is never assigned to
		public static void SettleWaterAt(int originX, int originY) {
			_SettleWaterAt(originX, originY);
		}
	}
}