using Origins.Items.Materials;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Tools {
	public class Rivenator : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Rivenator");
			Tooltip.SetDefault("'Unsettlingly floppy'\nAble to mine Hellstone");
		}
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.NightmarePickaxe);
			Item.damage = 16;
			Item.melee = true;
            Item.pick = 80;
			Item.width = 34;
			Item.height = 32;
			Item.useTime = 13;
			Item.useAnimation = 24;
			Item.knockBack = 4f;
			Item.value = 3600;
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1;
		}
		public override void AddRecipes() {
			Recipe recipe = Mod.CreateRecipe(Type);
			recipe.AddIngredient(ModContent.ItemType<Infested_Bar>(), 12);
			//recipe.AddIngredient(ModContent.ItemType<Riven_Sample>(), 6);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.Register();
		}
	}
}
