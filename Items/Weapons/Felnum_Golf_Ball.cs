using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons {
	public class Felnum_Golf_Ball : ModItem {
		public override string Texture => "Terraria/Images/Item_" + ItemID.GolfBallDyedBrown;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Felnum Golf Ball");
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.GolfBall);
			Item.damage = 20;
			Item.knockBack = 4;
			Item.DamageType = DamageClass.Generic;
			Item.noMelee = true;
			Item.shoot = ModContent.ProjectileType<Felnum_Golf_Ball_P>();
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Green;
		}
		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
			damage = damage.Scale(1.5f);
		}
	}
	public class Felnum_Golf_Ball_P : Golf_Ball_Projectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.GolfBallDyedBrown;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Felnum Golf Ball");
			ProjectileID.Sets.IsAGolfBall[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.GolfBallDyedBrown);
			Projectile.ignoreWater = true;
		}
		public override void AI() {
			Lighting.AddLight(Projectile.Center, new Vector3(0, 0.3375f, 1.275f) * (Projectile.velocity.Length() + 4) * 0.1f);
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			damage = (int)(damage * Projectile.velocity.Length() * 0.1667f);
		}
	}
}
