using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
    public class Nuclear_Arm : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Nuclear Arm");
			Tooltip.SetDefault("'Like nuclear arms? Well now you can have nuclear arms on your... arms'");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.TerraBlade);
			Item.damage = 44;
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Melee];
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 28;
			Item.useAnimation = 28;
			Item.knockBack = 4f;
			Item.shoot = ModContent.ProjectileType<Nuclear_Arm_P>();
			Item.shootSpeed = 5;
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.buyPrice(gold: 2);
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.ExplosivePowder, 10);
			recipe.AddIngredient(ModContent.ItemType<Busted_Servo>(), 20);
			recipe.AddIngredient(ModContent.ItemType<Power_Core>(), 2);
			recipe.AddTile(TileID.MythrilAnvil); //Fabricator
			recipe.Register();
		}
	}
	public class Nuclear_Arm_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Nuclear_Arm_P";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Nuclear Arm");
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.RocketI);
		}
	}
}
