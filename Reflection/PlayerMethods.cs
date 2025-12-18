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
	public class PlayerMethods : ReflectionLoader {
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

		private delegate void ItemCheck_Shoot_Del(int i, Item sItem, int weaponDamage);
		[ReflectionParentType(typeof(Player)), ReflectionMemberName("ItemCheck_Shoot")]
		private static ItemCheck_Shoot_Del _ItemCheck_Shoot;

		private delegate void SetupPlayer_Del(Player player);
		[ReflectionParentType(typeof(PlayerLoader)), ReflectionMemberName("SetupPlayer")]
		private static SetupPlayer_Del _SetupPlayer;
		public static void SetupPlayer(Player player) => _SetupPlayer(player);
		//private delegate void GrabItems_Del(int playerIndex);
		//private static GrabItems_Del _GrabItems;
		public static void ApplyNPCOnHitEffects(Player player, Item sItem, Rectangle itemRectangle, int damage, float knockBack, int npcIndex, int dmgRandomized, int dmgDone) {
			DelegateMethods._target.SetValue(_ApplyNPCOnHitEffects, player);
			_ApplyNPCOnHitEffects(sItem, itemRectangle, damage, knockBack, npcIndex, dmgRandomized, dmgDone);
		}
		public static void PickupItem(Player player, int worldItemArrayIndex, Item itemToPickUp) {
			DelegateMethods._target.SetValue(_PickupItem, player);
			_PickupItem(player.whoAmI, worldItemArrayIndex, itemToPickUp);
		}
		public static void PullItem_Common(Player player, Item itemToPickUp, float xPullSpeed) {
			DelegateMethods._target.SetValue(_PullItem_Common, player);
			_PullItem_Common(itemToPickUp, xPullSpeed);
		}
		public static void PullItem_Pickup(Player player, Item itemToPickUp, float speed, int acc) {
			DelegateMethods._target.SetValue(_PullItem_Pickup, player);
			_PullItem_Pickup(itemToPickUp, speed, acc);
		}
		public static void PullItem_ToVoidVault(Player player, Item itemToPickUp) {
			DelegateMethods._target.SetValue(_PullItem_ToVoidVault, player);
			_PullItem_ToVoidVault(itemToPickUp);
		}
		public static void ItemCheck_Shoot(Player player, Item sItem, int weaponDamage) {
			DelegateMethods._target.SetValue(_ItemCheck_Shoot, player);
			_ItemCheck_Shoot(player.whoAmI, sItem, weaponDamage);
		}
		/*public static void GrabItems(Player player) {
			Basic._target.SetValue(_ApplyNPCOnHitEffects, player);
			_GrabItems(player.whoAmI);
		}*/
	}
}