using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Melee {
	public class Baseball_Bat : ModItem {
		
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WoodenSword);
			Item.damage = 5;
			Item.knockBack = 8f;
			Item.value = Item.sellPrice(silver: 2);
		}
	}
}
