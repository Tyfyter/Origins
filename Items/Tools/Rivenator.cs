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
            item.CloneDefaults(ItemID.NightmarePickaxe);
			item.damage = 16;
			item.melee = true;
            item.pick = 80;
			item.width = 34;
			item.height = 32;
			item.useTime = 13;
			item.useAnimation = 24;
			item.knockBack = 4f;
			item.value = 3600;
			item.rare = ItemRarityID.Blue;
			item.UseSound = SoundID.Item1;
		}
		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ModContent.ItemType<Infested_Bar>(), 12);
			//recipe.AddIngredient(ModContent.ItemType<Riven_Sample>(), 6);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
