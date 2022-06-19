using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using Origins.Items.Materials;

namespace Origins.Items.Tools {
	public class Pullinsmash : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Pullinsmash");
			Tooltip.SetDefault("'Brings the nails to you'");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.TitaniumWaraxe);
			Item.damage = 16;
			Item.DamageType = DamageClass.Melee;
            Item.hammer = 55;
			Item.width = 60;
			Item.height = 52;
			Item.useTime = 17;
			Item.useAnimation = 27;
			Item.knockBack = 4f;
			Item.value = 3600;
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1;
		}
		public override void AddRecipes() {
            Recipe recipe = Mod.CreateRecipe(Type);
            recipe.AddIngredient(ModContent.ItemType<Infested_Bar>(), 10);
			recipe.AddIngredient(ModContent.ItemType<Riven_Sample>(), 5);
			recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
	}
}
