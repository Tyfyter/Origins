using Origins.Buffs;
using Origins.Items.Other.Fish;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class Jam_Sandwich : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Jam Sandwich");
			Tooltip.SetDefault("Medium improvements to all stats \n'What kind of jam is this?'");
			SacrificeTotal = 5;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ShrimpPoBoy);
			Item.holdStyle = ItemHoldStyleID.None;
			Item.scale = 0.85f;
			Item.buffType = BuffID.WellFed2;
			Item.buffTime = 60 * 60 * 10;
			Item.rare = ItemRarityID.Orange;
		}
	}
}
