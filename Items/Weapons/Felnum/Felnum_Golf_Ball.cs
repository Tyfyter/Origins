using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Felnum {
	public class Felnum_Golf_Ball : ModItem {
		public override string Texture => "Terraria/Images/Item_"+ItemID.GolfBallDyedBrown;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Felnum Golf Ball");
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.GolfBall);
			Item.damage = 20;
			Item.knockBack = 4;
			Item.DamageType = DamageClass.Generic;
			Item.noMelee = true;
			Item.shoot = ModContent.ProjectileType<Felnum_Golf_Ball_P>();
		}
		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
			damage = damage.MultiplyBonuses(1.5f);
		}
	}
	public class Felnum_Golf_Ball_P : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.GolfBallDyedBrown;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Felnum Golf Ball");
			ProjectileID.Sets.IsAGolfBall[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.GolfBallDyedBrown);
			Projectile.ignoreWater = true;
		}
		public override void AI() {
			Lighting.AddLight(Projectile.Center, new Vector3(0, 0.3375f, 1.275f) * (Projectile.velocity.Length() + 4) * 0.1f);
		}
		public override bool? CanDamage() {
			if (Projectile.velocity.LengthSquared() > 4 * 4) {
				return null;
			}
			return false;
		}
		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
			damage = (int)(damage * Projectile.velocity.Length() * 0.1667f);
		}
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			Vector2 hitbox = Projectile.Hitbox.Center.ToVector2();
			Vector2 intersect = Rectangle.Intersect(Projectile.Hitbox, target.Hitbox).Center.ToVector2();
			bool bounced = false;
			if (hitbox.X != intersect.X) {
				Projectile.velocity.X = -Projectile.velocity.X;
				bounced = true;
			}
			if (hitbox.Y != intersect.Y) {
				Projectile.velocity.Y = -Projectile.velocity.Y;
				bounced = true;
			}
			if (!bounced) {
				if (Math.Abs(Projectile.velocity.X) > Math.Abs(Projectile.velocity.Y)) {
					Projectile.velocity.X = -Projectile.velocity.X;
				} else if (Math.Abs(Projectile.velocity.Y) > Math.Abs(Projectile.velocity.X)) {
					Projectile.velocity.Y = -Projectile.velocity.Y;
				}
			}
		}
	}

}
