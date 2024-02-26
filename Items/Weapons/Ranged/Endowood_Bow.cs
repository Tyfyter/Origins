using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ranged {
    public class Endowood_Bow : ModItem {
        public string[] Categories => new string[] {
            "Bow"
        };
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ShadewoodBow);
			Item.damage = 8;
			Item.width = 24;
			Item.height = 56;
			Item.value = Item.sellPrice(copper: 20);
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(-3f, 0);
		}
	}
}
