using Origins.Items.Accessories;
using Origins.UI;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Ticking_Bomb : ModItem { 
		public static float DamageMult => 1 + ModContent.GetInstance<Ticking_Bomb_UI>().TotalSeconds * 0.175f;
		public override void SetStaticDefaults() {
			ItemID.Sets.ItemsThatCountAsBombsForDemolitionistToSpawn[Type] = true;
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Bomb);
			Item.damage = 70;
			Item.shootSpeed *= 1.5f;
			Item.value = 1000;
			Item.shoot = ModContent.ProjectileType<Ticking_Bomb_P>();
			Item.ammo = ItemID.Bomb;
			Item.rare = ItemRarityID.Orange;
		}
		public override void AddRecipes() {
			AddRecipe(10, ALRecipeGroups.CopperWatches);
			AddRecipe(25, ALRecipeGroups.SilverWatches);
			AddRecipe(50, ALRecipeGroups.GoldWatches);
			CreateRecipe(100)
			.AddIngredient(ItemID.Bomb, 100)
			.AddIngredient<Eitrite_Watch>()
			.Register();
		}
		void AddRecipe(int yield, RecipeGroup group) =>
			CreateRecipe(yield)
			.AddIngredient(ItemID.Bomb, yield)
			.AddRecipeGroup(group)
			.Register();
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			for (int i = 0; i < tooltips.Count; i++) {
				if (tooltips[i].Name == "Tooltip0") {
					tooltips[i].Text = string.Format(tooltips[i].Text, ModContent.GetInstance<Ticking_Bomb_UI>().TotalSeconds, DamageMult);
					break;
				}
			}
		}
		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
			damage *= DamageMult;
		}
	}
	public class Ticking_Bomb_UI() : Ticking_Explosives_UI(6, 2) {
		public override bool IsActive() => Main.LocalPlayer.HeldItem.ModItem is Ticking_Bomb;
	}
	public class Ticking_Bomb_P : ModProjectile {
		public override string Texture => typeof(Ticking_Bomb).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 0;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bomb);
			Projectile.friendly = false;
			Projectile.timeLeft = 60 * 20;
			Projectile.appliesImmunityTimeOnSingleHits = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}
		public override void OnSpawn(IEntitySource source) {
			Projectile.ai[2] = (int)(60 * ModContent.GetInstance<Ticking_Bomb_UI>().TotalSeconds);
		}
		public override void AI() {
			if (Projectile.ai[2] > 0) Projectile.timeLeft = 60;
			if (Projectile.timeLeft > 3 && --Projectile.ai[2] <= 0) Projectile.timeLeft = 3;
		}
		public override bool PreKill(int timeLeft) {
			Projectile.type = ProjectileID.Bomb;
			return true;
		}
		public override void OnKill(int timeLeft) {
			Projectile.friendly = true;
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 128;
			Projectile.height = 128;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.Damage();
		}
	}
}
