using Microsoft.Xna.Framework;
using Origins.Dev;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Shield)]
	public class Grave_Danger : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat"
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(36, 42);
			Item.defense = 5;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().graveDanger = true;
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			if (Main.LocalPlayer.difficulty == 2) {
				for (int i = 0; i < tooltips.Count; i++) {
					if (tooltips[i].Name == "Tooltip0") {
						tooltips[i].Text = Language.GetTextValue("Mods.Origins.Doubles.GraveDangerHardDesc");
						tooltips[i].OverrideColor = Main.MouseTextColorReal.MultiplyRGBA(new Color(0.75f, 0.75f, 0.75f, 0.85f));
						break;
					}
				}
			}
		}
	}
}
