using Microsoft.Xna.Framework;
using Origins.Dusts;
using Origins.Tiles.Defiled;
using Origins.World.BiomeData;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
    public class White_Water : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("White Water");
			Tooltip.SetDefault("Spreads the {$Defiled_Wastelands} to some blocks");
			SacrificeTotal = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.UnholyWater);
			Item.shoot = ModContent.ProjectileType<White_Water_P>();
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 10);
			recipe.AddIngredient(ItemID.BottledWater, 10);
			recipe.AddIngredient(ModContent.ItemType<Defiled_Sand_Item>());
			recipe.AddIngredient(ModContent.ItemType<Defiled_Grass_Seeds>());
			recipe.Register();
		}
	}
	public class White_Water_P : ModProjectile {
		public override string Texture => base.Texture.Substring(0, base.Texture.Length - 2);
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("White Water");
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.UnholyWater);
		}
		public override void Kill(int timeLeft) {
			AltLibrary.Core.ALConvert.Convert<Defiled_Wastelands_Alt_Biome>((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16);

			SoundEngine.PlaySound(SoundID.Shatter, Projectile.position);
			for (int i = 0; i < 5; i++) {
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Glass);
			}
			for (int i = 0; i < 30; i++) {
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, White_Water_Dust.ID, 0f, -2f, 0, default(Color), 1.1f);
				dust.alpha = 100;
				dust.velocity.X *= 1.5f;
				dust.velocity *= 3f;
			}
		}
	}
}
