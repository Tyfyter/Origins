using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Melee {
	public class Crystal_Cutters : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Crystal Cutters");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.TerraBlade);
			Item.damage = 44;
			Item.DamageType = DamageClass.Melee;
			Item.width = 34;
			Item.height = 34;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 39;
			Item.useAnimation = 13;
			Item.knockBack = 4f;
			//Item.shoot = ModContent.ProjectileType<Crystal_Cutters_P>();
			Item.shootSpeed = 7;
			Item.rare = ItemRarityID.LightRed;
			Item.autoReuse = true;
			Item.value = Item.buyPrice(gold: 1);
		}
	}
	/*public class Crystal_Cutters_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Melee/Crystal_Cutters_P";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Crystal Ball");
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.CrystalDart);
		}
	}*/
}
