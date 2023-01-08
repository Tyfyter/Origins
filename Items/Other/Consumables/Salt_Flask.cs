using Origins.Buffs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
    public class Salt_Flask : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Flask of Alkahest");
			Tooltip.SetDefault("Melee and whip attacks tenderize enemies");
			SacrificeTotal = 20;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.FlaskofCursedFlames);
			Item.buffType = Weapon_Imbue_Salt.ID;
			Item.buffTime = 60 * 60 * 20;
		}
	}
}
