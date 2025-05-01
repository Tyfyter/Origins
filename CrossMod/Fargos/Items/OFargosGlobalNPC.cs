using Origins.Tiles.Other;
using System.Collections.Generic;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.CrossMod.Fargos.NPCs {
	[ExtendsFromMod(nameof(Fargowiltas))]
	public class OFargosGlobalItem : GlobalItem {
		private static string ExpandedTooltipLoc(string line) => Language.GetOrRegister($"Mods.Origins.CrossMod.ExpandedTooltips.{line}", () => line).Value;
		private TooltipLine FountainTooltip(string biome) {
			return new TooltipLine(Mod, "Tooltip0", $"[i:909] [c/AAAAAA:{ExpandedTooltipLoc($"Fountain{biome}")}]");
		}
		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
			int tipLine = tooltips.FindIndex((line) => line.Mod == nameof(Fargowiltas) && line.Name == "TooltipNPCSold") + 1;
			if (item.type == ItemType<Defiled_Fountain_Item>()) {
				tooltips.Insert(tipLine, FountainTooltip("Defiled"));
			}
			if (item.type == ItemType<Riven_Fountain_Item>()) {
				tooltips.Insert(tipLine, FountainTooltip("Riven"));
			}
			if (item.type == ItemType<Brine_Fountain_Item>()) {
				tooltips.Insert(tipLine, FountainTooltip("Brine"));
			}
		}
	}
}