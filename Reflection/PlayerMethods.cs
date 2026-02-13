#pragma warning disable CS0649
#pragma warning disable IDE0044
using PegasusLib.Reflection;
using System;
using Terraria;
using Terraria.ModLoader;
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

		private delegate void ItemCheck_GetMeleeHitbox_Del(Item sItem, Rectangle heldItemFrame, out bool dontAttack, out Rectangle itemRectangle);
		[ReflectionParentType(typeof(Player)), ReflectionMemberName("ItemCheck_GetMeleeHitbox")]
		private static ItemCheck_GetMeleeHitbox_Del _ItemCheck_GetMeleeHitbox;

		private delegate Rectangle ItemCheck_EmitUseVisuals_Del(Item sItem, Rectangle itemRectangle);
		[ReflectionParentType(typeof(Player)), ReflectionMemberName("ItemCheck_EmitUseVisuals")]
		private static ItemCheck_EmitUseVisuals_Del _ItemCheck_EmitUseVisuals;

		private delegate void ItemCheck_MeleeHitNPCs_Del(Item sItem, Rectangle itemRectangle, int originalDamage, float knockBack);
		[ReflectionParentType(typeof(Player)), ReflectionMemberName("ItemCheck_MeleeHitNPCs")]
		private static ItemCheck_MeleeHitNPCs_Del _ItemCheck_MeleeHitNPCs;

		private delegate void ItemCheck_MeleeHitPVP_Del(Item sItem, Rectangle itemRectangle, int originalDamage, float knockBack);
		[ReflectionParentType(typeof(Player)), ReflectionMemberName("ItemCheck_MeleeHitPVP")]
		private static ItemCheck_MeleeHitPVP_Del _ItemCheck_MeleeHitPVP;

		private delegate void ItemCheck_EmitHammushProjectiles_Del(int i, Item sItem, Rectangle itemRectangle, int damage);
		[ReflectionParentType(typeof(Player)), ReflectionMemberName("ItemCheck_EmitHammushProjectiles")]
		private static ItemCheck_EmitHammushProjectiles_Del _ItemCheck_EmitHammushProjectiles;

		private delegate void ProcessHitAgainstNPC_Del(Item sItem, Rectangle itemRectangle, int originalDamage, float knockBack, int npcIndex);
		[ReflectionParentType(typeof(Player)), ReflectionMemberName("ProcessHitAgainstNPC")]
		private static ProcessHitAgainstNPC_Del _ProcessHitAgainstNPC;

		private delegate bool IsBottomOfTreeTrunkNoRoots_Del(int x, int y);
		[ReflectionParentType(typeof(Player)), ReflectionMemberName("IsBottomOfTreeTrunkNoRoots")]
		private static IsBottomOfTreeTrunkNoRoots_Del _IsBottomOfTreeTrunkNoRoots;

		private delegate void ClearMiningCacheAt_Del(int x, int y, int hitTileCacheType);
		[ReflectionParentType(typeof(Player)), ReflectionMemberName("ClearMiningCacheAt")]
		private static ClearMiningCacheAt_Del _ClearMiningCacheAt;

		private delegate void TryReplantingTree_Del(int x, int y);
		[ReflectionParentType(typeof(Player)), ReflectionMemberName("TryReplantingTree")]
		private static TryReplantingTree_Del _TryReplantingTree;

		[ReflectionParentType(typeof(Player))]
		private static Action<int> set_ItemUsesThisAnimation;

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
		public static void ItemCheck_GetMeleeHitbox(Player player, Item sItem, Rectangle heldItemFrame, out bool dontAttack, out Rectangle itemRectangle) {
			DelegateMethods._target.SetValue(_ItemCheck_GetMeleeHitbox, player);
			_ItemCheck_GetMeleeHitbox(sItem, heldItemFrame, out dontAttack, out itemRectangle);
		}
		public static Rectangle ItemCheck_EmitUseVisuals(Player player, Item sItem, Rectangle itemRectangle) {
			DelegateMethods._target.SetValue(_ItemCheck_EmitUseVisuals, player);
			return _ItemCheck_EmitUseVisuals(sItem, itemRectangle);
		}
		public static void ItemCheck_MeleeHitNPCs(Player player, Item sItem, Rectangle itemRectangle, int damage, float knockBack) {
			DelegateMethods._target.SetValue(_ItemCheck_MeleeHitNPCs, player);
			_ItemCheck_MeleeHitNPCs(sItem, itemRectangle, damage, knockBack);
		}
		public static void ItemCheck_MeleeHitPVP(Player player, Item sItem, Rectangle itemRectangle, int damage, float knockBack) {
			DelegateMethods._target.SetValue(_ItemCheck_MeleeHitPVP, player);
			_ItemCheck_MeleeHitPVP(sItem, itemRectangle, damage, knockBack);
		}
		public static void ItemCheck_EmitHammushProjectiles(Player player, Item sItem, Rectangle itemRectangle, int damage) {
			DelegateMethods._target.SetValue(_ItemCheck_EmitHammushProjectiles, player);
			_ItemCheck_EmitHammushProjectiles(player.whoAmI, sItem, itemRectangle, damage);
		}
		public static void Set_ItemUsesThisAnimation(Player player, int value) {
			DelegateMethods._target.SetValue(set_ItemUsesThisAnimation, player);
			set_ItemUsesThisAnimation(value);
		}
		public static void ProcessHitAgainstNPC(Player player, Item sItem, Rectangle itemRectangle, int originalDamage, float knockBack, int npcIndex) {
			DelegateMethods._target.SetValue(_ProcessHitAgainstNPC, player);
			_ProcessHitAgainstNPC(sItem, itemRectangle, originalDamage, knockBack, npcIndex);
		}
		public static void ProcessHitAgainstAllNPCsNoCooldown(Player player, Item item, Rectangle itemRectangle, int weaponDamage, float knockBack) {
			DelegateMethods._target.SetValue(_ProcessHitAgainstNPC, player);
			foreach (NPC npc in Main.ActiveNPCs) {
				int attackCD = player.attackCD;
				int meleeNPCHitCooldown = player.meleeNPCHitCooldown[npc.whoAmI];
				npc.position += npc.netOffset;
				_ProcessHitAgainstNPC(item, itemRectangle, weaponDamage, knockBack, npc.whoAmI);
				npc.position -= npc.netOffset;
				player.attackCD = attackCD;
				player.meleeNPCHitCooldown[npc.whoAmI] = meleeNPCHitCooldown;
			}
		}
		public static bool IsBottomOfTreeTrunkNoRoots(int x, int y) {
			DelegateMethods._target.SetValue(_IsBottomOfTreeTrunkNoRoots, Main.CurrentPlayer);
			return _IsBottomOfTreeTrunkNoRoots(x, y);
		}
		public static void ClearMiningCacheAt(Player player, int x, int y, int hitTileCacheType) {
			DelegateMethods._target.SetValue(_ClearMiningCacheAt, player);
			_ClearMiningCacheAt(x, y, hitTileCacheType);
		}
		public static void TryReplantingTree(Player player, int x, int y) {
			DelegateMethods._target.SetValue(_TryReplantingTree, player);
			_TryReplantingTree(x, y);
		}
		/*public static void GrabItems(Player player) {
			Basic._target.SetValue(_ApplyNPCOnHitEffects, player);
			_GrabItems(player.whoAmI);
		}*/
	}
}