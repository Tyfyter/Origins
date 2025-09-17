using CalamityMod.NPCs.TownNPCs;
using Microsoft.Xna.Framework;
using Origins.Tiles.Brine;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Impact_Golf_Ball : ModItem {
		public override string Texture => "Terraria/Images/Item_" + ItemID.GolfBallDyedBlack;
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.GolfBallDyedBlack);
			Item.damage = 30;
			Item.DamageType = DamageClasses.Explosive;
			Item.shoot = ModContent.ProjectileType<Impact_Golf_Ball_P>();
			Item.value = Item.sellPrice(gold: 1);
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Explosive_Golf_Ball>()
			.AddIngredient<Peat_Moss_Item>()
			.Register();
		}
	}
	public class Impact_Golf_Ball_P : Golf_Ball_Projectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.GolfBallDyedBlack;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			// DisplayName.SetDefault("Impact Golf Ball");
			Origins.ExplosiveBaseDamage[Type] = 30;
			Origins.DamageModOnHit[Type] = true;
		}
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.penetrate = 1;
			Projectile.DamageType = DamageClasses.Explosive;
		}
		public override void AI() {
			if ((Projectile.velocity - Projectile.oldVelocity).LengthSquared() > 8 * 8) {
				Projectile.Kill();
			}
			if (Projectile.originalDamage == 0) Projectile.originalDamage = Origins.ExplosiveBaseDamage[Type];
		}
		public override bool? CanDamage() {
			if (Projectile.velocity.LengthSquared() > 4) {
				return null;
			}
			return false;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (CanDamage() ?? true) Projectile.Kill();
			return true;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Projectile.Kill();
		}
		public override bool PreKill(int timeLeft) {
			Projectile.type = ProjectileID.RocketIII;
			return true;
		}
	}
	public class Explosive_Golf_Ball : ModItem {
		public override string Texture => "Terraria/Images/Item_" + ItemID.GolfBallDyedBlack;
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.GolfBallDyedBlack);
			Item.damage = 30;
			Item.DamageType = DamageClasses.Explosive;
			Item.shoot = ModContent.ProjectileType<Explosive_Golf_Ball_P>();
			Item.value = Item.sellPrice(gold: 1);
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddRecipeGroup(OriginSystem.GolfBallsRecipeGroupID)
			.AddIngredient(ItemID.ExplosivePowder)
			.Register();
		}
	}
	public class Explosive_Golf_Ball_P : Golf_Ball_Projectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.GolfBallDyedBlack;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			// DisplayName.SetDefault("Explosive Golf Ball");
			Origins.ExplosiveBaseDamage[Type] = 30;
			Origins.DamageModOnHit[Type] = true;
		}
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.penetrate = 1;
			Projectile.DamageType = DamageClasses.Explosive;
		}
		public override void AI() {
			if (Projectile.originalDamage == 0) Projectile.originalDamage = Origins.ExplosiveBaseDamage[Type];
		}
		public override bool? CanDamage() {
			if (Projectile.velocity.LengthSquared() > 4) {
				return null;
			}
			return false;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Projectile.Kill();
		}
		public override bool PreKill(int timeLeft) {
			Projectile.type = ProjectileID.RocketIII;
			return true;
		}
	}
	public class Bouncy_Explosive_Golf_Ball : ModItem {
		public override string Texture => "Terraria/Images/Item_" + ItemID.GolfBallDyedPink;
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.GolfBallDyedBlack);
			Item.damage = 20;
			Item.DamageType = DamageClasses.Explosive;
			Item.shoot = ModContent.ProjectileType<Bouncy_Explosive_Golf_Ball_P>();
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Explosive_Golf_Ball>()
			.AddIngredient(ItemID.PinkGel)
			.Register();
		}
	}
	public class Bouncy_Explosive_Golf_Ball_P : Golf_Ball_Projectile {
		int bounceTimer = 0;
		protected override bool CloneNewInstances => true;
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.GolfBallDyedPink;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			// DisplayName.SetDefault("Ricochet Golf Ball");
			Origins.ExplosiveBaseDamage[Type] = 20;
			Origins.DamageModOnHit[Type] = true;
		}
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.penetrate = 25;
			Projectile.DamageType = DamageClasses.Explosive;
		}
		public override void AI() {
			Vector2 diff = Projectile.velocity - Projectile.oldVelocity;
			if (diff.LengthSquared() > 8 * 8) {
				Projectile explosion = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ProjectileID.RocketI, Projectile.originalDamage, Projectile.knockBack, Projectile.owner);
				explosion.DamageType = Projectile.DamageType;
				explosion.timeLeft = 1;
				Projectile.velocity += Vector2.Normalize(diff);
				if (bounceTimer <= 0) {
					Projectile.penetrate--;
					bounceTimer = 10;
				}
			}
			if (bounceTimer > 0) {
				bounceTimer--;
			}
			if (Projectile.originalDamage == 0) Projectile.originalDamage = Origins.ExplosiveBaseDamage[Type];
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			base.OnHitNPC(target, hit, damageDone);
			Projectile.velocity += Projectile.velocity.SafeNormalize(default);
			Projectile explosion = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ProjectileID.RocketI, Projectile.originalDamage, Projectile.knockBack, Projectile.owner);
			explosion.DamageType = Projectile.DamageType;
			explosion.timeLeft = 1;
			if (bounceTimer <= 0) {
				Projectile.penetrate--;
				bounceTimer = 10;
			}
		}
	}
	public class Remote_Golf_Ball : ModItem {
		public override string Texture => "Terraria/Images/Item_" + ItemID.GolfBallDyedBlack;
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.GolfBallDyedBlack);
			Item.damage = 30;
			Item.DamageType = DamageClasses.Explosive;
			Item.shoot = ModContent.ProjectileType<Remote_Golf_Ball_P>();
			Item.value = Item.sellPrice(gold: 1);
		}
		public override bool AltFunctionUse(Player player) {
			return true;
		}
		public override bool CanShoot(Player player) {
			return player.altFunctionUse != 2;
		}
	}
	public class Remote_Golf_Ball_P : Golf_Ball_Projectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.GolfBallDyedBlack;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			// DisplayName.SetDefault("Remote Golf Ball");
			Origins.ExplosiveBaseDamage[Type] = 30;
			Origins.DamageModOnHit[Type] = true;
		}
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.penetrate = -1;
			Projectile.DamageType = DamageClasses.Explosive;
		}
		public override void AI() {
			Player owner = Main.player[Projectile.owner];
			if (owner.ItemAnimationActive && owner.altFunctionUse == 2 && owner.HeldItem.shoot == Type) {
				Projectile.type = ProjectileID.MiniNukeGrenadeI;
				Projectile.Kill();
			}
			if (Projectile.originalDamage == 0) Projectile.originalDamage = Origins.ExplosiveBaseDamage[Type];
		}
		public override bool? CanDamage() {
			if (Projectile.type == ProjectileID.MiniNukeGrenadeI) {
				return null;
			}
			return false;
		}
		public override bool PreKill(int timeLeft) {
			Projectile explosion = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ProjectileID.MiniNukeGrenadeI, Projectile.originalDamage, Projectile.knockBack, Projectile.owner);
			explosion.DamageType = Projectile.DamageType;
			explosion.timeLeft = 1;
			return true;
		}
	}
}
