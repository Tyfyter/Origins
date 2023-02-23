using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Origins.Items.Weapons.Ammo;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
    public class Mine_Flayer : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Mine Flayer");
			Tooltip.SetDefault("Releases a barrage of mines when swung");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.TerraBlade);
			Item.damage = 40;
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Melee];
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 3;
			Item.useAnimation = 36;
			Item.knockBack = 4f;
			Item.useAmmo = ModContent.ItemType<Resizable_Mine>();
			Item.shoot = ModContent.ProjectileType<Resizable_Mine_P>();
			Item.shootSpeed = 9;
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.buyPrice(gold: 2);
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Busted_Servo>(), 30);
			recipe.AddIngredient(ModContent.ItemType<Power_Core>());
			recipe.AddIngredient(ModContent.ItemType<Rotor>(), 8);
			recipe.AddTile(TileID.MythrilAnvil); //Fabricator
			recipe.Register();
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			velocity = velocity.RotatedByRandom(0.15f);
		}
	}
}
