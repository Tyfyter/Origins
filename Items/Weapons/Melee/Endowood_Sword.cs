using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
namespace Origins.Items.Weapons.Melee {
	public class Endowood_Sword : ModItem, ICustomWikiStat {
        public string[] Categories => [
            "Sword"
        ];
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ShadewoodSword);
			Item.damage = 11;
			Item.knockBack = 8f;
			Item.value = Item.sellPrice(copper: 30);
		}
	}
}
