using Humanizer;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using ThoriumMod.NPCs;

namespace Origins.Core {
	//TODO: move to PegasusLib
	class StackableAccessory : GlobalItem {
		static LocalizedText Tooltip; 
		public override void Load() {
			On_ItemSlot.PickItemMovementAction += On_ItemSlot_PickItemMovementAction;
			Tooltip = Mod.GetLocalization($"{Name}.{nameof(Tooltip)}");
		}
		public static bool AppliesToContext(int context) => Math.Abs(context) switch {
			ItemSlot.Context.EquipArmor => true,
			ItemSlot.Context.EquipAccessory => true,
			ItemSlot.Context.EquipArmorVanity => true,
			ItemSlot.Context.EquipAccessoryVanity => true,
			ItemSlot.Context.DisplayDollArmor => true,
			ItemSlot.Context.DisplayDollAccessory => true,
			ItemSlot.Context.EquipDye => true,
			ItemSlot.Context.EquipMiscDye => true,
			ItemSlot.Context.HatRackDye => true,
			ItemSlot.Context.DisplayDollDye => true,
			_ => false
		};
		static int On_ItemSlot_PickItemMovementAction(On_ItemSlot.orig_PickItemMovementAction orig, Item[] inv, int context, int slot, Item checkItem) {
			if (AppliesToContext(context) && checkItem?.ModItem is IStackableAccessory stackable && stackable.CanStackInSlot(inv, slot, context)) return 0;
			return orig(inv, context, slot, checkItem);
		}
		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
			if (!AppliesToContext(item.tooltipContext) || item?.ModItem is not IStackableAccessory stackable || !stackable.CanStackInSlot([], -1, item.tooltipContext)) return;
			for (int i = tooltips.Count - 1; i >= 0; i--) {
				if (tooltips[i].Name == "SocialDesc" || tooltips[i].Name.StartsWith("Tooltip")) {
					tooltips.Add(new(Mod, "Stackable", Tooltip.Value));
					break;
				}
			}
		}
	}
	public interface IStackableAccessory {
		bool CanStackInSlot(Item[] inv, int slot, int context) => true;
	}
}
