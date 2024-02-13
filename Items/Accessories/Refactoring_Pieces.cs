﻿using Origins.Dev;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Refactoring_Pieces : ModItem, ICustomWikiStat {
		public string[] Categories => new string[] {
			"Combat"
		};
		public override void SetStaticDefaults() {
			Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(11, 6));
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(30, 32);
			Item.rare = ItemRarityID.Expert;
			Item.expert = true;
			Item.value = Item.sellPrice(gold: 2);
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().refactoringPieces = true;
		}
	}
}