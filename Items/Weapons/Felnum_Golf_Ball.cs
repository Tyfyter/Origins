using Microsoft.Xna.Framework;
using Origins.CrossMod;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons {
	public class Felnum_Golf_Ball : ModItem {
		public override string Texture => "Terraria/Images/Item_" + ItemID.GolfBallDyedBrown;
		public override void SetStaticDefaults() {
			Origins.DamageBonusScale[Type] = 1.5f;
			CritType.SetCritType<Felnum_Crit_Type>(Type);
			OriginsSets.Items.FelnumItem[Type] = true;
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
        public override void AddRecipes() {
            Recipe.Create(Type)
            .AddIngredient(ModContent.ItemType<Felnum_Bar>())
            .AddTile(TileID.Anvils)
            .Register();
        }
	}
	public class Felnum_Golf_Ball_P : Golf_Ball_Projectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.GolfBallDyedBrown;
		public override void SetStaticDefaults() {
			ProjectileID.Sets.IsAGolfBall[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.GolfBallDyedBrown);
			Projectile.ignoreWater = true;
		}
		public override void AI() {
			Lighting.AddLight(Projectile.Center, new Vector3(0, 0.3375f, 1.275f) * (Projectile.velocity.Length() + 4) * 0.1f);
			if (Projectile.originalDamage == 0) Projectile.originalDamage = 20;
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.SourceDamage *= Projectile.velocity.Length() * 0.1667f;
		}
	}
}
