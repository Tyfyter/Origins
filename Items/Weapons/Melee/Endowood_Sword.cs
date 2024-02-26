using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Melee {
	public class Endowood_Sword : ModItem {
        public string[] Categories => new string[] {
            "Sword"
        };
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ShadewoodSword);
			Item.damage = 11;
			Item.knockBack = 8f;
			Item.value = Item.sellPrice(copper: 30);
		}
	}
}
