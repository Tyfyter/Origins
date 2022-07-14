using Origins.Buffs;
using Origins.Items.Other.Fish;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class Krunch_Mix : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Krunch Mix");
			Tooltip.SetDefault("Increased regeneration and minor improvements to all stats \n'And, that's why I love...'");
			SacrificeTotal = 5;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.PotatoChips);
			Item.holdStyle = ItemHoldStyleID.None;
			Item.scale = 0.85f;
			Item.buffType = BuffID.WellFed;
			Item.buffTime = 60 * 60 * 10;
			Item.rare = ItemRarityID.Orange;
		}
		public override bool? UseItem(Player player) {
			player.AddBuff(BuffID.Regeneration, Item.buffTime);
			return null;
		}
	}
}
