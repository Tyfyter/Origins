using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public abstract class Brine_Leafed_Clover : ModItem {
		protected static sbyte faceSlot = -1;
		public abstract int Level { get; }
		public abstract int NextLowerTier { get; }
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Brine-Leaf Clover");
			Tooltip.SetDefault("Increases the likelihood of favorable outcomes based on how many leaves it has\nA leaf will fall out if it causes something exceptionally rare");
			SacrificeTotal = 1;
		}
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
			tooltips[0].Text += $" ({Level})";
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.brineClover = Level;
			originPlayer.brineCloverItem = Item;
		}
	}
	public class Brine_Leafed_Clover_0 : Brine_Leafed_Clover {
		public override int Level => 0;
		public override int NextLowerTier => 0;
	}
	public class Brine_Leafed_Clover_1 : Brine_Leafed_Clover {
		public override int Level => 1;
		public override int NextLowerTier => ModContent.ItemType<Brine_Leafed_Clover_0>();
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
