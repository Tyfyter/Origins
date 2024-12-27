#pragma warning disable CS0649
#pragma warning disable IDE0044
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using PegasusLib;
using PegasusLib.Reflection;
using DelegateMethods = PegasusLib.Reflection.DelegateMethods;

namespace Origins.Reflection {
	public class NPCMethods : ReflectionLoader {
		private delegate void BeHurtByOtherNPC_Del(int npcIndex, NPC thatNPC);
		[ReflectionParentType(typeof(NPC)), ReflectionMemberName("BeHurtByOtherNPC")]
		private static BeHurtByOtherNPC_Del _BeHurtByOtherNPC;
		public static void BeHurtByOtherNPC(NPC self, NPC other) {
			DelegateMethods._target.SetValue(_BeHurtByOtherNPC, self);
			_BeHurtByOtherNPC(other.whoAmI, other);
		}
	}
}