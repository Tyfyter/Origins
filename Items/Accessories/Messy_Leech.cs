﻿using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Messy_Leech : ModItem, ICustomWikiStat {
		public string[] Categories => new string[] {
			"Combat"
		};
		public override void SetDefaults() {
			Item.DefaultToAccessory(24, 26);
			Item.value = Item.sellPrice(silver: 60);
			Item.rare = ItemRarityID.White;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().messyLeech = true;
		}
	}
}
