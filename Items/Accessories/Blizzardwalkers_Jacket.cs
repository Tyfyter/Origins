﻿using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Back, EquipType.Front)]
	public class Blizzardwalkers_Jacket : ModItem, ICustomWikiStat {
		public const int max_active_time = 60 * 6;
		public string[] Categories => [
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 28);
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Blue;
			Item.master = true;
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.blizzardwalkerJacket = true;
			originPlayer.blizzardwalkerJacketVisual = !hideVisual;
		}
	}
}
