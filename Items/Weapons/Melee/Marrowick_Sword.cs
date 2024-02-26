using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Melee {
	public class Marrowick_Sword : ModItem {
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ShadewoodSword);
			Item.damage = 12;
			Item.knockBack = 5f;
			Item.value = Item.sellPrice(copper: 30);
		}
	}
}
