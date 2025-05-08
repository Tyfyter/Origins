using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Origins.Items.Materials;

namespace Origins.Items.Weapons.Melee {
	public class Silicon_Sword : ModItem {
		public override void SetDefaults() {
			Item.damage = 18;
			Item.DamageType = DamageClass.Melee;
			Item.width = 42;
			Item.height = 42;
			Item.useTime = 16;
			Item.useAnimation = 16;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 2;
			Item.autoReuse = true;
			Item.useTurn = true;
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.Item1;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Silicon_Bar>(14)
			.AddCondition(OriginsModIntegrations.AprilFools)
			.Register();
		}
	}
}
