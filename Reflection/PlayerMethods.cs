#pragma warning disable CS0649
#pragma warning disable IDE0044
using Microsoft.Xna.Framework;
using System;
using Terraria;

namespace Origins.Reflection {
	public class PlayerMethods : ReflectionLoader {
		public override Type ParentType => GetType();
		private delegate void ApplyNPCOnHitEffects_Del(Item sItem, Rectangle itemRectangle, int damage, float knockBack, int npcIndex, int dmgRandomized, int dmgDone);
		[ReflectionParentType(typeof(Player)), ReflectionMemberName("ApplyNPCOnHitEffects")]
		private static ApplyNPCOnHitEffects_Del _ApplyNPCOnHitEffects;

		private delegate Item PickupItem_Del(int playerIndex, int worldItemArrayIndex, Item itemToPickUp);
		[ReflectionParentType(typeof(Player)), ReflectionMemberName("PickupItem")]
		private static PickupItem_Del _PickupItem;

		private delegate void PullItem_Common_Del(Item itemToPickUp, float xPullSpeed);
		[ReflectionParentType(typeof(Player)), ReflectionMemberName("PullItem_Common")]
		private static PullItem_Common_Del _PullItem_Common;

		private delegate void PullItem_Pickup_Del(Item itemToPickUp, float speed, int acc);
		[ReflectionParentType(typeof(Player)), ReflectionMemberName("PullItem_Pickup")]
		private static PullItem_Pickup_Del _PullItem_Pickup;

		private delegate void PullItem_ToVoidVault_Del(Item itemToPickUp);
		[ReflectionParentType(typeof(Player)), ReflectionMemberName("PullItem_ToVoidVault")]
		private static PullItem_ToVoidVault_Del _PullItem_ToVoidVault;
		//private delegate void GrabItems_Del(int playerIndex);
		//private static GrabItems_Del _GrabItems;
		public static void ApplyNPCOnHitEffects(Player player, Item sItem, Rectangle itemRectangle, int damage, float knockBack, int npcIndex, int dmgRandomized, int dmgDone) {
			Basic._target.SetValue(_ApplyNPCOnHitEffects, player);
			_ApplyNPCOnHitEffects(sItem, itemRectangle, damage, knockBack, npcIndex, dmgRandomized, dmgDone);
		}
		public static void PickupItem(Player player, int worldItemArrayIndex, Item itemToPickUp) {
			Basic._target.SetValue(_PickupItem, player);
			_PickupItem(player.whoAmI, worldItemArrayIndex, itemToPickUp);
		}
		public static void PullItem_Common(Player player, Item itemToPickUp, float xPullSpeed) {
			Basic._target.SetValue(_PullItem_Common, player);
			_PullItem_Common(itemToPickUp, xPullSpeed);
		}
		public static void PullItem_Pickup(Player player, Item itemToPickUp, float speed, int acc) {
			Basic._target.SetValue(_PullItem_Pickup, player);
			_PullItem_Pickup(itemToPickUp, speed, acc);
		}
		public static void PullItem_ToVoidVault(Player player, Item itemToPickUp) {
			Basic._target.SetValue(_PullItem_ToVoidVault, player);
			_PullItem_ToVoidVault(itemToPickUp);
		}
		/*public static void GrabItems(Player player) {
			Basic._target.SetValue(_ApplyNPCOnHitEffects, player);
			_GrabItems(player.whoAmI);
		}*/
	}
}