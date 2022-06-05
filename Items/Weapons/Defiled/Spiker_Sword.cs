using Origins.Items.Materials;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Defiled {
	public class Spiker_Sword : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Spiker Sword");
			Tooltip.SetDefault("Very pointy");
		}
		public override void SetDefaults() {
			Item.damage = 30;
			Item.melee = true;
			Item.width = 42;
			Item.height = 50;
			Item.useTime = 28;
			Item.useAnimation = 28;
			Item.useStyle = 1;
			Item.knockBack = 7.5f;
			Item.value = 5000;
            Item.useTurn = true;
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1;
		}
		public override void AddRecipes() {
			Recipe recipe = Mod.CreateRecipe(Type);
			recipe.AddIngredient(ModContent.ItemType<Defiled_Bar>(), 10);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.Register();
			recipe = Mod.CreateRecipe(Type);
			recipe.AddIngredient(this);
			recipe.AddIngredient(ItemID.Muramasa);
			recipe.AddIngredient(ItemID.BladeofGrass);
			recipe.AddIngredient(ItemID.FieryGreatsword);
			recipe.AddTile(TileID.DemonAltar);
			recipe.SetResult(ItemID.NightsEdge);
			recipe.Register();
		}
	}
}
