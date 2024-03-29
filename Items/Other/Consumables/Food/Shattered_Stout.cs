﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables.Food {
    public class Shattered_Stout : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 5;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.SmoothieofDarkness);
			Item.holdStyle = ItemHoldStyleID.None;
			Item.buffType = BuffID.WellFed;
			Item.buffTime = 60 * 60 * 20;
		}
	}
}
