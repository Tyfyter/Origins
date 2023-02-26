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
			Item.damage = 60;
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Melee];
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 4;
			Item.useAnimation = 36;
			Item.knockBack = 4f;
			Item.useAmmo = ModContent.ItemType<Resizable_Mine>();
			Item.shoot = ModContent.ProjectileType<Resizable_Mine_P>();
			Item.shootSpeed = 9;
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.buyPrice(gold: 2);
			Item.reuseDelay = 60;
			Item.autoReuse = false;
			Item.UseSound = null;
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
			velocity = OriginExtensions.Vec2FromPolar(player.direction == 1 ? player.itemRotation : player.itemRotation + MathHelper.Pi, velocity.Length());
			Terraria.Audio.SoundEngine.PlaySound(SoundID.Item61.WithPitch(0.25f), position);
		}
	}
}
