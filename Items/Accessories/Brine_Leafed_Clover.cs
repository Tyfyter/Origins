using Origins.Dev;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[LegacyName("Brine_Leafed_Clover_0")]
	public class Brine_Leafed_Clover : ModItem, IItemObtainabilityProvider, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Misc
		];
		protected static sbyte faceSlot = -1;
		public virtual int Level { get; } = 0;
		public virtual int NextLowerTier { get; } = ItemID.None;
		public override LocalizedText DisplayName => Mod.GetLocalization($"{LocalizationCategory}.Brine_Leafed_Clover.DisplayName{(Level <= 0 ? "Stem" : string.Empty)}");
		public override LocalizedText Tooltip => Mod.GetLocalization($"{LocalizationCategory}.Brine_Leafed_Clover.Tooltip{(Level <= 0 ? "Stem" : string.Empty)}");

		public override void Load() {
			if (faceSlot == -1) faceSlot = (sbyte)EquipLoader.AddEquipTexture(Mod, "Origins/Items/Accessories/Brine_Leafed_Clover_Face", EquipType.Face, name: "Brine_Leafed_Clover_Face");
		}
		public override void Unload() {
			faceSlot = -1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(32, 28);
			Item.rare = ItemRarityID.LightRed;
			Item.value = (int)(5 * 156.25 * Math.Pow(2, Level) * Level);
			Item.faceSlot = faceSlot;
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			if (Level > 0) tooltips[0].Text += $" ({Level})";
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.brineClover = Level;
			originPlayer.brineCloverItem = Item;
		}
		public IEnumerable<int> ProvideItemObtainability() {
			yield return NextLowerTier;
		}
		public override bool CanAccessoryBeEquippedWith(Item equippedItem, Item incomingItem, Player player) => equippedItem.ModItem is not Brine_Leafed_Clover || incomingItem.ModItem is not Brine_Leafed_Clover;
	}
	public class Brine_Leafed_Clover_1 : Brine_Leafed_Clover {
		public override int Level => 1;
		public override int NextLowerTier => ModContent.ItemType<Brine_Leafed_Clover>();
	}
	public class Brine_Leafed_Clover_2 : Brine_Leafed_Clover {
		public override int Level => 2;
		public override int NextLowerTier => ModContent.ItemType<Brine_Leafed_Clover_1>();
	}
	public class Brine_Leafed_Clover_3 : Brine_Leafed_Clover {
		public override int Level => 3;
		public override int NextLowerTier => ModContent.ItemType<Brine_Leafed_Clover_2>();
	}
	public class Brine_Leafed_Clover_4 : Brine_Leafed_Clover {
		public override int Level => 4;
		public override int NextLowerTier => ModContent.ItemType<Brine_Leafed_Clover_3>();
	}
}
