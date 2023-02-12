using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
    public class Grenado : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Grenado");
			Tooltip.SetDefault("Uses grenades for ammo");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.RubyStaff);
			Item.damage = 80;
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Magic];
			Item.noMelee = true;
			Item.useTime = 26;
			Item.useAnimation = 26;
			Item.mana = 0;
			Item.shoot = ProjectileID.Grenade;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Yellow;
		}
	}
}
