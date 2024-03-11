using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
namespace Origins.Items.Weapons.Melee {
	public class Marrowick_Sword : ModItem, ICustomWikiStat {
        public string[] Categories => new string[] {
            "Sword"
        };
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ShadewoodSword);
			Item.damage = 12;
			Item.knockBack = 5f;
			Item.value = Item.sellPrice(copper: 30);
		}
	}
}
