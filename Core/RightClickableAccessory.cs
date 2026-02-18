using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace Origins.Core {
	//TODO: remove, moved to PegasusLib
	class RightClickableAccessory : ILoadable {
		void ILoadable.Load(Mod mod) {
			On_ItemSlot.RightClick_ItemArray_int_int += On_ItemSlot_RightClick_ItemArray_int_int;
			On_ItemSlot.LeftClick_ItemArray_int_int += On_ItemSlot_LeftClick_ItemArray_int_int;
		}

		private void On_ItemSlot_LeftClick_ItemArray_int_int(On_ItemSlot.orig_LeftClick_ItemArray_int_int orig, Item[] inv, int context, int slot) {
			orig(inv, context, slot);
			if (Main.mouseRight && Main.mouseRightRelease) {
				switch (Math.Abs(context)) {
					case ItemSlot.Context.EquipArmorVanity:
					case ItemSlot.Context.EquipAccessoryVanity:
					break;
					default:
					ProcessRightClick(inv, context, slot);
					break;
				}
			}
		}

		private static void On_ItemSlot_RightClick_ItemArray_int_int(On_ItemSlot.orig_RightClick_ItemArray_int_int orig, Item[] inv, int context, int slot) {
			if (!Main.mouseRight || !Main.mouseRightRelease || ProcessRightClick(inv, context, slot)) {
				orig(inv, context, slot);
			}
		}
		static bool ProcessRightClick(Item[] inv, int context, int slot) {
			switch (Math.Abs(context)) {
				case ItemSlot.Context.EquipAccessory:
				case ItemSlot.Context.EquipAccessoryVanity:
				case ItemSlot.Context.DisplayDollAccessory:
				break;

				default:
				return true;
			}
			if (inv[slot]?.ModItem is not IRightClickableAccessory accessory || !accessory.CanRightClickAccessory(inv, context, slot)) {
				return true;
			}
			if (accessory.RightClickAccessory(inv, context, slot) && !NetmodeActive.SinglePlayer) {
				int baseSlot;
				switch (Main.LocalPlayer.CurrentLoadoutIndex) {
					default:
					baseSlot = PlayerItemSlotID.Loadout1_Armor_0;
					break;
					case 1:
					baseSlot = PlayerItemSlotID.Loadout2_Armor_0;
					break;
					case 2:
					baseSlot = PlayerItemSlotID.Loadout3_Armor_0;
					break;
				}
				NetMessage.SendData(MessageID.SyncEquipment, -1, -1, null, Main.myPlayer, baseSlot + slot);
			}
			return false;
		}
		void ILoadable.Unload() { }
	}
	public interface IRightClickableAccessory {
		/// <summary>
		/// If this returns false, the slot's normal right click behavior will apply instead of <see cref="RightClickAccessory"/>
		/// </summary>
		public bool CanRightClickAccessory(Item[] inv, int context, int slot) => !ItemSlot.ShiftInUse;
		/// <summary>
		/// If this returns true, the accessory will be synced to other players
		/// </summary>
		public bool RightClickAccessory(Item[] inv, int context, int slot);
	}
}
