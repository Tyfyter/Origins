using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ammo {
	public class Explosive_Harpoon : ModItem {
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Explosive Harpoon");
			SacrificeTotal = 99;
			ID = Type;
		}
		public override void SetDefaults() {
			Item.damage = 10;
			Item.DamageType = DamageClass.Ranged;
			Item.consumable = true;
			Item.maxStack = 99;
			Item.shoot = Explosive_Harpoon_P.ID;
			Item.ammo = Type;
			Item.value = Item.sellPrice(silver: 30);
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddRecipeGroup(RecipeGroupID.IronBar);
			recipe.AddIngredient(ModContent.ItemType<Peat_Moss>());
			recipe.AddTile(TileID.Anvils);
			recipe.Register();

			recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Harpoon>());
			recipe.AddIngredient(ModContent.ItemType<Peat_Moss>());
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
	public class Explosive_Harpoon_P : ModProjectile {
		//Now make it blow up
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Harpoon;
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Explosive Harpoon");
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Harpoon);
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}
		public override void OnSpawn(IEntitySource source) {
			if (Projectile.ai[1] == 1) {
				Projectile.penetrate = 2;
			}
		}
		public override void AI() {
			if (Projectile.ai[0] == 1 && Projectile.penetrate >= 0) {
				Projectile.aiStyle = 1;
				Projectile.velocity = Projectile.oldVelocity;
				Projectile.tileCollide = true;
				Vector2 diff = Main.player[Projectile.owner].itemLocation - Projectile.Center;
				SoundEngine.PlaySound(SoundID.Item10, Projectile.Center + diff / 2);
				float len = diff.Length() * 0.25f;
				diff /= len;
				Vector2 pos = Projectile.Center;
				for (int i = 0; i < len; i++) {
					Dust.NewDust(pos - new Vector2(2), 4, 4, DustID.Stone, Scale: 0.75f);
					pos += diff;
				}
			}
			if (Projectile.penetrate == 1) {
				Projectile.penetrate--;
			}
		}
	}
}
