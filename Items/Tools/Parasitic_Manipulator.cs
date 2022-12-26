using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Tools {
    public class Parasitic_Manipulator : ModItem {

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Parasitic Manipulator");
			SacrificeTotal = 1;
		}

		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ReinforcedFishingPole);
			Item.damage = 0;
			Item.DamageType = DamageClass.Generic;
			Item.noMelee = true;
			Item.fishingPole = 27;
			Item.shootSpeed = 14f;
			Item.shoot = ModContent.ProjectileType<Parasitic_Bobber>();
			Item.rare = ItemRarityID.Blue;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Infested_Bar>(), 8);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
	public class Parasitic_Bobber : ModProjectile {

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Parasitic Bobber");
		}

		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.BobberReinforced);
			DrawOriginOffsetY = -8;
			Projectile.friendly = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}
		public override void AI() {
			if (Projectile.ai[0] == 1) {
				Projectile.usesLocalNPCImmunity = false;
				Rectangle biggerHitbox = Projectile.Hitbox;
				biggerHitbox.Inflate(14, 14);
				for (int i = 0; i < Main.maxItems; i++) {
					Item item = Main.item[i];
					if (item.active && biggerHitbox.Intersects(item.Hitbox)) {
						item.velocity = Projectile.velocity;
					}
				}
			}
		}
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			if (Projectile.ai[0] == 1) {
				target.velocity = Vector2.Lerp(target.velocity, Projectile.velocity, target.knockBackResist);
			}
		}
	}
}