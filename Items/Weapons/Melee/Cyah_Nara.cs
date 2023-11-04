using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Melee {
	[LegacyName("Syah_Nara")]
	public class Cyah_Nara : ModItem {
		
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Katana);
			Item.damage = 18;
			Item.DamageType = DamageClass.Melee;
			Item.noUseGraphic = false;
			Item.noMelee = false;
			Item.width = 46;
			Item.height = 52;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 19;
			Item.useAnimation = 19;
			Item.knockBack = 0f;
			Item.shoot = ProjectileID.None;
			Item.value = Item.sellPrice(silver: 2);
			Item.rare = ItemRarityID.White;
			Item.autoReuse = true;
		}
	}
}
