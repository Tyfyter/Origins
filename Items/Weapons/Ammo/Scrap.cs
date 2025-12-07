using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ammo {
	public class Scrap : ModItem {
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WoodenArrow);
			Item.maxStack = 9999;
			Item.damage = 0;
			Item.knockBack = 0;
			Item.shoot = ProjectileID.None;
			Item.shootSpeed = 0;
			Item.ammo = Type;
			Item.value = Item.sellPrice(copper: 65);
			Item.rare = ItemRarityID.Pink;
		}
	}
}
