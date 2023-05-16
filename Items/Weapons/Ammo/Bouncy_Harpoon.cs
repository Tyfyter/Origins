using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ammo {
    public class Bouncy_Harpoon : ModItem {
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Bouncy Harpoon");
			SacrificeTotal = 99;
			ID = Type;
		}
		public override void SetDefaults() {
			Item.damage = 10;
			Item.DamageType = DamageClass.Ranged;
			Item.consumable = true;
			Item.maxStack = 99;
			Item.shoot = Bouncy_Harpoon_P.ID;
			Item.ammo = Harpoon.ID;
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Blue;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 8);
			recipe.AddRecipeGroup(RecipeGroupID.IronBar, 8);
			recipe.AddIngredient(ItemID.PinkGel);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();

			recipe = Recipe.Create(Type, 8);
			recipe.AddIngredient(ModContent.ItemType<Harpoon>(), 8);
			recipe.AddIngredient(ItemID.PinkGel);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
	public class Bouncy_Harpoon_P : Harpoon_P {
		public static new int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Bouncy Harpoon");
			ID = Type;
		}
		public override void OnSpawn(IEntitySource source) {
			Projectile.penetrate = 17;
			if (Projectile.ai[1] == 1) {
				Projectile.maxPenetrate = 1;
			}
		}
		public override void AI() {
			if (Projectile.ai[0] == 1 && Projectile.maxPenetrate == 1) {
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
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			Projectile.penetrate = Projectile.maxPenetrate == 1 ? 2 : -1;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (--Projectile.penetrate <= 1) return true;
			Projectile.velocity = oldVelocity *
				new Vector2(Projectile.velocity.X == oldVelocity.X ? 1 : -1, Projectile.velocity.Y == oldVelocity.Y ? 1 : -1);
			return false;
		}
	}
}
