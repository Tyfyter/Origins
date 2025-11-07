using Origins.Items.Accessories;
using Origins.UI;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Ticking_Dynamite : ModItem { 
		public static float DamageMult => 1 + ModContent.GetInstance<Ticking_Dynamite_UI>().TotalSeconds * 0.125f;
		public override void SetStaticDefaults() {
			ItemID.Sets.ItemsThatCountAsBombsForDemolitionistToSpawn[Type] = true;
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Dynamite);
			Item.damage = 120;
			Item.shootSpeed *= 1.5f;
			Item.value = 1000;
			Item.shoot = ModContent.ProjectileType<Ticking_Dynamite_P>();
			Item.ammo = ItemID.Dynamite;
			Item.rare = ItemRarityID.Orange;
		}
		public override void AddRecipes() {
			AddRecipe(6, ALRecipeGroups.CopperWatches);
			AddRecipe(15, ALRecipeGroups.SilverWatches);
			AddRecipe(30, ALRecipeGroups.GoldWatches);
			CreateRecipe(60)
			.AddIngredient(ItemID.Dynamite, 60)
			.AddIngredient<Eitrite_Watch>()
			.Register();
		}
		void AddRecipe(int yield, RecipeGroup group) =>
			CreateRecipe(yield)
			.AddIngredient(ItemID.Dynamite, yield)
			.AddRecipeGroup(group)
			.Register();
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			for (int i = 0; i < tooltips.Count; i++) {
				if (tooltips[i].Name == "Tooltip0") {
					tooltips[i].Text = string.Format(tooltips[i].Text, ModContent.GetInstance<Ticking_Dynamite_UI>().TotalSeconds, DamageMult);
					break;
				}
			}
		}
		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
			damage *= DamageMult;
		}
	}
	public class Ticking_Dynamite_UI() : Ticking_Explosives_UI(12, 3) {
		public override bool IsActive() => Main.LocalPlayer.HeldItem.ModItem is Ticking_Dynamite;
	}
	public class Ticking_Dynamite_P : ModProjectile {
		public override string Texture => typeof(Ticking_Dynamite).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 0;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Dynamite);
			Projectile.friendly = false;
			Projectile.timeLeft = 60 * 20;
			Projectile.appliesImmunityTimeOnSingleHits = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}
		public override void OnSpawn(IEntitySource source) {
			Projectile.ai[2] = (int)(60 * ModContent.GetInstance<Ticking_Dynamite_UI>().TotalSeconds);
		}
		public override void AI() {
			if (Projectile.ai[2] > 0) Projectile.timeLeft = 60;
			if (Projectile.timeLeft > 3 && --Projectile.ai[2] <= 0) Projectile.timeLeft = 3;
		}
		public override bool PreKill(int timeLeft) {
			Projectile.type = ProjectileID.Dynamite;
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
