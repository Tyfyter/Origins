using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
    public class Eruption : ModItem {
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.SniperRifle);
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Item.damage = 50;
			Item.crit = 0;
			Item.useAnimation = 32;
			Item.useTime = 32;
			Item.useAmmo = ModContent.ItemType<Ammo.Resizable_Mine_One>();
			Item.shoot = ModContent.ProjectileType<Eruption_P>();
			Item.shootSpeed = 12;
			Item.reuseDelay = 6;
			Item.autoReuse = true;
			Item.value = Item.sellPrice(silver:50);
			Item.rare = ItemRarityID.Orange;
		}
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.HellstoneBar, 18);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			type = Item.shoot;
		}
	}
	public class Eruption_P : ModProjectile {
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ProximityMineI);
			Projectile.timeLeft = 600;
			Projectile.penetrate = 1;
			Projectile.aiStyle = 0;
			Projectile.width = 14;
			Projectile.height = 14;
			Projectile.hide = false;
			Projectile.localNPCHitCooldown = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.friendly = false;
		}
		public override void AI() {
			if (Projectile.owner == Main.myPlayer) {
				for (int i = 0; i < Main.maxNPCs; i++) {
					NPC npc = Main.npc[i];
					if (npc.CanBeChasedBy(Projectile) && npc.Hitbox.Intersects(Projectile.Hitbox)) {
						Projectile.NewProjectile(
							Projectile.GetSource_FromAI(),
							Projectile.Center,
							-Vector2.UnitY,
							ModContent.ProjectileType<Eruption_Geyser>(),
							Projectile.damage,
							Projectile.knockBack,
							Main.myPlayer
						);
						Projectile.penetrate--;
						break;
					}
				}
			}
			Projectile.velocity.Y += 0.3f;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (oldVelocity.X != Projectile.velocity.X) {
				Projectile.velocity.X *= -0.3f;
				Projectile.velocity.Y *= 0.9f;
			}
			if (oldVelocity.Y != Projectile.velocity.Y) {
				Projectile.velocity.X *= 0.9f;
				Projectile.velocity.Y *= -0.3f;
			}
			return false;
		}
	}
	public class Eruption_Geyser : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.GeyserTrap;
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.GeyserTrap);
			Projectile.trap = false;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
		}
	}
}
