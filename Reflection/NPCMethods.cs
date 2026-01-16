#pragma warning disable CS0649
#pragma warning disable IDE0044
using System;
using Terraria;
using PegasusLib.Reflection;
using DelegateMethods = PegasusLib.Reflection.DelegateMethods;

namespace Origins.Reflection {
	public class NPCMethods : ReflectionLoader {
		private delegate void BeHurtByOtherNPC_Del(int npcIndex, NPC thatNPC);
		[ReflectionParentType(typeof(NPC)), ReflectionMemberName("BeHurtByOtherNPC")]
		private static BeHurtByOtherNPC_Del _BeHurtByOtherNPC;
		[ReflectionParentType(typeof(NPC)), ReflectionMemberName("GetShimmered")]
		private static Action _GetShimmered;
		private delegate void AI_007_TownEntities_GetWalkPrediction_Del(int myTileX, int homeFloorX, bool canBreathUnderWater, bool currentlyDrowning, int tileX, int tileY, out bool keepwalking, out bool avoidFalling);
		[ReflectionParentType(typeof(NPC)), ReflectionMemberName("AI_007_TownEntities_GetWalkPrediction")]
		private static AI_007_TownEntities_GetWalkPrediction_Del _AI_007_TownEntities_GetWalkPrediction;
		public static void GetShimmered(NPC self) {
			DelegateMethods._target.SetValue(_GetShimmered, self);
			_GetShimmered();
		}
		public static void BeHurtByOtherNPC(NPC self, NPC other) {
			DelegateMethods._target.SetValue(_BeHurtByOtherNPC, self);
			_BeHurtByOtherNPC(other.whoAmI, other);
		}
		public static void AI_007_TownEntities_GetWalkPrediction(NPC self, int myTileX, int homeFloorX, bool canBreathUnderWater, bool currentlyDrowning, int tileX, int tileY, out bool keepwalking, out bool avoidFalling) {
			DelegateMethods._target.SetValue(_AI_007_TownEntities_GetWalkPrediction, self);
			_AI_007_TownEntities_GetWalkPrediction(myTileX, homeFloorX, canBreathUnderWater, currentlyDrowning, tileX, tileY, out keepwalking, out avoidFalling);
		}
	}
}