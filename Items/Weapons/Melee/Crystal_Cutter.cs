using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Melee {
	public class Crystal_Cutter : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Crystal Cutter");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.StylistKilLaKillScissorsIWish);
			Item.damage = 44;
			Item.DamageType = DamageClass.Melee;
			Item.width = 34;
			Item.height = 34;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 39;
			Item.useAnimation = 13;
			Item.knockBack = 4f;
			Item.shoot = ProjectileID.CrystalDart;//TODO: make unique sprite & projectile
			Item.shootSpeed = 7;
			Item.rare = ItemRarityID.LightRed;
			Item.autoReuse = true;
			Item.value = Item.buyPrice(gold: 1);
		}
	}
}
